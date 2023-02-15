using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.CAD.PowerConsumptions;
public partial class PowerConsumptions
{
    [Inject]
    protected IPowerConsumptionsClient Client { get; set; } = default!;

    protected EntityServerTableContext<PowerConsumptionDto, Guid, PowerConsumptionUpdateRequest> Context { get; set; } = default!;

    private EntityTable<PowerConsumptionDto, Guid, PowerConsumptionUpdateRequest>? _table;

    private string? _searchString;
    protected override void OnInitialized() =>
        Context = new(
            entityName: "Power Consumption",
            entityNamePlural: "Power Consumptions",
            entityResource: FSHResource.CAD,
            fields: new()
            {
                new(data => data.GroupCode, "Code", "Code"),
                new(data => data.GroupName, "Name", "Name"),
                new(data => data.BillMonth, "BillMonth", "BillMonth"),
                new(data => data.KwhPurchased, "KWH Purchased", "KWHPurchased", typeof(decimal)),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", "Notes", visible: false),
            },
            enableAdvancedSearch: true,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<PowerConsumptionSearchRequest>()))
                .Adapt<PaginationResponse<PowerConsumptionDto>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<PowerConsumptionCreateRequest>()),
            updateFunc: async (id, PowerConsumption) => await Client.UpdateAsync(id, PowerConsumption.Adapt<PowerConsumptionUpdateRequest>()),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);

    // Advanced Search
    // private string _searchBillMonth;
    // private string SearchBillMonth
    // {
    //    get => _searchBillMonth;
    //    set
    //    {
    //        _searchBillMonth = value;
    //        _ = _table.ReloadDataAsync();
    //    }
    // }

    private static async Task<IEnumerable<string>> SearchBillMonth(string value)
    {
        string[] billMonths = CadFunctions.BillMonths();

        // if text is null or empty, show complete list
        if (string.IsNullOrEmpty(value))
            return billMonths;
        return billMonths.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }

}