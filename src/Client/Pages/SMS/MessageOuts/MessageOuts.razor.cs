using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS.MessageOuts;
public partial class MessageOuts
{
    [Inject]
    protected IMessageOutsClient Client { get; set; } = default!;
    protected EntityServerTableContext<MessageOutDto, int, MessageOutCreateRequest> Context { get; set; } = default!;

    private EntityTable<MessageOutDto, int, MessageOutCreateRequest>? _table;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Message",
            entityNamePlural: "Messages",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.IsAPI, "API", "IsAPI", typeof(bool)),
                new(data => data.MessageType, "Type", "MessageType"),
                new(data => data.MessageTo, "Recepient", "MessageTo"),
                new(data => data.MessageText, "Message", "MessageText"),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<MessageOutSearchRequest>()))
                .Adapt<PaginationResponse<MessageOutDto>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<MessageOutCreateRequest>()),
            exportAction: string.Empty);
}
