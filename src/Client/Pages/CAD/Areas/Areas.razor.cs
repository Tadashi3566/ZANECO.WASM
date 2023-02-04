using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.CAD.Areas;
public partial class Areas
{
    [Inject]
    protected IAreasClient Client { get; set; } = default!;

    protected EntityServerTableContext<AreaDto, Guid, AreaUpdateRequest> Context { get; set; } = default!;

    private EntityTable<AreaDto, Guid, AreaUpdateRequest> _table = default!;

    private string? _searchString;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Area",
            entityNamePlural: "Areas",
            entityResource: FSHResource.CAD,
            fields: new()
            {
                new(data => data.Number, "Number", "Number"),
                new(data => data.Code, "Code", "Code"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Description, "Description", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<AreaSearchRequest>()))
                .Adapt<PaginationResponse<AreaDto>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<AreaCreateRequest>()),
            updateFunc: async (id, Area) => await Client.UpdateAsync(id, Area.Adapt<AreaUpdateRequest>()),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}