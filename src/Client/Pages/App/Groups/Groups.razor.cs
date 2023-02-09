using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.App.Groups;
public partial class Groups
{
    [Inject]
    protected IGroupsClient Client { get; set; } = default!;

    protected EntityServerTableContext<GroupDto, Guid, GroupViewModel> Context { get; set; } = default!;

    private EntityTable<GroupDto, Guid, GroupViewModel> _table = default!;

    private string? _searchString;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Group",
            entityNamePlural: "Groups",
            entityResource: FSHResource.Groups,
            fields: new()
            {
                new(data => data.Application, "Application", "Application"),
                new(data => data.Parent, "Parent", "Parent", Template: TemplateParentTag),
                new(data => data.Tag, "Tag", visible: false),
                new(data => data.Number, "Number", "Number"),
                new(data => data.Name, "Name", "Name", Template: TemplateCodeName),
                new(data => data.Amount, "Amount", "Amount", typeof(decimal)),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: true,
            idFunc: Group => Group.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<GroupSearchRequest>()))
                .Adapt<PaginationResponse<GroupDto>>(),
            createFunc: async group => await Client.CreateAsync(group.Adapt<GroupCreateRequest>()),
            updateFunc: async (id, group) => await Client.UpdateAsync(id, group),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);

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

public class GroupViewModel : GroupUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}