using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.Surveys;

public partial class CommentComponent
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthenticationService AuthService { get; set; } = default!;
    [Inject]
    protected IRatingsClient Client { get; set; } = default!;
    [Inject]
    protected SweetAlertService? Swal { get; set; }

    private readonly RatingCreateRequest _rating = new();

    private CustomValidation? _customValidation;

    private string _rateName = "Excellent";

    protected override void OnInitialized()
    {
        _rating.RateNumber = 5;
    }

    private void SetRateName(int rateNumber)
    {
        switch (rateNumber)
        {
            case 1:
                _rateName = "Poor";
                break;

            case 2:
                _rateName = "Fair";
                break;

            case 3:
                _rateName = "Good";
                break;

            case 4:
                _rateName = "Very Good";
                break;

            case 5:
                _rateName = "Excellent";
                break;
        }
    }

    private async Task Submit()
    {
        string transactionTitle = "Submit Comment";
        string transactionContent = $"Are you sure you want to submit this comment?";
        var parameters = new DialogParameters
        {
            { nameof(TransactionConfirmation.TransactionIcon), Icons.Material.Filled.Send },
            { nameof(TransactionConfirmation.TransactionTitle), transactionTitle },
            { nameof(TransactionConfirmation.ContentText), transactionContent },
            { nameof(TransactionConfirmation.ConfirmText), "Submit" }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<TransactionConfirmation>(transactionTitle, parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            await ApiHelper.ExecuteCallGuardedAsync(() => Client.CreateAsync(_rating), Snackbar, _customValidation, "Your comment was successfully submitted.");

            await Swal!.FireAsync("Success", "Your comment was successfully submitted.", SweetAlertIcon.Success);

            _rating.Comment = string.Empty;
        }
    }
}