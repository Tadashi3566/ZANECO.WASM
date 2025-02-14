﻿using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.Multitenancy;

public partial class Tenants
{
    [Inject]
    private ITenantsClient TenantsClient { get; set; } = default!;

    private string? _searchString;
    protected EntityClientTableContext<TenantDetail, Guid, CreateTenantRequest> Context { get; set; } = default!;
    private List<TenantDetail> _tenants = new();
    public EntityTable<TenantDetail, Guid, CreateTenantRequest> EntityTable { get; set; } = default!;

    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    private ClientPreference _preference = new();

    private bool _canUpgrade;
    private bool _canModify;

    protected override async Task OnInitializedAsync()
    {
        Context = new(
            entityName: L["Tenant"],
            entityNamePlural: L["Tenants"],
            entityResource: FSHResource.Tenants,
            searchAction: FSHAction.View,
            deleteAction: string.Empty,
            updateAction: string.Empty,
            fields: new()
            {
                new(tenant => tenant.Id, L["Id"]),
                new(tenant => tenant.Name, L["Name"]),
                new(tenant => tenant.AdminEmail, L["Admin Email"]),
                new(tenant => tenant.ValidUpto, L["Valid Upto"], Type: typeof(DateTime)),
                new(tenant => tenant.IsActive, L["Active"], Type: typeof(bool))
            },
            loadDataFunc: async () => _tenants = (await TenantsClient.GetListAsync()).Adapt<List<TenantDetail>>(),
            searchFunc: (searchString, tenantDto) =>
                string.IsNullOrWhiteSpace(searchString)
                    || tenantDto.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase),
            createFunc: tenant => TenantsClient.CreateAsync(tenant.Adapt<CreateTenantRequest>()),
            hasExtraActionsFunc: () => true,
            exportAction: string.Empty);

        var state = await AuthState;
        _canUpgrade = await AuthService.HasPermissionAsync(state.User, FSHAction.UpgradeSubscription, FSHResource.Tenants);
        _canModify = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.Tenants);
    }

    private void ViewTenantDetails(string id)
    {
        var tenant = _tenants.First(f => f.Id == id);
        tenant.ShowDetails = !tenant.ShowDetails;
        foreach (var otherTenants in _tenants.Except(new[] { tenant }))
        {
            otherTenants.ShowDetails = false;
        }
    }

    private async Task ViewUpgradeSubscriptionModalAsync(string id)
    {
        var tenant = _tenants.First(f => f.Id == id);
        var parameters = new DialogParameters
        {
            {
                nameof(UpgradeSubscriptionModal.Request),
                new UpgradeSubscriptionRequest
                {
                    TenantId = tenant.Id,
                    ExtendedExpiryDate = tenant.ValidUpto
                }
            }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<UpgradeSubscriptionModal>(L["Upgrade Subscription"], parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            await EntityTable.ReloadDataAsync();
        }
    }

    private async Task DeactivateTenantAsync(string id)
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(() => TenantsClient.DeactivateAsync(id),
            Snackbar,
            successMessage: L["Tenant Deactivated."]) is not null)
        {
            await EntityTable.ReloadDataAsync();
        }
    }

    private async Task ActivateTenantAsync(string id)
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(() => TenantsClient.ActivateAsync(id),
            Snackbar,
            successMessage: L["Tenant Activated."]) is not null)
        {
            await EntityTable.ReloadDataAsync();
        }
    }

    public class TenantDetail : TenantDto
    {
        public bool ShowDetails { get; set; }
    }
}