using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.Surveys;
public partial class Rates
{
    [Inject]
    protected IRatesClient Client { get; set; } = default!;

    protected EntityServerTableContext<RateDto, Guid, RateUpdateRequest> Context { get; set; } = default!;

    private EntityTable<RateDto, Guid, RateUpdateRequest> _table = default!;

    protected override async Task OnInitializedAsync() =>
        Context = new(
            entityName: "Rate",
            entityNamePlural: "Rates",
            entityResource: FSHResource.Rating,
            fields: new()
            {
                new(data => data.Id, "Id", "Id"),
                new(data => data.Number, "Number", "Number"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            idFunc: Rate => Rate.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<RateSearchRequest>()))
                .Adapt<PaginationResponse<RateDto>>(),
            createFunc: async Rate => await Client.CreateAsync(Rate.Adapt<RateCreateRequest>()),
            updateFunc: async (id, Rate) => await Client.UpdateAsync(id, Rate),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}