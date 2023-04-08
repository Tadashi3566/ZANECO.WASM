using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Schedule;
using System.Globalization;
using System.Timers;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Appointments;

public partial class Appointments
{
    [Parameter]
    public Guid EmployeeId { get; set; } = Guid.Empty;
    [Inject]
    protected IAppointmentsClient Client { get; set; } = default!;
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IPersonalClient User { get; set; } = default!;

    protected EntityServerTableContext<AppointmentDto, int, AppointmentViewModel> Context { get; set; } = default!;

    private EntityTable<AppointmentDto, int, AppointmentViewModel>? _table;

    private bool _canViewEmployees;

    private string? _searchString;

    private List<AppointmentDto>? _appointments;

    protected override void OnParametersSet()
    {
        if (EmployeeId != Guid.Empty)
        {
            _searchEmployeeId = EmployeeId;
        }
    }

    protected override async void OnInitialized()
    {
        var state = await AuthState;
        _canViewEmployees = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Employees);

        var filter = new AppointmentSearchRequest
        {
            
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfAppointmentDto response)
        {
            _appointments = response.Data.ToList();
        }

        Context = new(
            entityName: "Appointment",
            entityNamePlural: "Appointments",
            entityResource: FSHResource.Appointment,
            fields: new()
            {
                new(data => data.Id, "Id", "Id"),
                new(data => data.EmployeeName, "Employee", "EmployeeName"),
                new(data => data.AppointmentType, "Type", "AppointmentType"),
                new(data => data.Subject, "Subject", "Subject"),
                new(data => data.StartDateTime, "Date Time", Template: TemplateStartEnd),
                new(data => data.Status, "Status", "Status"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                if (SearchEmployeeId.Equals(Guid.Empty))
                {
                    var user = await User.GetProfileAsync();
                    if (user.EmployeeId is not null)
                    {
                        _searchEmployeeId = (Guid)user.EmployeeId!;
                    }
                }

                var filter = _filter.Adapt<AppointmentSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<AppointmentDto>>();
            },
            createFunc: async request =>
            {
                if (!string.IsNullOrEmpty(request.ImageInBytes))
                {
                    request.Image = new ImageUploadRequest() { Data = request.ImageInBytes, Extension = request.ImageExtension ?? string.Empty, Name = $"{request.Subject}_{Guid.NewGuid():N}" };
                }

                request.EmployeeId = _searchEmployeeId;

                await Client.CreateAsync(request.Adapt<AppointmentCreateRequest>());

                request.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, request) =>
            {
                if (!string.IsNullOrEmpty(request.ImageInBytes))
                {
                    request.DeleteCurrentImage = true;
                    request.Image = new ImageUploadRequest() { Data = request.ImageInBytes, Extension = request.ImageExtension ?? string.Empty, Name = $"{request.Subject}_{Guid.NewGuid():N}" };
                }

                await Client.UpdateAsync(id, request.Adapt<AppointmentUpdateRequest>());

                request.ImageInBytes = string.Empty;
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
    }

    // Advanced Search
    private Guid _searchEmployeeId;
    private Guid SearchEmployeeId
    {
        get => _searchEmployeeId;
        set
        {
            _searchEmployeeId = value;
            _ = _table!.ReloadDataAsync();
        }
    }

    private List<MudBlazor.BreadcrumbItem> _breadcrumbs = new List<MudBlazor.BreadcrumbItem>
    {
        new MudBlazor.BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
        new MudBlazor.BreadcrumbItem("Employees", href: "/hr/employees", icon: Icons.Material.Filled.Groups),
    };

    // TODO : Make this as a shared service or something? Since it's used by Profile Component also for now, and literally any other component that will have image upload.
    // The new service should ideally return $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}"
    private async Task UploadImage(InputFileChangeEventArgs e)
    {
        if (e.File != null)
        {
            string? extension = Path.GetExtension(e.File.Name);
            if (!ApplicationConstants.SupportedImageFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("Image Format Not Supported.", Severity.Error);
                return;
            }

            Context.AddEditModal.RequestModel.ImageExtension = extension;
            var imageFile = await e.File.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
            byte[]? buffer = new byte[imageFile.Size];
            await imageFile.OpenReadStream(ApplicationConstants.MaxAllowedSize).ReadAsync(buffer);
            Context.AddEditModal.RequestModel.ImageInBytes = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            Context.AddEditModal.ForceRender();
        }
    }

    private void ClearImageInBytes()
    {
        Context.AddEditModal.RequestModel.ImageInBytes = string.Empty;
        Context.AddEditModal.ForceRender();
    }

    private void SetDeleteCurrentImageFlag()
    {
        Context.AddEditModal.RequestModel.ImageInBytes = string.Empty;
        Context.AddEditModal.RequestModel.ImagePath = string.Empty;
        Context.AddEditModal.RequestModel.DeleteCurrentImage = true;
        Context.AddEditModal.ForceRender();
    }

    SfTextBox? SubjectRef;
    SfCheckBox<bool>? ViewRef;
    SfTextBox? DescriptionRef;
    SfMultiSelect<int[], CalendarData>? ResourceRef;
    SfSchedule<AppointmentDto>? ScheduleRef;
    SfDropDownList<int, CalendarData>? CalendarRef;

    //public class AppointmentDto
    //{
    //    public int Id { get; set; }
    //    public string Subject { get; set; }
    //    public DateTime StartDateTime { get; set; }
    //    public DateTime EndDateTime { get; set; }
    //    public string Location { get; set; }
    //    public string Description { get; set; }
    //    public bool IsAllDay { get; set; }
    //    public bool IsReadonly { get; set; }
    //    public int CalendarId { get; set; }
    //    public int? RecurrenceID { get; set; }
    //    public string RecurrenceRule { get; set; }
    //    public string RecurrenceException { get; set; }

    //    public AppointmentDto() { }

    //    public AppointmentDto(int Id, string Subject, DateTime StartDateTime, DateTime EndDateTime, string Location, string Description, bool IsAllDay, bool IsReadonly, int CalendarId, int RecurrenceID, string RecurrenceRule, string RecurrenceException)
    //    {
    //        this.Id = Id;
    //        this.Subject = Subject;
    //        this.StartDateTime = StartDateTime;
    //        this.EndDateTime = EndDateTime;
    //        this.Location = Location;
    //        this.Description = Description;
    //        this.IsAllDay = IsAllDay;
    //        this.IsReadonly = IsReadonly;
    //        this.CalendarId = CalendarId;
    //        this.RecurrenceID = RecurrenceID;
    //        this.RecurrenceRule = RecurrenceRule;
    //        this.RecurrenceException = RecurrenceException;
    //    }

    //    public List<AppointmentDto> GetEvents()
    //    {
    //        List<AppointmentDto> EventData = new List<AppointmentDto>();
    //        DateTime YearStart = new DateTime(DateTime.Now.Year, 1, 1);
    //        DateTime YearEnd = new DateTime(DateTime.Now.Year, 12, 31);
    //        string[] EventSubjects = new string[]
    //        {
    //            "Bering Sea Gold", "Technology", "Maintenance", "Meeting", "Travelling", "Annual Conference", "Birthday Celebration", "Farewell Celebration",
    //            "Wedding Anniversary", "Alaska: The Last Frontier", "Deadliest Catch", "Sports Day", "MoonShiners", "Close Encounters", "HighWay Thru Hell",
    //            "Daily Planet", "Cash Cab", "Basketball Practice", "Rugby Match", "Guitar Class", "Music Lessons", "Doctor checkup", "Brazil - Mexico",
    //            "Opening ceremony", "Final presentation"
    //        };
    //        DateTime CurrentDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday);
    //        DateTime Start = new DateTime(CurrentDate.Year, CurrentDate.Month, CurrentDate.Day, 10, 0, 0);
    //        DateTime End = new DateTime(CurrentDate.Year, CurrentDate.Month, CurrentDate.Day, 11, 30, 0);
    //        EventData.Add(new AppointmentDto()
    //        {
    //            Id = 1,
    //            Subject = EventSubjects[new Random().Next(1, 25)],
    //            StartDateTime = Start.ToLocalTime(),
    //            EndDateTime = End.ToLocalTime(),
    //            Location = "",
    //            Description = "Event Scheduled",
    //            RecurrenceRule = "FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR;INTERVAL=1;COUNT=10;",
    //            IsAllDay = false,
    //            IsReadonly = false,
    //            CalendarId = 1
    //        });
    //        for (int a = 0, id = 2; a < 500; a++)
    //        {
    //            Random random = new Random();
    //            int Month = random.Next(1, 12);
    //            int Date = random.Next(1, 28);
    //            int Hour = random.Next(1, 24);
    //            int Minutes = random.Next(1, 60);
    //            DateTime start = new DateTime(YearStart.Year, Month, Date, Hour, Minutes, 0);
    //            DateTime end = new DateTime(start.Ticks).AddHours(2);
    //            DateTime StartDate = new DateTime(start.Ticks);
    //            DateTime EndDate = new DateTime(end.Ticks);
    //            AppointmentDto eventDatas = new AppointmentDto()
    //            {
    //                Id = id,
    //                Subject = EventSubjects[random.Next(1, 25)],
    //                StartDateTime = StartDate,
    //                EndDateTime = EndDate,
    //                Location = "",
    //                Description = "Event Scheduled",
    //                IsAllDay = id % 10 == 0,
    //                IsReadonly = EndDate < DateTime.Now,
    //                CalendarId = (a % 4) + 1
    //            };
    //            EventData.Add(eventDatas);
    //            id++;
    //        }
    //        return EventData;
    //    }
    //}

    public class CalendarData
    {
        public string CalendarName { get; set; }
        public string CalendarColor { get; set; }
        public int CalendarId { get; set; }
    }

    public class WeekDays
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class SlotData : WeekDays { }

    public class TimeFormatData
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class WeekNumbers
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Tooltip
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Timezone
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    //List<AppointmentDto> DataSource = new AppointmentDto().GetEvents();
    public AppointmentDto EventData { get; set; }
    public CellClickEventArgs CellData { get; set; }
    private bool isCell { get; set; }
    private bool isEvent { get; set; }
    private bool isRecurrence { get; set; }
    private int SlotCount { get; set; } = 2;
    private int SlotInterval { get; set; } = 60;
    internal int FirstDayOfWeek { get; set; } = 0;
    private bool EnableGroup { get; set; } = true;
    private bool TooltipEnable { get; set; } = false;
    private bool isRowAutoHeight { get; set; } = false;
    private bool EnableTimeScale { get; set; } = true;
    private bool ShowWeekNumber { get; set; } = false;
    private bool isQuickInfoCreated { get; set; } = false;
    private CalendarWeekRule WeekRule { get; set; } = CalendarWeekRule.FirstDay;
    private string WeeklyRule { get; set; } = "Off";
    private string Tooltipvalue { get; set; } = "Off";
    private View CurrentView { get; set; } = View.Week;
    private string SelectedView { get; set; } = "Week";
    private string DayStartHour { get; set; } = "00:00";
    private string DayEndHour { get; set; } = "24:00";
    private string WorkStartHour { get; set; } = "09:00";
    private string WorkEndHour { get; set; } = "18:00";
    private string TimeFormat { get; set; } = "hh:mm tt";
    private bool IsSettingsVisible { get; set; } = false;
    public string[] GroupData = new string[] { "Calendars" };
    private DateTime SystemTime { get; set; } = DateTime.UtcNow;
    private DateTime SelectedDate { get; set; } = DateTime.UtcNow;
    private DateTime? StartWorkHour { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 0, 0);
    private DateTime? EndWorkHour { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0);
    private DateTime? ScheduleStartHour { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
    private DateTime? ScheduleEndHour { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
    private int[] SelectedResource { get; set; } = new int[] { 1 };
    private int[] WorkDays { get; set; } = new int[] { 1, 2, 3, 4, 5 };
    private Timezone TimezoneData { get; set; } = new Timezone() { Name = "UTC+00:00", Key = "UTC", Value = "UTC" };
    private Query ResourceQuery { get; set; } = new Query().Where(new WhereFilter() { Field = "CalendarId", Operator = "equal", value = 1 });
    public List<CalendarData> Calendars { get; set; } = new List<CalendarData> {
        new CalendarData { CalendarName = "My Calendar", CalendarId = 1, CalendarColor = "#c43081" },
        new CalendarData { CalendarName= "Company", CalendarId= 2, CalendarColor= "#ff7f50" },
        new CalendarData { CalendarName= "Birthday", CalendarId= 3, CalendarColor= "#AF27CD" },
        new CalendarData { CalendarName= "Holiday", CalendarId= 4, CalendarColor= "#808000" }
    };

    private List<SlotData> SlotIntervalDataSource = new List<SlotData>()
    {
        new SlotData() { Name = "1 hour", Value = 60 },
        new SlotData() { Name = "1.5 hours", Value = 90 },
        new SlotData() { Name = "2 hours", Value = 120 },
        new SlotData() { Name = "2.5 hours", Value = 150 },
        new SlotData() { Name = "3 hours", Value = 180 },
        new SlotData() { Name = "3.5 hours", Value = 210 },
        new SlotData() { Name = "4 hours", Value = 240 },
        new SlotData() { Name = "4.5 hours", Value = 270 },
        new SlotData() { Name = "5 hours", Value = 300 },
        new SlotData() { Name = "5.5 hours", Value = 330 },
        new SlotData() { Name = "6 hours", Value = 360 },
        new SlotData() { Name = "6.5 hours", Value = 390 },
        new SlotData() { Name = "7 hours", Value = 420 },
        new SlotData() { Name = "7.5 hours", Value = 450 },
        new SlotData() { Name = "8 hours", Value = 480 },
        new SlotData() { Name = "8.5 hours", Value = 510 },
        new SlotData() { Name = "9 hours", Value = 540 },
        new SlotData() { Name = "9.5 hours", Value = 570 },
        new SlotData() { Name = "10 hours", Value = 600 },
        new SlotData() { Name = "10.5 hours", Value = 630 },
        new SlotData() { Name = "11 hours", Value = 660 },
        new SlotData() { Name = "11.5 hours", Value = 690 },
        new SlotData() { Name = "12 hours", Value = 720 }
    };

    private List<SlotData> SlotCountDataSource = new List<SlotData>()
    {
        new SlotData() { Name = "1", Value = 1 },
        new SlotData() { Name = "2", Value = 2 },
        new SlotData() { Name = "3", Value = 3 },
        new SlotData() { Name = "4", Value = 4 },
        new SlotData() { Name = "5", Value = 5 },
        new SlotData() { Name = "6", Value = 6 },
        new SlotData() { Name = "7", Value = 7 },
        new SlotData() { Name = "8", Value = 8 },
        new SlotData() { Name = "9", Value = 9 },
        new SlotData() { Name = "10", Value = 10 }
    };

    private List<TimeFormatData> TimeFormatDataSource = new List<TimeFormatData>()
{
        new TimeFormatData() { Name = "12 hours", Value = "hh:mm tt" },
        new TimeFormatData() { Name = "24 hours", Value = "HH:mm" }
    };

    private List<WeekNumbers> WeekNumbersData = new List<WeekNumbers>()
    {
        new WeekNumbers() { Name = "Off", Value = "Off" },
        new WeekNumbers() { Name = "First Day Of Year", Value = "FirstDay" },
        new WeekNumbers() { Name = "First Full Week", Value = "FirstFullWeek" },
        new WeekNumbers() { Name = "First Four-Day Week", Value = "FirstFourDayWeek" }
    };

    private List<Tooltip> TooltipData = new List<Tooltip>()
{
        new Tooltip() { Name = "Off", Value = "Off" },
        new Tooltip() { Name = "On", Value = "On" }
    };

    private List<string> ScheduleViews { get; set; } = new List<string>() { "Day", "Week", "WorkWeek", "Month", "Year", "Agenda", "TimelineDay", "TimelineWeek", "TimelineWorkWeek", "TimelineMonth", "TimelineYear" };

    private List<WeekDays> WeekCollection { get; set; } = new List<WeekDays>() {
        new WeekDays () { Name = "Sunday", Value = 0 },
        new WeekDays () { Name = "Monday", Value = 1 },
        new WeekDays () { Name = "Tuesday", Value = 2 },
        new WeekDays () { Name = "Wednesday", Value = 3 },
        new WeekDays () { Name = "Thursday", Value = 4 },
        new WeekDays () { Name = "Friday", Value = 5 },
        new WeekDays () { Name = "Saturday", Value = 6 }
    };

    private List<Timezone> TimezoneCollection { get; set; } = new List<Timezone>() {
        new Timezone () { Name = "UTC-08:00", Key = "Pacific Standard Time", Value = "America/Los_Angeles" },
        new Timezone () { Name = "UTC-07:00", Key = "Mountain Standard Time", Value = "America/Denver" },
        new Timezone () { Name = "UTC-06:00", Key = "Central Standard Time", Value = "America/Chicago" },
        new Timezone () { Name = "UTC-05:00", Key = "Eastern Standard Time", Value = "America/New_York" },
        new Timezone () { Name = "UTC-04:00", Key = "Atlantic Standard Time", Value = "Atlantic/Bermuda" },
        new Timezone () { Name = "UTC-03:00", Key = "Greenland Standard Time", Value = "Atlantic/Stanley" },
        new Timezone () { Name = "UTC-02:00", Key = "Mid-Atlantic Standard Time", Value = "America/Sao_Paulo" },
        new Timezone () { Name = "UTC-01:00", Key = "Cape Verde Standard Time", Value = "Atlantic/Cape_Verde" },
        new Timezone () { Name = "UTC+00:00", Key = "UTC", Value = "UTC" },
        new Timezone () { Name = "UTC+01:00", Key = "Romance Standard Time", Value = "Europe/Paris" },
        new Timezone () { Name = "UTC+03:00", Key = "Russian Standard Time", Value = "Europe/Moscow" },
        new Timezone () { Name = "UTC+05:30", Key = "India Standard Time", Value = "Asia/Kolkata" },
        new Timezone () { Name = "UTC+08:00", Key = "W. Australia Standard Time", Value = "Australia/Perth" },
        new Timezone () { Name = "UTC+10:00", Key = "E. Australia Standard Time", Value = "Australia/Brisbane" },
        new Timezone () { Name = "UTC+10:30", Key = "Lord Howe Standard Time", Value = "Australia/Adelaide" },
        new Timezone () { Name = "UTC+13:00", Key = "New Zealand Standard Time", Value = "Pacific/Auckland" }
    };

    private Dictionary<string, object> htmlAttribute = new Dictionary<string, object>() {
        {"tabindex", "-1" }
    };

    private void OnViewChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
    {
        switch (this.CurrentView)
        {
            case View.Day:
            case View.TimelineDay:
                this.CurrentView = args.Checked ? View.TimelineDay : View.Day;
                break;
            case View.Week:
            case View.TimelineWeek:
                this.CurrentView = args.Checked ? View.TimelineWeek : View.Week;
                break;
            case View.WorkWeek:
            case View.TimelineWorkWeek:
                this.CurrentView = args.Checked ? View.TimelineWorkWeek : View.WorkWeek;
                break;
            case View.Month:
            case View.TimelineMonth:
                this.CurrentView = args.Checked ? View.TimelineMonth : View.Month;
                break;
            case View.Year:
            case View.TimelineYear:
                this.CurrentView = args.Checked ? View.TimelineYear : View.Year;
                break;
            case View.Agenda:
                this.CurrentView = View.Agenda;
                break;
        }
    }

    private async void OnNewEventAdd()
    {
        DateTime Date = this.ScheduleRef.SelectedDate;
        DateTime Start = new DateTime(Date.Year, Date.Month, Date.Day, DateTime.Now.Hour, 0, 0);
        AppointmentDto eventData = new AppointmentDto
        {
            Id = await ScheduleRef.GetMaxEventIdAsync<int>(),
            Subject = "Add title",
            StartDateTime = Start,
            EndDateTime = Start.AddHours(1),
            Location = "",
            Description = "",
            IsAllDay = false,
            CalendarId = this.ResourceRef.Value[0]
        };
        await ScheduleRef.OpenEditorAsync(eventData, CurrentAction.Add);
    }

    private async void OnNewRecurringEventAdd()
    {
        DateTime Date = this.ScheduleRef.SelectedDate;
        DateTime Start = new DateTime(Date.Year, Date.Month, Date.Day, DateTime.Now.Hour, 0, 0);
        AppointmentDto eventData = new AppointmentDto
        {
            Id = await ScheduleRef.GetMaxEventIdAsync<int>(),
            Subject = "Add title",
            StartDateTime = Start,
            EndDateTime = Start.AddHours(1),
            Location = "",
            Description = "",
            IsAllDay = false,
            CalendarId = this.ResourceRef.Value[0],
            RecurrenceRule = "FREQ=DAILY;INTERVAL=1;"
        };
        await ScheduleRef.OpenEditorAsync(eventData, CurrentAction.Add);
    }

    private void OnDayView()
    {
        this.CurrentView = this.ViewRef.Checked ? View.TimelineDay : View.Day;
    }

    private void OnWeekView()
    {
        this.CurrentView = this.ViewRef.Checked ? View.TimelineWeek : View.Week;
    }

    private void OnWorkWeekView()
    {
        this.CurrentView = this.ViewRef.Checked ? View.TimelineWorkWeek : View.WorkWeek;
    }

    private void OnMonthView()
    {
        this.CurrentView = this.ViewRef.Checked ? View.TimelineMonth : View.Month;
    }

    private void OnYearView()
    {
        this.CurrentView = this.ViewRef.Checked ? View.TimelineYear : View.Year;
    }

    private void OnAgendaView()
    {
        this.CurrentView = View.Agenda;
    }

    private async void OnSettingsClick()
    {
        this.IsSettingsVisible = !this.IsSettingsVisible;
        StateHasChanged();
        await this.ScheduleRef.RefreshEventsAsync();
    }

    private string GetEventDetails(AppointmentDto data)
    {
        return data.StartDateTime.ToString("dddd dd, MMMM yyyy", CultureInfo.InvariantCulture) + " (" + data.StartDateTime.ToString(TimeFormat, CultureInfo.InvariantCulture) + "-" + data.EndDateTime.ToString(TimeFormat, CultureInfo.InvariantCulture) + ")";
    }

    private string GetHeaderStyles(AppointmentDto data)
    {
        if (data.Id == default(int))
        {
            return "align-items: center ; color: #919191;";
        }
        else
        {
            CalendarData resData = GetResourceData(data);
            return "background:" + (resData == null ? "#007bff" : resData.CalendarColor) + "; color: #FFFFFF;";
        }
    }

    private async Task SetFocus()
    {
        if (isQuickInfoCreated)
        {
            await Task.Delay(20);
            await SubjectRef.FocusAsync();
        }
    }

    private async Task OnQuickInfoSubjectCreated()
    {
        await Task.Yield();
        await SubjectRef.FocusAsync();
        isQuickInfoCreated = true;
    }

    public void OnToolbarCreated()
    {
        System.Timers.Timer timer = new (1000);
        timer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) =>
        {
            string key = this.TimezoneData.Key ?? "UTC";
            SystemTime = this.TimeConvertor(key);
            ScheduleRef?.PreventRender();
            InvokeAsync(() => { StateHasChanged(); });
        });
        timer.Enabled = true;
    }

    private CalendarData GetResourceData(AppointmentDto data)
    {
        if (data.CalendarId != 0)
        {
            int resourceId = SelectedResource.Where(item => item == data.CalendarId).FirstOrDefault();
            CalendarData resourceData = this.Calendars.Where(item => item.CalendarId == resourceId).FirstOrDefault();
            return resourceData;
        }
        return null;
    }

    private DateTime TimeConvertor(string TimeZoneId)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId));
    }

    private async void OnMoreDetailsClick(MouseEventArgs args, AppointmentDto data, bool isEventData)
    {
        await ScheduleRef.CloseQuickInfoPopupAsync();
        if (isEventData == false)
        {
            AppointmentDto eventData = new AppointmentDto
            {
                Id = await ScheduleRef.GetMaxEventIdAsync<int>(),
                Subject = SubjectRef.Value ?? "",
                StartDateTime = data.StartDateTime,
                EndDateTime = data.EndDateTime,
                Location = data.Location,
                Description = DescriptionRef.Value ?? "",
                IsAllDay = data.IsAllDay,
                CalendarId = CalendarRef.Value,
                RecurrenceException = data.RecurrenceException,
                RecurrenceID = data.RecurrenceID,
                RecurrenceRule = data.RecurrenceRule
            };
            await ScheduleRef.OpenEditorAsync(eventData, CurrentAction.Add);
        }
        else
        {
            AppointmentDto eventData = new AppointmentDto
            {
                Id = data.Id,
                Subject = data.Subject,
                Location = data.Location,
                Description = data.Description,
                StartDateTime = data.StartDateTime,
                EndDateTime = data.EndDateTime,
                IsAllDay = data.IsAllDay,
                CalendarId = data.CalendarId,
                RecurrenceException = data.RecurrenceException,
                RecurrenceID = data.RecurrenceID,
                RecurrenceRule = data.RecurrenceRule
            };
            if (!string.IsNullOrEmpty(eventData.RecurrenceRule))
            {
                await ScheduleRef.OpenEditorAsync(eventData, CurrentAction.EditOccurrence);
            }
            else
            {
                await ScheduleRef.OpenEditorAsync(eventData, CurrentAction.Save);
            }
        }
    }

    private async Task OnDelete(AppointmentDto data)
    {
        await ScheduleRef.CloseQuickInfoPopupAsync();
        await ScheduleRef.DeleteEventAsync(data, !string.IsNullOrEmpty(data.RecurrenceRule) ? CurrentAction.DeleteOccurrence : CurrentAction.Delete);
    }

    private async Task OnAdd(MouseEventArgs args, AppointmentDto data)
    {
        await ScheduleRef.CloseQuickInfoPopupAsync();
        AppointmentDto cloneData = new AppointmentDto
        {
            Id = await ScheduleRef.GetMaxEventIdAsync<int>(),
            Subject = SubjectRef.Value ?? "Add title",
            Description = DescriptionRef.Value ?? "Add notes",
            StartDateTime = data.StartDateTime,
            EndDateTime = data.EndDateTime,
            CalendarId = CalendarRef.Value,
            Location = data.Location,
            IsAllDay = data.IsAllDay,
            RecurrenceException = data.RecurrenceException,
            RecurrenceID = data.RecurrenceID,
            RecurrenceRule = data.RecurrenceRule
        };
        await ScheduleRef.AddEventAsync(cloneData);
    }

    public void OnWeekNumberChange(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, WeekNumbers> args)
    {
        switch (args.Value)
        {
            case "Off":
                this.ShowWeekNumber = false;
                break;
            case "FirstDay":
                this.ShowWeekNumber = true;
                this.WeekRule = CalendarWeekRule.FirstDay;
                break;
            case "FirstFullWeek":
                this.ShowWeekNumber = true;
                this.WeekRule = CalendarWeekRule.FirstFullWeek;
                break;
            case "FirstFourDayWeek":
                this.ShowWeekNumber = true;
                this.WeekRule = CalendarWeekRule.FirstFourDayWeek;
                break;
        }
    }

    public void OnWeekDaysChange(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, WeekDays> args)
    {
        this.FirstDayOfWeek = args.Value;
    }

    public void OnWorkDaysChange(Syncfusion.Blazor.DropDowns.MultiSelectChangeEventArgs<int[]> args)
    {
        this.WorkDays = args.Value;
    }

    public void OnDayStartHourChange(Syncfusion.Blazor.Calendars.ChangeEventArgs<DateTime?> args)
    {
        this.DayStartHour = args.Text;
    }

    public void OnDayEndHourChange(Syncfusion.Blazor.Calendars.ChangeEventArgs<DateTime?> args)
    {
        this.DayEndHour = args.Text;
    }

    public void OnWorkStartHourChange(Syncfusion.Blazor.Calendars.ChangeEventArgs<DateTime?> args)
    {
        this.WorkStartHour = args.Text;
    }

    public void OnWorkEndHourChange(Syncfusion.Blazor.Calendars.ChangeEventArgs<DateTime?> args)
    {
        this.WorkEndHour = args.Text;
    }

    public void OnTimezoneChange(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, Timezone> args)
    {
        this.TimezoneData = args.ItemData;
        var zones = TimeZoneInfo.GetSystemTimeZones();
        SystemTime = this.TimeConvertor(this.TimezoneData.Key);
    }

    public void OnResourceChange(Syncfusion.Blazor.DropDowns.MultiSelectChangeEventArgs<int[]> args)
    {
        WhereFilter predicate = new WhereFilter();
        if (args.Value != null)
        {
            predicate = new WhereFilter() { Field = "CalendarId", Operator = "equal", value = args.Value.Count() > 0 ? args.Value[0] : 0 }.
                Or(new WhereFilter() { Field = "CalendarId", Operator = "equal", value = args.Value.Count() > 1 ? args.Value[1] : 0 }).
                Or(new WhereFilter() { Field = "CalendarId", Operator = "equal", value = args.Value.Count() > 2 ? args.Value[2] : 0 }).
                Or(new WhereFilter() { Field = "CalendarId", Operator = "equal", value = args.Value.Count() > 3 ? args.Value[3] : 0 });
        }
        else
        {
            predicate = new WhereFilter() { Field = "CalendarId", Operator = "equal", value = 1 };
        }
        this.ResourceQuery = new Query().Where(predicate);
    }

    public void OnGroupChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
    {
        this.EnableGroup = args.Checked;
        this.GroupData = args.Checked ? new string[] { "Calendars" } : null;
    }

    public void OnTimeScaleChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
    {
        this.EnableTimeScale = args.Checked;
    }

    public void OnRowAutoHeightChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
    {
        this.isRowAutoHeight = args.Checked;
    }

    public void OnTooltipChange(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, Tooltip> args)
    {
        switch (args.Value)
        {
            case "Off":
                this.TooltipEnable = false;
                break;
            case "On":
                this.TooltipEnable = true;
                break;
        }
    }

    public void OnSlotIntervalChange(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, SlotData> args)
    {
        this.SlotInterval = args.Value;
    }

    public void OnSlotCountChange(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, SlotData> args)
    {
        this.SlotCount = args.Value;
    }

    public async Task OnFileUploadChange(UploadChangeEventArgs args)
    {
        foreach (Syncfusion.Blazor.Inputs.UploadFiles file in args.Files)
        {
            StreamReader reader = new StreamReader(file.File.OpenReadStream(long.MaxValue));
            string fileContent = await reader.ReadToEndAsync();
            await ScheduleRef.ImportICalendarAsync(fileContent);
        }
    }

    public async void OnPrintClick()
    {
        await ScheduleRef.PrintAsync();
    }

    public async void OnExportClick(Syncfusion.Blazor.SplitButtons.MenuEventArgs args)
    {
        if (args.Item.Text == "Excel")
        {
            List<AppointmentDto> ExportDatas = new List<AppointmentDto>();
            List<AppointmentDto> EventCollection = await ScheduleRef.GetEventsAsync();
            List<Syncfusion.Blazor.Schedule.Resource> ResourceCollection = ScheduleRef.GetResourceCollections();
            List<CalendarData> ResourceData = ResourceCollection[0].DataSource as List<CalendarData>;
            for (int a = 0, count = ResourceData.Count(); a < count; a++)
            {
                List<AppointmentDto> datas = EventCollection.Where(e => e.CalendarId == ResourceData[a].CalendarId).ToList();
                foreach (AppointmentDto data in datas)
                {
                    ExportDatas.Add(data);
                }
            }
            ExportOptions Options = new ExportOptions()
            {
                ExportType = ExcelFormat.Xlsx,
                CustomData = ExportDatas,
                Fields = new string[] { "Id", "Subject", "StartDateTime", "EndDateTime", "CalendarId" }
            };
            await ScheduleRef.ExportToExcelAsync(Options);
        }
        else
        {
            await ScheduleRef.ExportToICalendarAsync();
        }
    }

    public async Task OnOpen(BeforeOpenCloseMenuEventArgs<MenuItem> args)
    {
        if (args.ParentItem == null)
        {
            CellData = await ScheduleRef.GetTargetCellAsync((int)args.Left, (int)args.Top);
            await ScheduleRef.CloseQuickInfoPopupAsync();
            if (CellData == null)
            {
                EventData = await ScheduleRef.GetTargetEventAsync((int)args.Left, (int)args.Top);
                if (EventData.Id == 0)
                {
                    args.Cancel = true;
                }
                if (EventData.RecurrenceRule != null)
                {
                    isCell = isEvent = true;
                    isRecurrence = false;
                }
                else
                {
                    isCell = isRecurrence = true;
                    isEvent = false;
                }
            }
            else
            {
                isCell = false;
                isEvent = isRecurrence = true;
            }
        }
    }

    public async Task OnItemSelected(MenuEventArgs<MenuItem> args)
    {
        var SelectedMenuItem = args.Item.Id;
        var ActiveCellsData = await ScheduleRef.GetSelectedCellsAsync();
        if (ActiveCellsData == null)
        {
            ActiveCellsData = CellData;
        }
        switch (SelectedMenuItem)
        {
            case "Today":
                string key = this.TimezoneData.Key ?? "UTC";
                SelectedDate = this.TimeConvertor(key);
                break;
            case "Add":
                await ScheduleRef.OpenEditorAsync(ActiveCellsData, CurrentAction.Add);
                break;
            case "AddRecurrence":
                await ScheduleRef.OpenEditorAsync(ActiveCellsData, CurrentAction.Add, RepeatType.Daily);
                break;
            case "Save":
                await ScheduleRef.OpenEditorAsync(EventData, CurrentAction.Save);
                break;
            case "EditOccurrence":
                await ScheduleRef.OpenEditorAsync(EventData, CurrentAction.EditOccurrence);
                break;
            case "EditSeries":
                List<AppointmentDto> Events = await ScheduleRef.GetEventsAsync();
                EventData = (AppointmentDto)Events.Where(data => data.Id == EventData.RecurrenceID).FirstOrDefault();
                await ScheduleRef.OpenEditorAsync(EventData, CurrentAction.EditSeries);
                break;
            case "Delete":
                await ScheduleRef.DeleteEventAsync(EventData);
                break;
            case "DeleteOccurrence":
                await ScheduleRef.DeleteEventAsync(EventData, CurrentAction.DeleteOccurrence);
                break;
            case "DeleteSeries":
                await ScheduleRef.DeleteEventAsync(EventData, CurrentAction.DeleteSeries);
                break;
        }
    }
}

public class AppointmentViewModel : AppointmentUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}