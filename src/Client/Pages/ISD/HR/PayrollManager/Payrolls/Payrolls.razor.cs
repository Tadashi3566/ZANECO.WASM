using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.Payrolls;
public partial class Payrolls
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IPayrollClient Client { get; set; } = default!;

    protected EntityServerTableContext<PayrollDto, Guid, PayrollUpdateRequest> Context { get; set; } = default!;

    private EntityTable<PayrollDto, Guid, PayrollUpdateRequest>? _table;

    private string? _searchString;
    private bool _canViewPayroll;
    private int _workingDays = 10;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewPayroll = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Payroll);

        Context = new(
            entityName: "Payroll",
            entityNamePlural: "Payroll",
            entityResource: FSHResource.Payroll,
            fields: new()
            {
                new(data => data.PayrollType, "Payroll Type", visible: false),
                new(data => data.EmploymentType, "Employment Type", "EmploymentType", Template: TemplateType),
                new(data => data.Name, "Name", "Name"),

                new(data => data.DateStart, "Date Start", visible: false),
                new(data => data.DateEnd, "Start-End Dates", "DateEnd", Template: TemplateDate),
                new(data => data.WorkingDays, "Working Days", "WorkingDays"),
                new(data => data.PayrollDate, "Payroll Date", "PayrollDate", typeof(DateOnly)),

                new(data => data.TotalSalary, "Total Salary", "TotalSalary", typeof(decimal)),
                new(data => data.TotalAdditional, "Total Additional", "TotalAdditional", typeof(decimal)),
                new(data => data.TotalGross, "Total Gross", "TotalGross", typeof(decimal)),
                new(data => data.TotalDeduction, "Total Deduction", "TotalDeduction", typeof(decimal)),
                new(data => data.TotalNet, "Total Net", "TotalNet", typeof(decimal)),

                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", "Notes", visible: false),
            },
            idFunc: Payroll => Payroll.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<PayrollSearchRequest>()))
                .Adapt<PaginationResponse<PayrollDto>>(),
            createFunc: async Payroll =>
            {
                Payroll.WorkingDays = DateFunctions.GetWorkingDays((DateTime)Payroll.DateStart!, (DateTime)Payroll.DateEnd!);

                await Client.CreateAsync(Payroll.Adapt<PayrollCreateRequest>());
            },
            updateFunc: async (id, Payroll) =>
            {
                Payroll.WorkingDays = DateFunctions.GetWorkingDays((DateTime)Payroll.DateStart!, (DateTime)Payroll.DateEnd!);

                await Client.UpdateAsync(id, Payroll);
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            hasExtraActionsFunc: () => _canViewPayroll,
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