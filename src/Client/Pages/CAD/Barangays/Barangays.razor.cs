using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.CAD.Barangays;
public partial class Barangays
{
    [Inject]
    protected IBarangaysClient Client { get; set; } = default!;

    protected EntityServerTableContext<BarangayDto, Guid, BarangayUpdateRequest> Context { get; set; } = default!;

    private EntityTable<BarangayDto, Guid, BarangayUpdateRequest>? _table;

    private string? _searchString;
    protected override void OnInitialized() =>
        Context = new(
            entityName: "Barangay",
            entityNamePlural: "Barangays",
            entityResource: FSHResource.CAD,
            fields: new()
            {
                new(data => data.AreaName, "Area", "AreaName"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<BarangaySearchRequest>()))
                .Adapt<PaginationResponse<BarangayDto>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<BarangayCreateRequest>()),
            updateFunc: async (id, Barangay) => await Client.UpdateAsync(id, Barangay.Adapt<BarangayUpdateRequest>()),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}