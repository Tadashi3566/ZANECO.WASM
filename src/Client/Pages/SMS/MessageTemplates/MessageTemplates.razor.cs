using Mapster;
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
                new(data => data.IsMultiple, "Multiple", "IsMultiple", typeof(bool)),
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

    //private void ViewRecepients(int id)
    //{
    //    var template = Client.SearchAsync(new MessageTemplateSearchRequest());
    //    template.ShowRecepients = !template.ShowRecepients;
    //    foreach (var otherTemplates in _templates.Except(new[] { template }))
    //    {
    //        otherTemplates.ShowRecepients = false;
    //    }
    //}

    private async void CopyMessage(string message)
    {
        await ClipboardService!.CopyToClipboard(message);

        Snackbar.Add("The Template Message was copied to Clipboard", Severity.Success);
    }

    private async void SendSMS(MessageTemplateDetail request)
    {
        string transactionContent = $"Are you sure you want to send SMS to {ClassSMS.RecepientCount(request.Recepients):N0} recepient(s)?";
        DialogParameters parameters = new()
        {
            { nameof(TransactionConfirmation.ContentText), transactionContent }
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