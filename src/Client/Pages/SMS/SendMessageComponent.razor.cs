using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.SMS;

public partial class SendMessageComponent
{
    [Parameter]
    public string Recepients { get; set; } = string.Empty;
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthenticationService AuthService { get; set; } = default!;
    [Inject]
    protected IMessageOutsClient MessageClient { get; set; } = default!;

    private readonly MessageOutCreateRequest _model = new();

    private CustomValidation? _customValidation;

    protected override async Task OnParametersSetAsync()
    {
        if (Recepients != null)
        {
            _model.MessageTo = Recepients;
        }
    }

    private async Task Send()
    {
        string transactionContent = $"Are you sure you want to send SMS to {ClassSMS.RecepientCount(_model.MessageTo):N0} recepient(s)?";
        var parameters = new DialogParameters
        {
            { nameof(TransactionConfirmation.ContentText), transactionContent }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<TransactionConfirmation>("Send", parameters, options);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            await ApiHelper.ExecuteCallGuardedAsync(() => MessageClient.CreateAsync(_model), Snackbar, _customValidation,
        "Message successfully sent to the recepient(s).");
        }
    }
}