﻿using Mapster;
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
                new(data => data.EmployeeName, "Name", "EmployeeName", visible: EmployeeId.Equals(Guid.Empty)),
                new(data => data.AttendanceDate, "Date", "AttendanceDate", typeof(DateOnly)),
                new(data => data.ScheduleDetailDay, "Day", "ScheduleDetailDay"),
                new(data => data.ActualTimeIn1, "Actual TimeIn1", "ActualTimeIn1", Template: TemplateImageTimeIn1),
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
            enableAdvancedSearch: true,
            idFunc: Attendance => Attendance.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<AttendanceSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;
                filter.PayrollId = PayrollId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<AttendanceDto>>();
            },

            // createFunc: async data =>
            // {
            //    data.EmployeeId = SearchEmployeeId;
            //    await Client.CreateAsync(data.Adapt<AttendanceCreateRequest>());
            //    data.ImageInBytes = string.Empty;
            // },

            updateFunc: async (id, data) =>
            {
                data.EmployeeId = _searchEmployeeId;

                if (!string.IsNullOrEmpty(data.ImageBytes))
                {
                    data.DeleteCurrentImageIn1 = true;
                    data.ImageIn1 = new FileUploadRequest() { Data = data.ImageBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.EmployeeId}_{Guid.NewGuid():N}" };
                }

                await Client.UpdateAsync(id, data);
                await Client.UpdateAsync(id, data.Adapt<AttendanceUpdateRequest>());

                data.ImageBytes = string.Empty;
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
            _table.ReloadDataAsync();
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
            Context.AddEditModal.RequestModel.ImageBytes = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            Context.AddEditModal.ForceRender();
        }
    }

    private void ClearImageInBytes()
    {
        Context.AddEditModal.RequestModel.ImageBytes = string.Empty;
        Context.AddEditModal.ForceRender();
    }

    private void SetDeleteCurrentImageFlag()
    {
        Context.AddEditModal.RequestModel.ImageBytes = string.Empty;
        Context.AddEditModal.RequestModel.ImagePathIn1 = string.Empty;
        Context.AddEditModal.RequestModel.DeleteCurrentImageIn1 = true;
        Context.AddEditModal.ForceRender();
    }
}

public class AttendanceViewModel : AttendanceUpdateRequest
{
    public string? ImagePathIn1 { get; set; }
    public string? ImageBytes { get; set; }
    public string? ImageExtension { get; set; }
}