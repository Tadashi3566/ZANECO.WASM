﻿using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Components.EntityTable;
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

    protected EntityServerTableContext<MessageTemplateDto, int, MessageTemplateUpdateRequest> Context { get; set; } = default!;

    private EntityTable<MessageTemplateDto, int, MessageTemplateUpdateRequest> _table = default!;

    private MessageOutCreateRequest _messageOut = new();

    protected override void OnInitialized() =>
        Context = new(
            entityName: "MessageTemplate",
            entityNamePlural: "MessageTemplates",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.TemplateType, "Template Type", "TemplateType"),
                new(data => data.IsAPI, "API", "IsAPI", typeof(bool)),
                new(data => data.Subject, "Subject", "Subject"),
                new(data => data.Message, "Message", "Message"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: true,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<MessageTemplateSearchRequest>()))
                .Adapt<PaginationResponse<MessageTemplateDto>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<MessageTemplateCreateRequest>()),
            updateFunc: async (id, data) => await Client.UpdateAsync(id, data),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);

    private async void SendSMS(MessageTemplateDto request)
    {
        string recepients = ClassSMS.RemoveWhiteSpaces(request.Recepients);
        string[] recepientArray = recepients.Split(',');
        recepientArray = ClassSMS.GetDistinctFromArray(recepientArray);
        foreach (string recepient in recepientArray)
        {
            _messageOut.IsAPI = request.IsAPI;
            _messageOut.MessageType = request.MessageType;
            _messageOut.MessageTo = recepient;
            _messageOut.MessageText = request.Message;

            await ApiHelper.ExecuteCallGuardedAsync(() => MessageOut.CreateAsync(_messageOut), Snackbar, successMessage: "SMS successfully created and sent to queue.");
        }
    }
}
