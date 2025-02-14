﻿using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.MultiTenancy;

namespace ZANECO.WASM.Client.Pages.Authentication;

public partial class ForgotPassword
{
    private readonly ForgotPasswordRequest _forgotPasswordRequest = new();
    private CustomValidation? _customValidation;
    private bool BusySubmitting { get; set; }

    [Inject]
    private IUsersClient UsersClient { get; set; } = default!;

    private string Tenant { get; set; } = MultitenancyConstants.Root.Id;

    private ClientPreference _preference = new();

    private async Task SubmitAsync()
    {
        BusySubmitting = true;

        await ApiHelper.ExecuteCallGuardedAsync(
            () => UsersClient.ForgotPasswordAsync(Tenant, _forgotPasswordRequest), Snackbar, _customValidation);

        BusySubmitting = false;
    }
}