﻿using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.MultiTenancy;

namespace ZANECO.WASM.Client.Pages.Authentication;

public partial class SelfRegister
{
    [Inject]
    private IUsersClient Client { get; set; } = default!;

    private readonly CreateUserRequest _createUserRequest = new();

    private CustomValidation? _customValidation;
    private bool BusySubmitting { get; set; }

    private string Tenant { get; set; } = MultitenancyConstants.Root.Id;

    private bool _passwordVisibility;

    private InputType _passwordInput = InputType.Password;

    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

    private ClientPreference _preference = new();

    private async Task SubmitAsync()
    {
        BusySubmitting = true;

        string? sucessMessage = await ApiHelper.ExecuteCallGuardedAsync(() => Client.SelfRegisterAsync(Tenant, _createUserRequest), Snackbar, _customValidation);

        if (sucessMessage != null)
        {
            Snackbar.Add(sucessMessage, Severity.Info);
            //Navigation.NavigateTo("/login");
        }

        BusySubmitting = false;
    }

    private void TogglePasswordVisibility()
    {
        if (_passwordVisibility)
        {
            _passwordVisibility = false;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
            _passwordInput = InputType.Password;
        }
        else
        {
            _passwordVisibility = true;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
            _passwordInput = InputType.Text;
        }
    }
}