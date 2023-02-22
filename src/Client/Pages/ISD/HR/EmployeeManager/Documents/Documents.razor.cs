using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Documents;
public partial class Documents
{
    [Inject]
    protected IDocumentsClient Client { get; set; } = default!;

    protected EntityServerTableContext<DocumentDto, Guid, DocumentViewModel> Context { get; set; } = default!;

    private EntityTable<DocumentDto, Guid, DocumentViewModel>? _table;

    private string? _searchString;

    private IBrowserFile? _file;

    private string? _fileName;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "data",
            entityNamePlural: "Documents",
            entityResource: FSHResource.CAD,
            fields: new()
            {
                new(data => data.Reference, "Reference", "Reference", Template: TemplateDateReference),
                new(data => data.Name, "Name", "Name"),
                new(data => data.FileName, "FileName", "FileName", Template: TemplateFileNameContent),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", "Notes", visible: false),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<DocumentSearchRequest>()))
                .Adapt<PaginationResponse<DocumentDto>>(),
            createFunc: async data =>
            {
                if (data.FileInBytes is not null)
                {
                    data.FileName = _fileName!;
                    data.File = new FileUploadRequest() { Data = data.FileInBytes, Extension = data.FileExtension ?? string.Empty, Name = $"{Guid.NewGuid():N}_{data.FileName}" };
                }

                await Client.CreateAsync(data.Adapt<DocumentCreateRequest>());
                data.FileInBytes = null;
            },
            updateFunc: async (id, data) =>
            {
                if (data.FileInBytes is not null)
                {
                    data.DeleteCurrentFile = true;
                    data.FileName = _fileName!;
                    data.File = new FileUploadRequest() { Data = data.FileInBytes, Extension = data.FileExtension ?? string.Empty, Name = $"{Guid.NewGuid():N}_{data.FileName}" };
                }

                await Client.UpdateAsync(id, data.Adapt<DocumentUpdateRequest>());
                data.FileInBytes = null;
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);

    // TODO : Make this as a shared service or something? Since it's used by Profile Component also for now, and literally any other component that will have File upload.
    // The new service should ideally return $"data:{ApplicationConstants.StandardFileFormat};base64,{Convert.ToBase64String(buffer)}"
    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        _file = e.File;
        _fileName = e.File.Name;

        if (_file != null)
        {
            byte[] buffer = new byte[_file.Size];
            string? extension = Path.GetExtension(e.File.Name);
            if (!ApplicationConstants.SupportedDocumentFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("File Format Not Supported.", Severity.Error);
                return;
            }
            string format = "application/octet-stream";
            await _file.OpenReadStream(_file.Size).ReadAsync(buffer);
            Context.AddEditModal.RequestModel.FileExtension = extension;
            //Context.AddEditModal.RequestModel.FilePath = $"data:{format};base64,{Convert.ToBase64String(buffer)}";
            Context.AddEditModal.RequestModel.FileInBytes = buffer;
            Context.AddEditModal.ForceRender();

            //var FileFile = await e.File.RequestImageFileAsync(ApplicationConstants.StandardDocumentFormat, ApplicationConstants.MaxFileWidth, ApplicationConstants.MaxFileHeight);
            //byte[]? buffer = new byte[FileFile.Size];
            //await FileFile.OpenReadStream(ApplicationConstants.MaxAllowedSize).ReadAsync(buffer);
            //Context.AddEditModal.RequestModel.FileInBytes = $"data:{ApplicationConstants.StandardFileFormat};base64,{Convert.ToBase64String(buffer)}";
            //Context.AddEditModal.ForceRender();
        }
    }

    //private async Task UploadFiles(InputFileChangeEventArgs e)
    //{
    //    _file = e.File;
    //    if (_file != null)
    //    {
    //        var buffer = new byte[_file.Size];
    //        var extension = Path.GetExtension(_file.Name);
    //        var format = "application/octet-stream";
    //        await _file.OpenReadStream(_file.Size).ReadAsync(buffer);
    //        AddEditDocumentModel.URL = $"data:{format};base64,{Convert.ToBase64String(buffer)}";
    //        AddEditDocumentModel.UploadRequest = new UploadRequest { Data = buffer, UploadType = Application.Enums.UploadType.Documents, Extension = extension };
    //    }
    //}

    private void ClearFileInBytes()
    {
        Context.AddEditModal.RequestModel.FileInBytes = null;
        Context.AddEditModal.ForceRender();
    }

    private void SetDeleteCurrentFileFlag()
    {
        Context.AddEditModal.RequestModel.FileInBytes = null;
        Context.AddEditModal.RequestModel.FilePath = string.Empty;
        Context.AddEditModal.RequestModel.DeleteCurrentFile = true;
        Context.AddEditModal.ForceRender();
    }
}

public class DocumentViewModel : DocumentUpdateRequest
{
    public string? FilePath { get; set; }
    public byte[]? FileInBytes { get; set; }
    public string? FileExtension { get; set; }
}