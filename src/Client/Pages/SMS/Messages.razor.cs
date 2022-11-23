using Mapster;
using Microsoft.AspNetCore.Components;
using MudBlazor.Charts;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS;
public partial class Messages
{
    [Parameter]
    public string Recepient { get; set; } = string.Empty;
    [Inject]
    protected IMessageInsClient Client { get; set; } = default!;
    protected EntityServerTableContext<MessageInDto, int, MessageInUpdateRequest> Context { get; set; } = default!;

    private EntityTable<MessageInDto, int, MessageInUpdateRequest> _table = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Message",
            entityNamePlural: "Messages",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.ReceiveTime, "Date/Time", "ReceiveTime"),
                new(data => data.MessageText, "Message", "MessageText"),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<MessageInSearchRequest>();
                filter.Keyword = Recepient;
                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<MessageInDto>>();
            },
            //searchFunc: async filter => (await Client
            //    .SearchAsync(filter.Adapt<MessageInSearchRequest>()))
            //    .Adapt<PaginationResponse<MessageInDto>>(),
            exportAction: string.Empty);
}