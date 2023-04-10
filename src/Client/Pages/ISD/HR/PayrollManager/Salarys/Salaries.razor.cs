using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.Salarys;

public partial class Salaries
{
    [Inject]
    protected ISalarysClient Client { get; set; } = default!;

    protected EntityServerTableContext<SalaryDto, Guid, SalaryUpdateRequest> Context { get; set; } = default!;

    private EntityTable<SalaryDto, Guid, SalaryUpdateRequest>? _table;

    private string? _searchString;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Salary Profile",
            entityNamePlural: "Salary Profiles",
            entityResource: FSHResource.Payroll,
            fields: new()
            {
                new(data => data.StartDate, "Date Start", "StartDate", Template: TemplateStartEndDate),
                new(data => data.Number, "Number", "Number"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Amount, "Salary Amount", "SalaryAmount", typeof(decimal)),
                new(data => data.IncrementYears, "After Years", "IncrementYears", typeof(int)),
                new(data => data.IncrementAmount, "Increment Amount", "IncrementAmount", typeof(decimal)),
                new(data => data.IsActive, "Is Active", "IsActive", typeof(bool)),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<SalarySearchRequest>()))
                .Adapt<PaginationResponse<SalaryDto>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<SalaryCreateRequest>()),
            updateFunc: async (id, data) => await Client.UpdateAsync(id, data),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}