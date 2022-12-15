using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.AGMA.Winners;
public partial class Winners
{
    [Inject]
    protected IWinnersClient Client { get; set; } = default!;

    protected EntityServerTableContext<WinnerDto, Guid, WinnerViewModel> Context { get; set; } = default!;

    private EntityTable<WinnerDto, Guid, WinnerViewModel> _table = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Winner",
            entityNamePlural: "Winners",
            entityResource: FSHResource.Raffles,
            fields: new()
            {
                new(data => data.RaffleName, "Raffle", "RaffleName"),
                new(data => data.PrizeName, "Prize", "PrizeName"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Address, "Address", "Address"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: true,
            idFunc: Winner => Winner.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<WinnerSearchRequest>()))
                .Adapt<PaginationResponse<WinnerDto>>(),
            createFunc: async Winner => await Client.CreateAsync(Winner.Adapt<WinnerCreateRequest>()),
            updateFunc: async (id, Winner) => await Client.UpdateAsync(id, Winner),
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

public class WinnerViewModel : WinnerUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}