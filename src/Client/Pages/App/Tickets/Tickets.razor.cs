using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.App.Tickets;
public partial class Tickets
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IGroupsClient GroupsClient { get; set; } = default!;
    [Inject]
    protected ITicketsClient TicketsClient { get; set; } = default!;

    protected EntityServerTableContext<TicketDto, Guid, TicketViewModel> Context { get; set; } = default!;

    private EntityTable<TicketDto, Guid, TicketViewModel> _table = default!;

    private bool _canViewRoleClaims;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewRoleClaims = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.RoleClaims);

        Context = new(
            entityName: "Ticket",
            entityNamePlural: "Tickets",
            entityResource: FSHResource.Tickets,
            fields: new()
            {
                new(dto => dto.GroupCode, "Group", "Group"),
                new(dto => dto.Impact, "Impact/Urgency/Priority", "Impact", Template: TemplateScale),
                //new(dto => dto.Urgency, "Urgency", "Urgency"),
                //new(dto => dto.Priority, "Priority", "Priority"),
                new(dto => dto.Name, "Name", "Name"),
                new(dto => dto.Description, "Description", "Description"),
                new(dto => dto.Notes, "Notes", "Notes"),
                new(dto => dto.RequestedBy, "Requested", "Requested"),
                new(dto => dto.RequestThrough, "Through", "Through"),
                new(dto => dto.Reference, "Reference", "Reference"),
                new(dto => dto.AssignedTo, "Assigned To", "Assigned To"),
                new(dto => dto.Status, "Status", "Status"),
            },
            enableAdvancedSearch: true,
            idFunc: data => data.Id,
            searchFunc: async filter => (await TicketsClient
                .SearchAsync(filter.Adapt<TicketSearchRequest>()))
                .Adapt<PaginationResponse<TicketDto>>(),
            createFunc: async data =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Name}_{Guid.NewGuid():N}" };
                }

                await TicketsClient.CreateAsync(data.Adapt<TicketCreateRequest>());
                data.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, data) =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.DeleteCurrentImage = true;
                    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Name}_{Guid.NewGuid():N}" };
                }

                await TicketsClient.UpdateAsync(id, data.Adapt<TicketUpdateRequest>());
                data.ImageInBytes = string.Empty;
            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<TicketExportRequest>();

                exportFilter.GroupId = SearchGroupId == default ? null : SearchGroupId;

                return await TicketsClient.ExportAsync(exportFilter);
            },
            deleteFunc: async id => await TicketsClient.DeleteAsync(id));
    }

    // Advanced Search

    private Guid _searchGroupId;
    private Guid SearchGroupId
    {
        get => _searchGroupId;
        set
        {
            _searchGroupId = value;
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

public class TicketViewModel : TicketUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}