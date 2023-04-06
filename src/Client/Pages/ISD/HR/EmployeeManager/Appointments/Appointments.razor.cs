using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Syncfusion.Blazor.Calendars;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
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

    //private DateTime _startDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 8,0,0);
    //private DateTime _endDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 17,0,0);

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

                //request.StartDateTime = _startDateTime;
                //request.EndDateTime = _endDateTime;

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

                //request.StartDateTime = _startDateTime;
                //request.EndDateTime = _endDateTime;

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

    private List<BreadcrumbItem> _breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
        new BreadcrumbItem("Employees", href: "/hr/employees", icon: Icons.Material.Filled.Groups),
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
}

public class AppointmentViewModel : AppointmentUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}