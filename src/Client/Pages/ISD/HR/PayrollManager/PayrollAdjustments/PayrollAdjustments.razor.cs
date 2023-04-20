using Blazored.LocalStorage;
using Mapster;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.PayrollAdjustments;

public partial class PayrollAdjustments
{
    [Inject]
    protected IPayrollAdjustmentsClient Client { get; set; } = default!;

    [Inject]
    private ILocalStorageService? _localStorage { get; set; }

    protected EntityServerTableContext<PayrollAdjustmentDto, Guid, PayrollAdjustmentUpdateRequest> Context { get; set; } = default!;

    private EntityTable<PayrollAdjustmentDto, Guid, PayrollAdjustmentUpdateRequest>? _table;

    private string? _searchString;

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
        idFunc: PayrollAdjustment => PayrollAdjustment.Id,
        searchFunc: async filter => (await Client
            .SearchAsync(filter.Adapt<PayrollAdjustmentSearchRequest>()))
            .Adapt<PaginationResponse<PayrollAdjustmentDto>>(),
        createFunc: async PayrollAdjustment => await Client.CreateAsync(PayrollAdjustment.Adapt<PayrollAdjustmentCreateRequest>()),
        updateFunc: async (id, PayrollAdjustment) => await Client.UpdateAsync(id, PayrollAdjustment),
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