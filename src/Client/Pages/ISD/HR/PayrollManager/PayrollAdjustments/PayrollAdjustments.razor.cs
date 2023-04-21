using Blazored.LocalStorage;
using Mapster;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Pages.Identity.Users;
using ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.Schedules;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.PayrollAdjustments;

public partial class PayrollAdjustments
{
    [Parameter]
    public Guid PayrollId { get; set; } = Guid.Empty;

    [Inject]
    protected IPayrollAdjustmentsClient Client { get; set; } = default!;

    [Inject]
    private ILocalStorageService? _localStorage { get; set; }

    protected EntityServerTableContext<PayrollAdjustmentDto, Guid, PayrollAdjustmentUpdateRequest> Context { get; set; } = default!;

    private EntityTable<PayrollAdjustmentDto, Guid, PayrollAdjustmentUpdateRequest>? _table;

    private string? _searchString;

    protected override void OnParametersSet()
    {
        if (PayrollId != Guid.Empty)
        {
            _searchPayrollId = PayrollId;
        }
    }

    protected override async void OnInitialized()
    {
        Context = new(
        entityName: "Payroll Adjustment",
        entityNamePlural: "Payroll Adjustments",
        entityResource: FSHResource.Payroll,
        fields: new()
        {
                new(data => data.PayrollName, "Payroll", "PayrollName"),
                new(data => data.AdjustmentNumber, "Number", "AdjustmentNumber"),
                new(data => data.AdjustmentName, "Adjustment", "AdjustmentName"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
        },
        enableAdvancedSearch: true,
        idFunc: data => data.Id,
        searchFunc: async _filter =>
        {
            var filter = _filter.Adapt<PayrollAdjustmentSearchRequest>();

            filter.PayrollId = SearchPayrollId;

            var result = await Client.SearchAsync(filter);

            return result.Adapt<PaginationResponse<PayrollAdjustmentDto>>();
        },
        createFunc: async PayrollAdjustment =>
        {
            PayrollAdjustment.PayrollId = SearchPayrollId;
            await Client.CreateAsync(PayrollAdjustment.Adapt<PayrollAdjustmentCreateRequest>());
        },
        updateFunc: async (id, PayrollAdjustment) =>
        {
            PayrollAdjustment.PayrollId = SearchPayrollId;
            await Client.UpdateAsync(id, PayrollAdjustment);
        },
        deleteFunc: async id => await Client.DeleteAsync(id),
        exportAction: string.Empty);

        await GetPayrollId();
    }

    private List<BreadcrumbItem> _breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
        new BreadcrumbItem("Payroll", href: "/payroll/payrolladjustments", icon: Icons.Material.Filled.Payment),
    };

    // Advanced Search
    private Guid _searchPayrollId;

    private Guid SearchPayrollId
    {
        get => _searchPayrollId;
        set => _searchPayrollId = value;
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