using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.CAD.Routes;
public partial class Routes
{
    [Inject]
    protected IRoutesClient Client { get; set; } = default!;

    protected EntityServerTableContext<RouteDto, Guid, RouteUpdateRequest> Context { get; set; } = default!;

    private EntityTable<RouteDto, Guid, RouteUpdateRequest> _table = default!;

    protected override async Task OnInitializedAsync() =>
        Context = new(
            entityName: "Route",
            entityNamePlural: "Routes",
            entityResource: FSHResource.CAD,
            fields: new()
            {
                new(data => data.AreaName, "Area", "AreaName"),
                new(data => data.Number, "Number", "Number"),
                new(data => data.Code, "Code", "Code"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<RouteSearchRequest>()))
                .Adapt<PaginationResponse<RouteDto>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<RouteCreateRequest>()),
            updateFunc: async (id, Route) => await Client.UpdateAsync(id, Route.Adapt<RouteUpdateRequest>()),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}