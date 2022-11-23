﻿using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS;
public partial class Messages
{
    [Parameter]
    public string Recepient { get; set; } = string.Empty;
    [Inject]
    protected IMessageInsClient ClientIn { get; set; } = default!;
    [Inject]
    protected IMessageLogsClient ClientLog { get; set; } = default!;
    protected EntityServerTableContext<MessageInDto, int, MessageInUpdateRequest> ContextIn { get; set; } = default!;
    protected EntityServerTableContext<MessageLogDto, int, MessageLogUpdateRequest> ContextLog { get; set; } = default!;

    private EntityTable<MessageInDto, int, MessageInUpdateRequest> _tableIn = default!;

    private EntityTable<MessageLogDto, int, MessageLogUpdateRequest> _tableLog = default!;

    protected override void OnInitialized()
    {
        ContextIn = new(
            entityName: "Inbox",
            entityNamePlural: "Inbox",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.ReceiveTime, "Date/Time", "ReceiveTime"),
                new(data => data.MessageText, "Message", "MessageText"),
            },
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<MessageInSearchRequest>();
                filter.Keyword = Recepient;
                var result = await ClientIn.SearchAsync(filter);
                return result.Adapt<PaginationResponse<MessageInDto>>();
            },
            updateFunc: async (id, data) => await ClientIn.UpdateAsync(id, data),
            exportAction: string.Empty);

        ContextLog = new(
            entityName: "Outbox",
            entityNamePlural: "Outbox",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.SendTime, "Date/Time", "SendTime"),
                new(data => data.MessageText, "Message", "MessageText"),
            },
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<MessageLogSearchRequest>();
                filter.Keyword = Recepient;
                var result = await ClientLog.SearchAsync(filter);
                return result.Adapt<PaginationResponse<MessageLogDto>>();
            },
            updateFunc: async (id, data) => await ClientLog.UpdateAsync(id, data),
            exportAction: string.Empty);
    }
}