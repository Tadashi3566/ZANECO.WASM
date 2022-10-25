using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.HR.PayrollManager.Loans;
public partial class Loans
{
    [Parameter]
    public Guid EmployeeId { get; set; } = default!;
    [Inject]
    protected ILoanClient Client { get; set; } = default!;

    protected EntityServerTableContext<LoanDto, Guid, LoanUpdateRequest> Context { get; set; } = default!;

    private EntityTable<LoanDto, Guid, LoanUpdateRequest> _table = default!;

    private DateTime? _dtend;
    private decimal _ammortization = 0;

    protected override void OnParametersSet()
    {
        if (EmployeeId != Guid.Empty)
        {
            _searchEmployeeId = EmployeeId;
        }
    }

    protected override void OnInitialized()
    {
        Context = new(
            entityName: "Employee Loan",
            entityNamePlural: "Employee Loans",
            entityResource: FSHResource.Payroll,
            fields: new()
            {
                new(data => data.EmployeeName, "Employee", "Employee"),
                new(data => data.AdjustmentName, "Name", "Name"),
                new(data => data.Amount.ToString("N2"), "Amount", "Amount"),
                new(data => data.PaymentSchedule, "Schedule", "PaymentSchedule"),
                new(data => data.DateReleased.ToString("MMM dd, yyyy"), "Released", "DateReleased"),
                new(data => data.DateStart.ToString("MMM dd, yyyy"), "Start", "DateStart"),
                new(data => data.Months, "Months", "Months"),
                new(data => data.DateEnd.ToString("MMM dd, yyyy"), "End", "DateEnd"),
                new(data => data.Ammortization.ToString("N2"), "Ammortization", "Ammortization"),
                new(data => data.Status, "Status", "Status"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<LoanSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<LoanDto>>();
            },
            createFunc: async data =>
            {
                data.EmployeeId = SearchEmployeeId;
                data.DateEnd = _dtend;
                data.Ammortization = _ammortization;

                await Client.CreateAsync(data.Adapt<LoanCreateRequest>());
            },
            updateFunc: async (id, data) =>
            {
                data.DateEnd = _dtend;
                data.Ammortization = _ammortization;

                await Client.UpdateAsync(id, data.Adapt<LoanUpdateRequest>());
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
    }

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

    private void SetDateEnd(string schedule, decimal amount, DateTime? dtstart, int months)
    {
        if (dtstart != null)
        {
            _dtend = dtstart?.AddMonths(months);
        }

        if (schedule.Equals("PAYROLL"))
        {
            _ammortization = amount / (months * 2);
        }
        else
        {
            _ammortization = amount / months;
        }

        StateHasChanged();
    }
}