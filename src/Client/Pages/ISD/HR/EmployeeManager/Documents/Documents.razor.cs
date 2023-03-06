using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Documents;

public partial class Documents
{
    [Parameter]
    public Guid EmployeeId { get; set; } = Guid.Empty;
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IDocumentsClient Client { get; set; } = default!;
    protected EntityServerTableContext<DocumentDto, Guid, DocumentViewModel> Context { get; set; } = default!;

    private EntityTable<DocumentDto, Guid, DocumentViewModel>? _table;

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
        entityName: "Document",
        entityNamePlural: "Documents",
        entityResource: FSHResource.Documents,
        fields: new()
        {
            new(data => data.EmployeeName, "Employee", "EmployeeName", visible: EmployeeId.Equals(Guid.Empty), Template: TemplateEmployee),
            new(data => data.ImagePath, "Image", Template: TemplateImage),
            new(data => data.Reference, "Reference", "Reference", Template: TemplateDateReference),
            new(data => data.Name, "Name", "Name"),
            new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            new(data => data.Notes, visible: false),
        },
        enableAdvancedSearch: false,
        idFunc: data => data.Id,
        searchFunc: async _filter =>
        {
            var filter = _filter.Adapt<DocumentSearchRequest>();

            filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

            var result = await Client.SearchAsync(filter);
            return result.Adapt<PaginationResponse<DocumentDto>>();
        },
        createFunc: async data =>
        {
            if (SearchEmployeeId.Equals(Guid.Empty))
            {
                Snackbar.Add("The document should be bind with an Employee", Severity.Error);
                return;
            }

            data.EmployeeId = SearchEmployeeId;

            if (data.ImageInBytes is not null)
            {
                data.Image = new ImageUploadRequest()
                {
                    Data = data.ImageInBytes,
                    Extension = data.ImageExtension ?? string.Empty,
                    Name = $"{data.Name}_{Guid.NewGuid():N}",
                    EmployeeId = SearchEmployeeId.ToString(),
                    DateStr = $"{data.DocumentDate:yyyy-MM-dd}"
                };
            }

            await Client.CreateAsync(data.Adapt<DocumentCreateRequest>());

            data.ImageInBytes = null;
        },
        updateFunc: async (id, data) =>
        {
            if (data.ImageInBytes is not null)
            {
                data.DeleteCurrentImage = true;
                data.Image = new ImageUploadRequest()
                {
                    Data = data.ImageInBytes,
                    Extension = data.ImageExtension ?? string.Empty,
                    Name = $"{data.Name}_{Guid.NewGuid():N}",
                    EmployeeId = SearchEmployeeId.ToString(),
                    DateStr = $"{data.DocumentDate:yyyy-MM-dd}"
                };
            }

            await Client.UpdateAsync(id, data.Adapt<DocumentUpdateRequest>());

            data.ImageInBytes = null;
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

public class DocumentViewModel : DocumentUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}