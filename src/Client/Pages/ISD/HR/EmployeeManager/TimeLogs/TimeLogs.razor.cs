using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.TimeLogs;

public partial class TimeLogs
{
    [Parameter]
    public Guid EmployeeId { get; set; } = Guid.Empty;

    [Parameter]
    public DateTime LogDate { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    [Inject]
    protected ITimeLogsClient Client { get; set; } = default!;

    protected EntityServerTableContext<TimeLogDto, Guid, TimeLogViewModel> Context { get; set; } = default!;

    [Inject]
    protected IPersonalClient User { get; set; } = default!;

    private EntityTable<TimeLogDto, Guid, TimeLogViewModel>? _table;

    private string? _searchString;
    private bool _canViewEmployees;
    private bool _canUpdateAttendance;

    private DateTime _startDate = DateTime.Today;
    private DateTime _endDate = DateTime.Today;

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
        _canUpdateAttendance = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.Attendance);

        Context = new(
            entityName: "Time Log",
            entityNamePlural: "Time Logs",
            entityResource: FSHResource.Attendance,
            fields: new()
            {
                new(data => data.ImagePath, "Image", "ImagePath", Template: TemplateImage),
                new(data => data.LogType, "Type", "LogType"),
                new(data => data.LogDate, "Date", "LogDate", typeof(DateOnly)),
                new(data => data.LogDateTime, "Time", "LogDateTime", typeof(TimeOnly)),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            idFunc: TimeLog => TimeLog.Id,
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

                var filter = _filter.Adapt<TimeLogSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;
                filter.StartDate = _startDate;
                filter.EndDate = _endDate;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<TimeLogDto>>();
            },
            createFunc: async data =>
            {
                data.EmployeeId = SearchEmployeeId;
                data.LogDate = DateTime.Today;
                data.LogDateTime = DateTime.Now;

                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new ImageUploadRequest()
                    {
                        Data = data.ImageInBytes,
                        Extension = data.ImageExtension ?? string.Empty,
                        EmployeeId = $"{data.EmployeeId}",
                        Name = $"{data.EmployeeId}_{data.LogDate:yyyy_MM_dd}_{data.LogType}_{Guid.NewGuid():N}"
                    };
                }

                await Client.CreateAsync(data.Adapt<TimeLogCreateRequest>());
                data.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, data) =>
            {
                data.EmployeeId = _searchEmployeeId;

                await Client.UpdateAsync(id, data.Adapt<TimeLogUpdateRequest>());
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
    }

    private List<BreadcrumbItem> _breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
        new BreadcrumbItem("Employees", href: "/hr/employees", icon: Icons.Material.Filled.Groups),
        new BreadcrumbItem("Attendance", href: "/employee/attendance", icon: Icons.Material.Filled.CalendarMonth),
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

    private DateTime? _StartDate = DateTime.Today; // new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    private DateTime? StartDate
    {
        get => _StartDate;
        set
        {
            _StartDate = value;
            _table!.ReloadDataAsync();
        }
    }

    private DateTime? _EndDate = DateTime.Today; // new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

    private DateTime? EndDate
    {
        get => _EndDate;
        set
        {
            _EndDate = value;
            _table!.ReloadDataAsync();
        }
    }

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
}

public class TimeLogViewModel : TimeLogUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}