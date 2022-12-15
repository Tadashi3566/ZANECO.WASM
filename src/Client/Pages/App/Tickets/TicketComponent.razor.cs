using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.App.Tickets;

public partial class TicketComponent
{
    [Parameter]
    public Guid Id { get; set; }
    [Inject]
    protected ITicketsClient TicketClient { get; set; } = default!;

    private readonly TicketUpdateRequest _ticket = new();

    private TicketDetailsDto _ticketDto = new();

    private string? _imageUrl;

    private CustomValidation? _customValidation;

    private ClientPreference _preference = new();

    //protected override void OnParametersSet()
    //{
    //    if (Id != Guid.Empty)
    //    {
    //        _context.Id = Id;
    //    }
    //}

    protected override async Task OnInitializedAsync()
    {
        if (Id != Guid.Empty)
        {
            _ticketDto = await TicketClient.GetAsync(Id);

            _ticket.Id = Id;
            _ticket.Impact = _ticketDto.Impact;
            _ticket.Urgency = _ticketDto.Urgency;
            _ticket.Priority = _ticketDto.Priority;
            _ticket.Name = _ticketDto.Name;
            _ticket.Description = _ticketDto.Description;
            _ticket.Notes = _ticketDto.Notes;
            _ticket.RequestedBy = _ticketDto.RequestedBy;
            _ticket.RequestThrough = _ticketDto.RequestThrough;
            _ticket.Reference = _ticketDto.Reference;
            _ticket.Status = _ticketDto.Status;
        }
    }

    private async Task OnSubmit() => await ApiHelper.ExecuteCallGuardedAsync((Func<Task<Guid>>)(() =>
        TicketClient.UpdateAsync(this.Id, _ticket)), Snackbar, _customValidation,
        "The Ticket has been successfully updated.");

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file is not null)
        {
            string? extension = Path.GetExtension(file.Name);
            if (!ApplicationConstants.SupportedImageFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("Image Format Not Supported.", Severity.Error);
                return;
            }

            string? fileName = $"{Id}-{Guid.NewGuid():N}";
            fileName = fileName[..Math.Min(fileName.Length, 90)];
            var imageFile = await file.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
            byte[]? buffer = new byte[imageFile.Size];
            await imageFile.OpenReadStream(ApplicationConstants.MaxAllowedSize).ReadAsync(buffer);
            string? base64String = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            _ticket.Image = new FileUploadRequest() { Name = fileName, Data = base64String, Extension = extension };

            await OnSubmit();
        }
    }

    private async Task RemoveImageAsync()
    {
        string deleteContent = "You're sure you want to delete your Ticket Image?";
        var parameters = new DialogParameters
        {
            { nameof(DeleteConfirmation.ContentText), deleteContent }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<DeleteConfirmation>("Delete", parameters, options);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            _ticket.DeleteCurrentImage = true;
            await OnSubmit();
        }
    }
}