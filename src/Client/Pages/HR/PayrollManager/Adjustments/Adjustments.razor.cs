using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.HR.PayrollManager.Adjustments;
public partial class Adjustments
{
    [Inject]
    protected IAdjustmentsClient Client { get; set; } = default!;

    protected EntityServerTableContext<AdjustmentDto, Guid, AdjustmentUpdateRequest> Context { get; set; } = default!;

    private EntityTable<AdjustmentDto, Guid, AdjustmentUpdateRequest> _table = default!;

    protected override Task OnInitializedAsync()
    {
        Context = new(
            entityName: "Adjustment",
            entityNamePlural: "Adjustments",
            entityResource: FSHResource.Adjustments,
            fields: new()
            {
                new(data => data.AdjustmentType, "Adjustment Type", "AdjustmentType"),
                new(data => data.EmployeeType, "Employee Type", "EmployeeType"),
                new(data => data.Number, "Number", "Number"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Amount.ToString("N2"), "Amount", "Amount"),
                new(data => data.PaymentSchedule, "Schedule", "PaymentSchedule"),
                new(data => data.IsOptional, "Optional", "IsOptional"),
                new(data => data.IsLoan, "Loan", "IsLoan"),
                new(data => data.IsActive, "Active", "IsActive"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            idFunc: Adjustment => Adjustment.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<AdjustmentSearchRequest>()))
                .Adapt<PaginationResponse<AdjustmentDto>>(),
            createFunc: async Adjustment => await Client.CreateAsync(Adjustment.Adapt<AdjustmentCreateRequest>()),
            updateFunc: async (id, Adjustment) => await Client.UpdateAsync(id, Adjustment),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
        return Task.CompletedTask;
    }
}