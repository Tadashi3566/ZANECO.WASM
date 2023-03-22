using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Attendances;

public partial class Attendances
{
    [Parameter]
    public Guid EmployeeId { get; set; } = Guid.Empty;
    [Parameter]
    public DateTime? AttendanceDate { get; set; } = DateTime.Today;
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IAttendancesClient Client { get; set; } = default!;
    [Inject]
    protected IPersonalClient User { get; set; } = default!;

    protected EntityServerTableContext<AttendanceDto, Guid, AttendanceUpdateRequest> Context { get; set; } = default!;

    private EntityTable<AttendanceDto, Guid, AttendanceUpdateRequest>? _table;

    private HashSet<AttendanceDto> _selectedItems = new();

    private MudDateRangePicker? _dateRangePicker = default!;
    private DateRange? _dateRange;

    private string? _searchString;
    private bool _canViewEmployees;

    protected override void OnParametersSet()
    {
        if (EmployeeId != Guid.Empty)
        {
            _searchEmployeeId = EmployeeId!;
        }
    }

    protected override async void OnInitialized()
    {
        var state = await AuthState;
        _canViewEmployees = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Employees);

        Context = new(
            entityName: "Attendance",
            entityNamePlural: "Attendance",
            entityResource: FSHResource.Attendance,
            fields: new()
            {
                new(data => data.EmployeeName, "Name", "EmployeeName", visible: true),
                new(data => data.AttendanceDate, "Date", "AttendanceDate", Template: TemplateDayType),
                new(data => data.ScheduleDetailDay, "Day", visible: false),
                new(data => data.DayType, "Type", "DayType", visible: false),
                new(data => data.ActualTimeIn1, "Time-In1", "ActualTimeIn1", Template: TemplateImageTimeIn1),
                new(data => data.ActualTimeOut1, "Time-Out1", "ActualTimeOut1", Template: TemplateImageTimeOut1),
                new(data => data.ActualTimeIn2, "Time-In2", "ActualTimeIn2", Template: TemplateImageTimeIn2),
                new(data => data.ActualTimeOut2, "Time-Out2", "ActualTimeOut2", Template: TemplateImageTimeOut2),
                new(data => data.LateMinutes, "Late/Under (Minutes)", "MinutesLate", Template: TemplateMinutesLateUnder),
                new(data => data.TotalHours, "Total/Paid (Hours)", "TotalHours", Template: TemplateHoursTotalPaid),
                new(data => data.Status, "Status", "Status"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            idFunc: Attendance => Attendance.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<AttendanceSearchRequest>();

                if (SearchEmployeeId.Equals(Guid.Empty))
                {
                    var user = await User.GetProfileAsync();
                    if (user.EmployeeId is not null)
                    {
                        _searchEmployeeId = (Guid)user.EmployeeId!;
                    }
                }

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

                filter.DateStart = _dateRange!.Start;
                filter.DateEnd = _dateRange.End;

                //filter.DateStart = DateStart;
                //filter.DateEnd = DateEnd;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<AttendanceDto>>();
            },
            createFunc: null,
            updateFunc: async (id, data) =>
            {
                data.EmployeeId = _searchEmployeeId;

                await Client.UpdateAsync(id, data.Adapt<AttendanceUpdateRequest>());

                AttendanceCalculateRequest attendanceCalculateRequest = new();
                attendanceCalculateRequest.Id = data.Id;
                await ApiHelper.ExecuteCallGuardedAsync(() => Client.CalculateAsync(attendanceCalculateRequest),
                    Snackbar, successMessage: $"Attendance Id {attendanceCalculateRequest.Id} has been successfully calculated.");
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
    }

    private List<BreadcrumbItem> _breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
        new BreadcrumbItem("Employees", href: "/hr/employees", icon: Icons.Material.Filled.Groups),
    };

    // Advanced Search
    private Guid _searchEmployeeId;

    private Guid SearchEmployeeId
    {
        get => _searchEmployeeId;
        set
        {
            _searchEmployeeId = value;
            _table!.ReloadDataAsync();
        }
    }

    private DateTime? _dateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime? DateStart
    {
        get => _dateStart;
        set
        {
            _dateStart = value;
            _table!.ReloadDataAsync();
        }
    }

    private DateTime? _dateEnd = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
    private DateTime? DateEnd
    {
        get => _dateEnd;
        set
        {
            _dateEnd = value;
            _table!.ReloadDataAsync();
        }
    }

    private async Task Calculate()
    {
        AttendanceCalculateRequest request = new();

        string transactionContent = $"Are you sure you want to calculate selected attendance?";
        DialogParameters parameters = new()
        {
            { nameof(TransactionConfirmation.TransactionTitle), "Calculate Selected Attendance" },
            { nameof(TransactionConfirmation.ContentText), transactionContent },
            { nameof(TransactionConfirmation.ConfirmText), "Calculate" }
        };
        DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        IDialogReference dialog = DialogService.Show<TransactionConfirmation>("Calculate", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            foreach (var item in _selectedItems)
            {
                request.Id = item.Id;

                await ApiHelper.ExecuteCallGuardedAsync(() => Client.CalculateAsync(request), Snackbar, successMessage: $"Attendance Id {item.Id} has been successfully calculated.");
            }

            await _table!.ReloadDataAsync();
        }
    }
}