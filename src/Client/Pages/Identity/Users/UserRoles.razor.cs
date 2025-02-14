﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.Identity.Users;

public partial class UserRoles
{
    [Parameter]
    public string? Id { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    [Inject]
    protected IUsersClient UsersClient { get; set; } = default!;

    private ClientPreference _preference = new();

    private List<UserRoleDto> _userRolesList = default!;

    private string _title = string.Empty;
    private string _description = string.Empty;

    private string _searchString = string.Empty;

    private bool _canEditUsers;
    private bool _canSearchRoles;
    private bool _loaded;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canEditUsers = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.Users);
        _canSearchRoles = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.UserRoles);

        if (await ApiHelper.ExecuteCallGuardedAsync(() => UsersClient.GetByIdAsync(Id),
            Snackbar
        ) is UserDetailsDto dto)
        {
            _title = $"{dto.FirstName} {dto.LastName}";
            _description = string.Format(L["Manage {0} {1}'s Roles"], dto.FirstName, dto.LastName);

            if (await ApiHelper.ExecuteCallGuardedAsync(() => UsersClient.GetRolesAsync(dto.Id.ToString()),
                Snackbar
            ) is ICollection<UserRoleDto> response)
            {
                _userRolesList = response.ToList();
            }
        }

        _loaded = true;
    }

    private async Task SaveAsync()
    {
        var request = new UserRolesRequest()
        {
            UserRoles = _userRolesList
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => UsersClient.AssignRolesAsync(Id, request),
                Snackbar,
            successMessage: L["Updated User Roles."]) is not null)
        {
            Navigation.NavigateTo("/users");
        }
    }

    private bool Search(UserRoleDto userRole) => string.IsNullOrWhiteSpace(_searchString) || userRole.RoleName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) is true;
}