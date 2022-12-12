using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.AGMA.Prizes;
public partial class Prizes
{
    [Inject]
    protected IPrizesClient Client { get; set; } = default!;

    protected EntityServerTableContext<PrizeDto, Guid, PrizeViewModel> Context { get; set; } = default!;

    private EntityTable<PrizeDto, Guid, PrizeViewModel> _table = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Prize",
            entityNamePlural: "Prizes",
            entityResource: FSHResource.Prizes,
            fields: new()
            {
                new(data => data.Name, "Name", "Name"),
                new(data => data.PrizeDate.ToString("MMM dd, yyyy"), "Prize Date", "PrizeDate"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: true,
            idFunc: Prize => Prize.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<PrizeSearchRequest>()))
                .Adapt<PaginationResponse<PrizeDto>>(),
            createFunc: async Prize => await Client.CreateAsync(Prize.Adapt<PrizeCreateRequest>()),
            updateFunc: async (id, Prize) => await Client.UpdateAsync(id, Prize),
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

public class PrizeViewModel : PrizeUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}