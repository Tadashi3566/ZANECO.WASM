using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.EmployeePayrolls;
public partial class EmployeePayrolls
{
    [Parameter]
    public Guid PayrollId { get; set; } = default!;
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IEmployeePayrollsClient Client { get; set; } = default!;
    [Inject]
    protected IEmployeePayrollDetailClient PayrollClient { get; set; } = default!;

    protected EntityServerTableContext<EmployeePayrollDto, Guid, EmployeePayrollUpdateRequest> Context { get; set; } = default!;

    private EntityTable<EmployeePayrollDto, Guid, EmployeePayrollUpdateRequest> _table = default!;

    private string? _searchString;

    private bool _canViewPayroll;

    protected override void OnParametersSet()
    {
        if (PayrollId != Guid.Empty)
        {
            _searchPayrollId = PayrollId;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewPayroll = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Payroll);

        Context = new(
            entityName: "Employee Payroll",
            entityNamePlural: "Employee Payroll",
            entityResource: FSHResource.Payroll,
            fields: new()
            {
                new(data => data.EmployeeName, "Employee", "Employee"),
                new(data => data.PayrollName, "Payroll", "PayrollName"),
                new(data => data.Salary, "Salary", "Salary", typeof(decimal)),
                new(data => data.Additional, "Additional", "Additional", typeof(decimal)),
                new(data => data.Gross, "Gross", "Gross", typeof(decimal)),
                new(data => data.Deduction, "Deduction", "Deduction", typeof(decimal)),
                new(data => data.Net, "Net", "Net", typeof(decimal)),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<EmployeePayrollSearchRequest>();

                filter.PayrollId = SearchPayrollId == default ? null : SearchPayrollId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<EmployeePayrollDto>>();
            },
            createFunc: async data =>
            {
                data.PayrollId = SearchPayrollId;
                await Client.CreateAsync(data.Adapt<EmployeePayrollCreateRequest>());
            },
            updateFunc: async (id, EmployeePayroll) => await Client.UpdateAsync(id, EmployeePayroll.Adapt<EmployeePayrollUpdateRequest>()),
            deleteFunc: async id => await Client.DeleteAsync(id),
            hasExtraActionsFunc: () => _canViewPayroll,
            exportAction: string.Empty);
    }

    private async Task EmployeePayrollGenerate(Guid employeeId, Guid payrollId)
    {
        EmployeePayrollDetailGenerateRequest request = new()
        {
            EmployeeId = employeeId,
            PayrollId = payrollId
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => PayrollClient.GenerateAsync(request.Adapt<EmployeePayrollDetailGenerateRequest>()), Snackbar))
        {
            Snackbar.Add("Employee Payroll has been successfully generated", Severity.Success);

            // NavigationManager.NavigateTo($"/payroll/employeepayrolldetails/{employeeId}/{payrollId}");
        }
    }

    // Advanced Search
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