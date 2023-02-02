using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.HR.PayrollManager.Contributions;
public partial class Contributions
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IContributionClient Client { get; set; } = default!;
    protected EntityServerTableContext<ContributionDto, Guid, ContributionUpdateRequest> Context { get; set; } = default!;

    private EntityTable<ContributionDto, Guid, ContributionUpdateRequest> _table = default!;

    private string? _searchString;
    private bool _canViewRoleClaims;
    private decimal _totalContribution = 0;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewRoleClaims = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.RoleClaims);

        Context = new(
            entityName: "Contribution",
            entityNamePlural: "Contributions",
            entityResource: FSHResource.Payroll,
            fields: new()
            {
                new(data => data.ContributionType, "Contribution Type", "ContributionType"),
                new(data => data.DateEffectivityStart, "Effectivity Date", "DateEffectivityStart", Template: TemplateDateEffectivity),
                new(data => data.DateEffectivityEnd, "End", visible: false),
                new(data => data.RangeStart, "Amount Range", "RangeStart", typeof(decimal), Template: TemplateAmountRange),
                new(data => data.RangeEnd, "To", Type: typeof(decimal), visible: false),
                new(data => data.EmployerContribution, "Employer/Employee", "EmployerContribution", typeof(decimal), Template: TemplateContribution),
                new(data => data.EmployeeContribution, "Employee", Type: typeof(decimal), visible: false),
                new(data => data.TotalContribution, "Total", "TotalContribution", typeof(decimal)),
                new(data => data.Percentage, "Percentage", "Percentage"),
                new(data => data.IsFixed, "Fixed", "IsFixed", typeof(bool)),
                new(data => data.Description, "Description", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<ContributionSearchRequest>()))
                .Adapt<PaginationResponse<ContributionDto>>(),
            createFunc: async data =>
            {
                data.TotalContribution = _totalContribution;

                await Client.CreateAsync(data.Adapt<ContributionCreateRequest>());
            },
            updateFunc: async (id, data) =>
            {
                data.TotalContribution = _totalContribution;

                await Client.UpdateAsync(id, data);
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            hasExtraActionsFunc: () => _canViewRoleClaims,
            exportAction: string.Empty);
    }

    private void CalculateTotalContribution(decimal employerContribution, decimal employeeContribution)
    {
        _totalContribution = employerContribution + employeeContribution;

        StateHasChanged();
    }
}