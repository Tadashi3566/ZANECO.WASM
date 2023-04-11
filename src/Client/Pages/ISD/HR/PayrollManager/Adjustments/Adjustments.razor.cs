using Mapster;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.Adjustments;

public partial class Adjustments
{
    [Inject]
    protected IAdjustmentsClient Client { get; set; } = default!;

    protected EntityServerTableContext<AdjustmentDto, Guid, AdjustmentUpdateRequest> Context { get; set; } = default!;

    private EntityTable<AdjustmentDto, Guid, AdjustmentUpdateRequest>? _table;

    private string? _searchString;

    protected override void OnInitialized()
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
        idFunc: Adjustment => Adjustment.Id,
        searchFunc: async filter => (await Client
            .SearchAsync(filter.Adapt<AdjustmentSearchRequest>()))
            .Adapt<PaginationResponse<AdjustmentDto>>(),
        createFunc: async Adjustment => await Client.CreateAsync(Adjustment.Adapt<AdjustmentCreateRequest>()),
        updateFunc: async (id, Adjustment) => await Client.UpdateAsync(id, Adjustment),
        deleteFunc: async id => await Client.DeleteAsync(id),
        exportAction: string.Empty);
    }

    private List<BreadcrumbItem> _breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
    };
}