using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Security;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Employers;
public partial class Employers
{
    [Parameter]
    public Guid EmployeeId { get; set; } = Guid.Empty;
    [Inject]
    protected IEmployersClient Client { get; set; } = default!;

    protected EntityServerTableContext<EmployerDto, Guid, EmployerViewModel> Context { get; set; } = default!;

    private EntityTable<EmployerDto, Guid, EmployerViewModel>? _table;

    private string? _searchString;

    protected override void OnParametersSet()
    {
        if (EmployeeId != Guid.Empty)
        {
            _searchEmployeeId = EmployeeId;
        }
    }

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Employer",
            entityNamePlural: "Employers",
            entityResource: FSHResource.Employees,
            fields: new()
            {
                new(data => data.EmployeeName, "Employee", visible: false),
                new(data => data.Name, "Name", "Name", Template: TemplateNameAddress),
                new(data => data.Designation, "Designation", "Designation"),
                new(data => data.StartDate, "StartDate", "StartDate", typeof(DateOnly)),
                new(data => data.EndDate, "EndDate", "EndDate", typeof(DateOnly)),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<EmployerSearchRequest>();
                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;
                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<EmployerDto>>();
            },
            createFunc: async data =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Name}_{Guid.NewGuid():N}" };
                }

                data.EmployeeId = _searchEmployeeId;

                await Client.CreateAsync(data.Adapt<EmployerCreateRequest>());
                data.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, Employer) =>
            {
                if (!string.IsNullOrEmpty(Employer.ImageInBytes))
                {
                    Employer.DeleteCurrentImage = true;
                    Employer.Image = new FileUploadRequest() { Data = Employer.ImageInBytes, Extension = Employer.ImageExtension ?? string.Empty, Name = $"{Employer.Name}_{Guid.NewGuid():N}" };
                }

                await Client.UpdateAsync(id, Employer.Adapt<EmployerUpdateRequest>());
                Employer.ImageInBytes = string.Empty;
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);

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

public class EmployerViewModel : EmployerUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}