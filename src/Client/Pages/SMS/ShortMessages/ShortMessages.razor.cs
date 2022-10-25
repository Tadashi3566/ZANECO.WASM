using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS.ShortMessages;
public partial class ShortMessages
{
    [Inject]
    protected IShortMessagesClient Client { get; set; } = default!;

    protected EntityServerTableContext<ShortMessageDto, Guid, ShortMessageUpdateRequest> Context { get; set; } = default!;

    private EntityTable<ShortMessageDto, Guid, ShortMessageUpdateRequest> _table = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "ShortMessage",
            entityNamePlural: "ShortMessages",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.MessageType, "Message Type", "MessageType"),
                new(data => data.Sender, "Sender", "Sender"),
                new(data => data.Receiver, "Receiver", "Receiver"),
                new(data => data.Message, "Message", "Message"),
                new(data => data.Status, "Status", "Status"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: true,
            idFunc: ShortMessage => ShortMessage.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<ShortMessageSearchRequest>()))
                .Adapt<PaginationResponse<ShortMessageDto>>(),
            createFunc: async ShortMessage => await Client.CreateAsync(ShortMessage.Adapt<ShortMessageCreateRequest>()),
            updateFunc: async (id, ShortMessage) => await Client.UpdateAsync(id, ShortMessage),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}