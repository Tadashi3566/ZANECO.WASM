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
    public string Recipients { get; set; } = string.Empty;
    [Parameter]
    public bool DisplayRecipients { get; set; } = true;
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthenticationService AuthService { get; set; } = default!;
    [Inject]
    protected IMessageOutsClient MessageClient { get; set; } = default!;

    private readonly MessageOutCreateRequest _model = new();

    private CustomValidation? _customValidation;

    protected override void OnParametersSet()
    {
        _model.MessageType = "sms.automatic";

        if (Recipients != null)
        {
            _model.MessageTo = Recipients;
        }
    }

    private async Task Send()
    {
        string transactionTitle = "Send Message";
        string transactionContent = $"Are you sure you want to {transactionTitle} to {ClassSms.RecipientCount(_model.MessageTo):N0} Recipient(s)?";
        var parameters = new DialogParameters
        {
            { nameof(TransactionConfirmation.TransactionIcon), Icons.Material.Filled.Send },
            { nameof(TransactionConfirmation.TransactionTitle), transactionTitle },
            { nameof(TransactionConfirmation.ContentText), transactionContent },
            { nameof(TransactionConfirmation.ConfirmText), "Send" }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<TransactionConfirmation>(transactionTitle, parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            await ApiHelper.ExecuteCallGuardedAsync(() => MessageClient.CreateAsync(_model),
                Snackbar,
                _customValidation, "Message successfully sent to the Recipient(s).");
        }
    }
}