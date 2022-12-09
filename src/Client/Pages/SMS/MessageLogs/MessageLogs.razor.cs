using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS.MessageLogs;
public partial class MessageLogs
{
    [Inject]
    protected IMessageLogsClient Client { get; set; } = default!;

    protected EntityServerTableContext<MessageLogDto, int, MessageLogUpdateRequest> Context { get; set; } = default!;

    private EntityTable<MessageLogDto, int, MessageLogUpdateRequest> _table = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Message Log",
            entityNamePlural: "Message Logs",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                //new(data => data.Id, "Id", "Id"),
                new(data => data.SendTime, "Send Date/Time", "SendTime", Template: TemplateReceivedTime),
                new(data => data.MessageFrom, "Sender/Receiver", "MessageFrom", Template: TemplateSenderReceiver),
                new(data => data.MessageText, "Message", "MessageText"),
                new(data => data.StatusText, "Status", "StatusCode", Template: TemplateStatus),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: true,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<MessageLogSearchRequest>()))
                .Adapt<PaginationResponse<MessageLogDto>>(),
            updateFunc: async (id, data) => await Client.UpdateAsync(id, data),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}