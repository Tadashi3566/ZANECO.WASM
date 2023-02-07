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
    public Guid PayrollId { get; set; } = Guid.Empty;
    [Inject]
    protected IAttendanceClient Client { get; set; } = default!;
    protected IPayrollClient ClientPayroll { get; set; } = default!;

    protected EntityServerTableContext<AttendanceDto, Guid, AttendanceViewModel> Context { get; set; } = default!;

    private EntityTable<AttendanceDto, Guid, AttendanceViewModel> _table = default!;

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
            entityNamePlural: "Attendances",
            entityResource: FSHResource.Attendance,
            fields: new()
            {
                new(data => data.EmployeeName, "Name", "EmployeeName"),
                new(data => data.AttendanceDate, "Date", "AttendanceDate", typeof(DateOnly)),
                new(data => data.ScheduleDetailDay, "Day", "ScheduleDetailDay"),
                new(data => data.ActualTimeIn1, "Actual In 1", "ActualTimeIn1"),
                new(data => data.ActualTimeOut1, "Actual Out 1", "ActualTimeOut1"),
                new(data => data.ActualTimeIn2, "Actual In 2", "ActualTimeIn2"),
                new(data => data.ActualTimeOut2, "Actual Out 2", "ActualTimeOut2"),
                new(data => data.LateMinutes, "Late (min)", "MinutesLate"),
                new(data => data.UnderTimeMinutes, "Under (min)", "UnderTimeMinutes"),
                new(data => data.TotalHours, "Total Hours", "TotalHours"),
                new(data => data.PaidHours, "Paid Hours", "PaidHours"),
                new(data => data.Status, "Status", "Status"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            idFunc: Attendance => Attendance.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<AttendanceSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;
                filter.PayrollId = PayrollId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<AttendanceDto>>();
            },
            createFunc: async data =>
            {
                data.EmployeeId = SearchEmployeeId;

                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Id}_{Guid.NewGuid():N}" };
                }

                await Client.CreateAsync(data.Adapt<AttendanceCreateRequest>());
                data.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, data) =>
            {
                data.EmployeeId = SearchEmployeeId;

                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.DeleteCurrentImage = true;
                    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Id}_{Guid.NewGuid():N}" };
                }

                //await Client.UpdateAsync(id, data);
                await Client.UpdateAsync(id, data.Adapt<AttendanceUpdateRequest>());
                data.ImageInBytes = string.Empty;
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
            _ = _table.ReloadDataAsync();
        }
    }

    // TODO : Make this as a shared service or something? Since it's used by Profile Component also for now, and literally any other component that will have image upload.
    // The new service should ideally return $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}"
    private async Task UploadFiles(InputFileChangeEventArgs e)
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

public class AttendanceViewModel : AttendanceUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}