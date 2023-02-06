using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.EmployeePayrollDetails;
public partial class EmployeePayrollDetails
{
    [Parameter]
    public Guid EmployeeId { get; set; } = default!;
    [Parameter]
    public Guid PayrollId { get; set; } = default!;
    [Inject]
    protected IEmployeePayrollDetailClient Client { get; set; } = default!;

    protected EntityServerTableContext<EmployeePayrollDetailDto, Guid, EmployeePayrollDetailUpdateRequest> Context { get; set; } = default!;

    private EntityTable<EmployeePayrollDetailDto, Guid, EmployeePayrollDetailUpdateRequest> _table = default!;

    private string? _searchString;
    protected override void OnParametersSet()
    {
        if (PayrollId != Guid.Empty)
        {
            _searchPayrollId = PayrollId;
        }

        if (EmployeeId != Guid.Empty)
        {
            _searchEmployeeId = EmployeeId;
        }
    }

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Employee Payroll",
            entityNamePlural: "Employee Payroll",
            entityResource: FSHResource.Payroll,
            fields: new()
            {
                new(data => data.EmployeeName, "Employee", "Employee"),
                new(data => data.PayrollName, "Payroll", "PayrollName"),
                new(data => data.AdjustmentType, "Type", "AdjustmentType"),
                new(data => data.AdjustmentName, "Name", "Name"),
                new(data => data.Amount, "Amount", "Amount", typeof(decimal)),
                new(data => data.Description, "Description", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<EmployeePayrollDetailSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;
                filter.PayrollId = SearchPayrollId == default ? null : SearchPayrollId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<EmployeePayrollDetailDto>>();
            },
            createFunc: async data =>
            {
                data.EmployeeId = SearchEmployeeId;
                data.PayrollId = SearchPayrollId;
                await Client.CreateAsync(data.Adapt<EmployeePayrollDetailCreateRequest>());
            },
            updateFunc: async (id, EmployeePayrollDetail) => await Client.UpdateAsync(id, EmployeePayrollDetail.Adapt<EmployeePayrollDetailUpdateRequest>()),
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

    private Guid _searchPayrollId;
    private Guid SearchPayrollId
    {
        get => _searchPayrollId;
        set
        {
            _searchPayrollId = value;
            _ = _table.ReloadDataAsync();
        }
    }
}