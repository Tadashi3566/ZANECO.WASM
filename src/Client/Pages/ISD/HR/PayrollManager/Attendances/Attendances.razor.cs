using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.Attendances;

public partial class Attendances
{
    [Parameter]
    public Guid EmployeeId { get; set; } = Guid.Empty;
    [Parameter]
    public DateTime? AttendanceDate { get; set; } = DateTime.Today;
    [Inject]
    protected IAttendanceClient Client { get; set; } = default!;

    protected EntityServerTableContext<AttendanceDto, Guid, AttendanceUpdateRequest> Context { get; set; } = default!;

    private EntityTable<AttendanceDto, Guid, AttendanceUpdateRequest>? _table;

    private string? _searchString;

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
            entityName: "Attendance",
            entityNamePlural: "Attendance",
            entityResource: FSHResource.Attendance,
            fields: new()
            {
                new(data => data.EmployeeName, "Name", "EmployeeName", visible: EmployeeId.Equals(Guid.Empty)),
                new(data => data.AttendanceDate, "Date", "AttendanceDate", typeof(DateOnly)),
                new(data => data.ScheduleDetailDay, "Day", "ScheduleDetailDay"),
                new(data => data.ActualTimeIn1, "Actual TimeLog", "ActualTimeIn1", Template: TemplateImageTimeIn1),
                new(data => data.ActualTimeOut1, "Actual TimeOut1", "ActualTimeOut1", Template: TemplateImageTimeOut1),
                new(data => data.ActualTimeIn2, "Actual TimeIn2", "ActualTimeIn2", Template: TemplateImageTimeIn2),
                new(data => data.ActualTimeOut2, "Actual TimeOut2", "ActualTimeOut2", Template: TemplateImageTimeOut2),
                new(data => data.LateMinutes, "Late (Minutes)", "MinutesLate"),
                new(data => data.UnderTimeMinutes, "Under (Minutes)", "UnderTimeMinutes"),
                new(data => data.TotalHours, "Total Hours", "TotalHours", Template: TemplateHoursTotalPaid),
                new(data => data.Status, "Status", "Status"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            idFunc: Attendance => Attendance.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<AttendanceSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;
                filter.DateStart = DateStart;
                filter.DateEnd = DateEnd;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<AttendanceDto>>();
            },
            createFunc: null,
            updateFunc: async (id, data) =>
            {
                data.EmployeeId = _searchEmployeeId;

                await Client.UpdateAsync(id, data.Adapt<AttendanceUpdateRequest>());
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
}