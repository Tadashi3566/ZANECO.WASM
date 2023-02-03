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

namespace ZANECO.WASM.Client.Pages.HR.PayrollManager.Payrolls;
public partial class Payrolls
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IPayrollClient Client { get; set; } = default!;

    protected EntityServerTableContext<PayrollDto, Guid, PayrollUpdateRequest> Context { get; set; } = default!;

    private EntityTable<PayrollDto, Guid, PayrollUpdateRequest> _table = default!;

    private bool _canViewRoleClaims;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewRoleClaims = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.RoleClaims);

        Context = new(
            entityName: "Payroll",
            entityNamePlural: "Payroll",
            entityResource: FSHResource.Payroll,
            fields: new()
            {
                new(data => data.PayrollType, "Payroll Type", "PayrollType"),
                new(data => data.EmploymentType, "Employment Type", "EmploymentType"),
                new(data => data.Name, "Name", "Name"),

                new(data => data.DateStart, "Date Start", "DateStart", typeof(DateOnly)),
                new(data => data.DateEnd, "Date End", "DateEnd", typeof(DateOnly)),
                new(data => data.WorkingDays, "Working Days", "WorkingDays"),
                new(data => data.PayrollDate, "Payroll Date", "PayrollDate", typeof(DateOnly)),

                new(data => data.TotalSalary, "Total Salary", "TotalSalary", typeof(decimal)),
                new(data => data.TotalAdditional, "Total Additional", "TotalAdditional", typeof(decimal)),
                new(data => data.TotalGross, "Total Gross", "TotalGross", typeof(decimal)),
                new(data => data.TotalDeduction, "Total Deduction", "TotalDeduction", typeof(decimal)),
                new(data => data.TotalNet, "Total Net", "TotalNet", typeof(decimal)),

                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            idFunc: Payroll => Payroll.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<PayrollSearchRequest>()))
                .Adapt<PaginationResponse<PayrollDto>>(),
            createFunc: async Payroll => await Client.CreateAsync(Payroll.Adapt<PayrollCreateRequest>()),
            updateFunc: async (id, Payroll) => await Client.UpdateAsync(id, Payroll),
            deleteFunc: async id => await Client.DeleteAsync(id),
            hasExtraActionsFunc: () => _canViewRoleClaims,
            exportAction: string.Empty);
    }

    private async Task PayrollGenerate(Guid payrollId)
    {
        PayrollGenerateRequest request = new()
        {
            Id = payrollId
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => Client.GenerateAsync(request.Adapt<PayrollGenerateRequest>()), Snackbar))
        {
            Snackbar.Add("Payroll has been successfully generated", Severity.Success);
        }
    }
}