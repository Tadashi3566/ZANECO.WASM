using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Dependents;
public partial class Dependents
{
    [Parameter]
    public Guid EmployeeId { get; set; } = Guid.Empty;
    [Inject]
    protected IDependentsClient Client { get; set; } = default!;

    protected EntityServerTableContext<DependentDto, Guid, DependentViewModel> Context { get; set; } = default!;

    private EntityTable<DependentDto, Guid, DependentViewModel> _table = default!;

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
            entityName: "Dependent",
            entityNamePlural: "Dependents",
            entityResource: FSHResource.Dependents,
            fields: new()
            {
                new(data => data.EmployeeName, "Employee", "Employee"),
                new(data => data.Name, "Name", "Name", Template: TemplateNameGender),
                new(data => data.Gender, "Gender", visible: false),
                new(data => data.BirthDate, "Birt hDate", "BirthDate", typeof(DateOnly)),
                new(data => data.Relation, "Relation", "Relation"),
                new(data => data.Description, "Description", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<DependentSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<DependentDto>>();
            },
            createFunc: async data =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Name}_{Guid.NewGuid():N}" };
                }

                data.EmployeeId = _searchEmployeeId;

                await Client.CreateAsync(data.Adapt<DependentCreateRequest>());
                data.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, Dependent) =>
            {
                if (!string.IsNullOrEmpty(Dependent.ImageInBytes))
                {
                    Dependent.DeleteCurrentImage = true;
                    Dependent.Image = new FileUploadRequest() { Data = Dependent.ImageInBytes, Extension = Dependent.ImageExtension ?? string.Empty, Name = $"{Dependent.Name}_{Guid.NewGuid():N}" };
                }

                await Client.UpdateAsync(id, Dependent.Adapt<DependentUpdateRequest>());
                Dependent.ImageInBytes = string.Empty;
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

public class DependentViewModel : DependentUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}