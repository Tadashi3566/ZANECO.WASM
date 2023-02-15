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

    private EntityTable<TicketDto, Guid, TicketViewModel>? _table;

    private bool _canViewTickets;

    private string? _userId;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewTickets = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Tickets);

        Context = new(
            entityName: "Ticket",
            entityNamePlural: "Tickets",
            entityResource: FSHResource.Tickets,
            fields: new()
            {
                new(dto => dto.GroupCode, "Group", "Group"),
                new(dto => dto.Impact, "Impact/Urgency/Priority", "Impact", Template: TemplateScale),
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

    private async Task<bool> SetProgress(string action, Guid ticketId)
    {
        var parameters = new DialogParameters
        {
            { nameof(TicketProgressDialog.TicketId), ticketId },
            { nameof(TicketProgressDialog.Action), action },
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<TicketProgressDialog>($"{action} Ticket", parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            if ((await AuthState).User is { } user)
            {
                _userId = user.GetUserId();
            }

            switch (action)
            {
                case "Open":
                    //await ApiHelper.ExecuteCallGuardedAsync(() => TicketsClient(_rating, _customValidation,
                    //    "Message successfully sent to the recepient(s).");        
                    break;

                case "Suspend":

                    break;

                case "Close":

                    break;

                case "Approve":

                    break;

                default: break;
            }
        }

        return false;
    }
}

public class TicketViewModel : TicketUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}