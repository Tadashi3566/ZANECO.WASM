using Mapster;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Components.Services;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS.MessageTemplates;
public partial class MessageTemplates
{
    [Inject]
    protected IMessageTemplatesClient Client { get; set; } = default!;
    [Inject]
    protected IMessageTemplatesClient MessageTemplate { get; set; } = default!;
    [Inject]
    protected IMessageOutsClient MessageOut { get; set; } = default!;
    [Inject]
    private IClipboardService? ClipboardService { get; set; }

    protected EntityServerTableContext<MessageTemplateDetail, int, MessageTemplateUpdateRequest> Context { get; set; } = default!;

    private EntityTable<MessageTemplateDetail, int, MessageTemplateUpdateRequest> _table = default!;

    private MessageOutCreateRequest _messageOut = new();

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Message Template",
            entityNamePlural: "Message Templates",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.TemplateType, "Template Type", "TemplateType"),
                new(data => data.IsAPI, "API", "IsAPI", typeof(bool)),
                new(data => data.IsFastMode, "Multiple", "isFastMode", typeof(bool)),
                new(data => data.Subject, "Subject", "Subject"),
                new(data => data.Message, "Message", "Message"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: true,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<MessageTemplateSearchRequest>()))
                .Adapt<PaginationResponse<MessageTemplateDetail>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<MessageTemplateCreateRequest>()),
            updateFunc: async (id, data) => await Client.UpdateAsync(id, data),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);

    private async Task MessageTemplateCopy(string message)
    {
        await ClipboardService!.CopyToClipboard(message);

        Snackbar.Add("The Template Message was copied to Clipboard", Severity.Success);
    }

    private async Task MessageTemplateDuplicate(MessageTemplateDto dto)
    {
        string transactionContent = $"Are you sure you want to duplicate this Message Template?";
        DialogParameters parameters = new()
        {
            { nameof(TransactionConfirmation.TransactionTitle), "Duplicate Message Template" },
            { nameof(TransactionConfirmation.ContentText), transactionContent },
            { nameof(TransactionConfirmation.ConfirmText), "Duplicate" }
        };
        DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        IDialogReference dialog = DialogService.Show<TransactionConfirmation>("Duplicate", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            MessageTemplateCreateRequest newMessageTemplate = new()
            {
                TemplateType = dto.TemplateType,
                MessageType = dto.MessageType,
                IsAPI = dto.IsAPI,
                IsMultiple = dto.IsMultiple,
                Recepients = dto.Recepients,
                Subject = dto.Subject,
                Message = dto.Message,
                Description = dto.Description!,
                Notes = dto.Notes!,
            };

            await ApiHelper.ExecuteCallGuardedAsync(() => MessageTemplate.CreateAsync(newMessageTemplate), Snackbar, successMessage: "Message Template successfully duplicated.");

            await _table.ReloadDataAsync();
        }
    }

    private async void SendSMS(MessageTemplateDetail request)
    {
        string transactionContent = $"Are you sure you want to send SMS to {ClassSMS.RecepientCount(request.Recepients):N0} recepient(s)?";
        DialogParameters parameters = new()
        {
            { nameof(TransactionConfirmation.ContentText), transactionContent },
            { nameof(TransactionConfirmation.ConfirmText), "Send" }
        };
        DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        IDialogReference dialog = DialogService.Show<TransactionConfirmation>("Send", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {
            _messageOut.IsAPI = request.IsAPI;
            _messageOut.MessageType = request.MessageType;
            _messageOut.MessageTo = request.Recepients;
            _messageOut.MessageText = request.Message;
            _messageOut.Description = request.Subject;

            await ApiHelper.ExecuteCallGuardedAsync(() => MessageOut.CreateAsync(_messageOut), Snackbar, successMessage: "Messages successfully created and sent to queue.");
        }
    }

    public class MessageTemplateDetail : MessageTemplateDto
    {
        public bool ShowRecepients { get; set; }
    }
}