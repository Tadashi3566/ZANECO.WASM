using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.HR.PayrollManager.EmployeeAdjustments;
public partial class EmployeeAdjustments
{
    [Parameter]
    public Guid EmployeeId { get; set; } = default!;
    [Inject]
    protected IEmployeeAdjustmentsClient Client { get; set; } = default!;

    protected EntityServerTableContext<EmployeeAdjustmentDto, Guid, EmployeeAdjustmentUpdateRequest> Context { get; set; } = default!;

    private EntityTable<EmployeeAdjustmentDto, Guid, EmployeeAdjustmentUpdateRequest> _table = default!;

    protected override void OnParametersSet()
    {
        if (EmployeeId != Guid.Empty)
        {
            _searchEmployeeId = EmployeeId;
        }
    }

    protected override async Task OnInitializedAsync() =>
        Context = new(
            entityName: "Employee Adjustment",
            entityNamePlural: "Employee Adjustments",
            entityResource: FSHResource.EmployeeAdjustments,
            fields: new()
            {
                new(data => data.EmployeeName, "Employee", "Employee"),
                new(data => data.AdjustmentType, "Type", "AdjustmentType"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Amount.ToString("N2"), "Amount", "Amount"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<EmployeeAdjustmentSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<EmployeeAdjustmentDto>>();
            },
            createFunc: async data =>
            {
                data.EmployeeId = EmployeeId;
                await Client.CreateAsync(data.Adapt<EmployeeAdjustmentCreateRequest>());
            },
            updateFunc: async (id, EmployeeAdjustment) => await Client.UpdateAsync(id, EmployeeAdjustment.Adapt<EmployeeAdjustmentUpdateRequest>()),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);

    // Advanced Search
    private Guid _searchEmployeeId;
    private Guid SearchEmployeeId
    {
        get => _searchEmployeeId;
        set
        {
            _searchEmployeeId = value;
            _ = _table.ReloadDataAsync();
        }
    }
}