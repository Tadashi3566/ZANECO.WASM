using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.CAD.PowerRates;
public partial class PowerRates
{
    [Inject]
    protected IPowerRatesClient Client { get; set; } = default!;

    protected EntityServerTableContext<PowerRateDto, Guid, PowerRateUpdateRequest> Context { get; set; } = default!;

    private EntityTable<PowerRateDto, Guid, PowerRateUpdateRequest>? _table;

    private string? _searchString;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "PowerRate",
            entityNamePlural: "PowerRates",
            entityResource: FSHResource.CAD,
            fields: new()
            {
                new(data => data.Code, "Code", "Code"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, visible: false),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<PowerRateSearchRequest>()))
                .Adapt<PaginationResponse<PowerRateDto>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<PowerRateCreateRequest>()),
            updateFunc: async (id, PowerRate) => await Client.UpdateAsync(id, PowerRate.Adapt<PowerRateUpdateRequest>()),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}