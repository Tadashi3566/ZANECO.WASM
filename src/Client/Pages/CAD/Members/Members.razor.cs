using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.CAD.Members;
public partial class Members
{
    [Inject]
    protected IMembersClient Client { get; set; } = default!;

    protected EntityServerTableContext<MemberDto, Guid, MemberViewModel> Context { get; set; } = default!;

    private EntityTable<MemberDto, Guid, MemberViewModel> _table = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Member",
            entityNamePlural: "Members",
            entityResource: FSHResource.CAD,
            fields: new()
            {
                new(data => data.MembershipDate.ToString("MMM dd, yyyy"), "MembershipDate", "MembershipDate"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Address, "Address", "Address"),
                new(data => data.Gender, "Gender", "Gender"),
                new(data => data.Phone, "Phone", "Phone"),
                new(data => data.BirthDate.ToString("MMM dd, yyyy"), "BirthDate", "BirthDate"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<MemberSearchRequest>()))
                .Adapt<PaginationResponse<MemberDto>>(),
            createFunc: async data =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Name}_{Guid.NewGuid():N}" };
                }

                await Client.CreateAsync(data.Adapt<MemberCreateRequest>());
                data.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, Member) =>
            {
                if (!string.IsNullOrEmpty(Member.ImageInBytes))
                {
                    Member.DeleteCurrentImage = true;
                    Member.Image = new FileUploadRequest() { Data = Member.ImageInBytes, Extension = Member.ImageExtension ?? string.Empty, Name = $"{Member.Name}_{Guid.NewGuid():N}" };
                }

                await Client.UpdateAsync(id, Member.Adapt<MemberUpdateRequest>());
                Member.ImageInBytes = string.Empty;
            },
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

public class MemberViewModel : MemberUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}