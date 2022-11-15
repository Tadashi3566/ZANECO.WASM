using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS.MessageIns;
public partial class MessageIns
{
    [Inject]
    protected IMessageInsClient Client { get; set; } = default!;

    protected EntityServerTableContext<MessageInDto, int, MessageInUpdateRequest> Context { get; set; } = default!;

    private EntityTable<MessageInDto, int, MessageInUpdateRequest> _table = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "MessageIn",
            entityNamePlural: "MessageIns",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.Id, "Id", "Id"),
                new(data => data.ReceiveTime, "Receive Date/Time", "ReceiveTime", Template: TemplateReceivedTime),
                new(data => data.MessageFrom, "Sender/Receiver", "MessageFrom", Template: TemplateSenderReceiver),
                new(data => data.MessageText, "Message", "MessageText"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: true,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<MessageInSearchRequest>()))
                .Adapt<PaginationResponse<MessageInDto>>(),

            // createFunc: async data => await Client.CreateAsync(data.Adapt<MessageInCreateRequest>()),
            updateFunc: async (id, data) => await Client.UpdateAsync(id, data),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}
