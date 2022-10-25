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

namespace ZANECO.WASM.Client.Pages.HR.PayrollManager.Schedules;
public partial class Schedules
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected ISchedulesClient Client { get; set; } = default!;
    [Inject]
    protected IEmployeesClient EmployeeClient { get; set; } = default!;

    protected EntityServerTableContext<ScheduleDto, Guid, ScheduleUpdateRequest> Context { get; set; } = default!;

    private EntityTable<ScheduleDto, Guid, ScheduleUpdateRequest> _table = default!;

    private bool _canViewRoleClaims;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewRoleClaims = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.RoleClaims);

        Context = new(
            entityName: "Schedules",
            entityNamePlural: "Schedules",
            entityResource: FSHResource.Schedules,
            fields: new()
            {
                new(data => data.Name, "Name", "Name"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<ScheduleSearchRequest>()))
                .Adapt<PaginationResponse<ScheduleDto>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<ScheduleCreateRequest>()),
            updateFunc: async (id, data) => await Client.UpdateAsync(id, data),
            deleteFunc: async id => await Client.DeleteAsync(id),
            hasExtraActionsFunc: () => _canViewRoleClaims,
            exportAction: string.Empty);
    }

    private async Task SetAsCurrentSchedule(Guid scheduleId)
    {
        EmployeeSetScheduleRequest request = new()
        {
            ScheduleId = scheduleId
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => EmployeeClient.SetScheduleAsync(scheduleId, request.Adapt<EmployeeSetScheduleRequest>()), Snackbar))
        {
            Snackbar.Add("Selected Schedule has been successfully Set to Unscheduled Employees", Severity.Success);
        }
    }
}