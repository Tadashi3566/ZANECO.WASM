using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.TimeLogs;
public partial class TimeLogs
{
    [Parameter]
    public Guid EmployeeId { get; set; } = Guid.Empty;
    [Parameter]
    public DateTime LogDate { get; set; }
    [Inject]
    protected ITimeLogsClient Client { get; set; } = default!;

    protected EntityServerTableContext<TimeLogDto, Guid, TimeLogViewModel> Context { get; set; } = default!;

    private EntityTable<TimeLogDto, Guid, TimeLogViewModel>? _table;

    private string? _searchString;
    private DateTime? _logDate = DateTime.Today;
    private TimeSpan? _logTime;

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
            entityName: "TimeLog",
            entityNamePlural: "TimeLogs",
            entityResource: FSHResource.Attendance,
            fields: new()
            {
                new(data => data.ImagePath, "Image", "ImagePath", Template: TemplateImage),
                //new(data => data.EmployeeId, "Name", "EmployeeName", visible: EmployeeId.Equals(Guid.Empty)),
                new(data => data.LogType, "Type", "LogType"),
                new(data => data.LogDate, "Date", "LogDate", typeof(DateOnly)),
                new(data => data.LogDateTime, "Time", "LogDateTime", typeof(TimeOnly)),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", "Notes", visible: false),
            },
            enableAdvancedSearch: true,
            idFunc: TimeLog => TimeLog.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<TimeLogSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

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
                    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.EmployeeId}_{data.LogDate:yyyy_MM_dd}_{data.LogType}_{Guid.NewGuid():N}" };
                }

                await Client.CreateAsync(data.Adapt<TimeLogCreateRequest>());
                data.ImageInBytes = string.Empty;
            },

            updateFunc: async (id, data) =>
            {
                data.EmployeeId = _searchEmployeeId;
                data.LogDate = _logDate;
                data.LogDateTime = _logDate + _logTime;

                //if (!string.IsNullOrEmpty(data.ImageInBytes))
                //{
                //    data.DeleteCurrentImage = true;
                //    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.EmployeeId}_{Guid.NewGuid():N}" };
                //}

                //await Client.UpdateAsync(id, data);

                await Client.UpdateAsync(id, data.Adapt<TimeLogUpdateRequest>());

                //data.ImageInBytes = string.Empty;
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
            _table!.ReloadDataAsync();
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

public class TimeLogViewModel : TimeLogUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}