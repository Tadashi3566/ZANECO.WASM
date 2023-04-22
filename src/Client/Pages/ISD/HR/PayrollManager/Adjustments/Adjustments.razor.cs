using Blazored.LocalStorage;
using Mapster;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.PayrollAdjustments;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.Adjustments;

public partial class Adjustments
{
    [Inject]
    protected IAdjustmentsClient Client { get; set; } = default!;

    [Inject]
    protected IPayrollAdjustmentsClient PayrollAdjustmentClient { get; set; } = default!;

    [Inject]
    private ILocalStorageService? _localStorage { get; set; }

    protected EntityServerTableContext<AdjustmentDto, Guid, AdjustmentUpdateRequest> Context { get; set; } = default!;

    private EntityTable<AdjustmentDto, Guid, AdjustmentUpdateRequest>? _table;

    private HashSet<AdjustmentDto> _selectedItems = new();

    private string? _searchString;

    protected override async void OnInitialized()
    {
        Context = new(
        entityName: "Adjustment",
        entityNamePlural: "Adjustments",
        entityResource: FSHResource.Payroll,
        fields: new()
        {
                new(data => data.AdjustmentType, "Adjustment Type", "AdjustmentType"),
                new(data => data.EmployeeType, "Employee Type", "EmployeeType"),
                new(data => data.Number, "Number", "Number"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Amount, "Amount", "Amount", typeof(decimal)),
                new(data => data.PaymentSchedule, "Schedule", "PaymentSchedule"),
                new(data => data.IsOptional, "Optional", "IsOptional", typeof(bool)),
                new(data => data.IsLoan, "Loan", "IsLoan", typeof(bool)),
                new(data => data.IsActive, "Active", "IsActive", typeof(bool)),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
        },
        enableAdvancedSearch: true,
        idFunc: Adjustment => Adjustment.Id,
        searchFunc: async filter => (await Client
            .SearchAsync(filter.Adapt<AdjustmentSearchRequest>()))
            .Adapt<PaginationResponse<AdjustmentDto>>(),
        createFunc: async Adjustment => await Client.CreateAsync(Adjustment.Adapt<AdjustmentCreateRequest>()),
        updateFunc: async (id, Adjustment) => await Client.UpdateAsync(id, Adjustment),
        deleteFunc: async id => await Client.DeleteAsync(id),
        exportAction: string.Empty);

        await GetPayrollId();
    }


    private List<BreadcrumbItem> _breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
    };

    // Advanced Search
    private Guid _searchPayrollId;

    private Guid SearchPayrollId
    {
        get => _searchPayrollId;
        set => _searchPayrollId = value;
    }

    private async Task AddToPayroll(AdjustmentDto dto)
    {
        string transactionTitle = "Add Adjustment to Payroll";
        string transactionContent = $"Are you sure you want to add Adjustment(s) to Payroll?";
        DialogParameters parameters = new()
        {
            { nameof(TransactionConfirmation.TransactionTitle), transactionTitle },
            { nameof(TransactionConfirmation.ContentText), transactionContent },
        };
        DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        IDialogReference dialog = DialogService.Show<TransactionConfirmation>(transactionTitle, parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled && _selectedItems.Count > 0)
        {
            foreach (var payrollId in _selectedItems.Select(x => x.Id))
            {
                var payrollAdjustment = new PayrollAdjustmentCreateRequest();

                payrollAdjustment.PayrollId = SearchPayrollId;
                payrollAdjustment.AdjustmentId = payrollId;

                await PayrollAdjustmentClient.CreateAsync(payrollAdjustment);

                Snackbar.Add("Adjustment(s) has been added to Payroll.", Severity.Success);
            }
        }
    }

    private async Task GetPayrollId()
    {
        try
        {
            string? _payrollId = await _localStorage!.GetItemAsync<string>("payrollId");

            if (_payrollId is not null)
            {
                _searchPayrollId = Guid.Parse(_payrollId);
            }

            StateHasChanged();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void SetPayrollId(in Guid payrollId) => _localStorage?.SetItemAsStringAsync("payrollId", payrollId.ToString());
}