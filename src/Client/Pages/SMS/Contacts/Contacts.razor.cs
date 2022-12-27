using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Components.Services;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS.Contacts;

public partial class Contacts
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IContactsClient Client { get; set; } = default!;
    [Inject]
    protected IMessageOutsClient MessageOutClient { get; set; } = default!;
    protected EntityServerTableContext<ContactDto, Guid, ContactViewModel> Context { get; set; } = default!;

    private EntityTable<ContactDto, Guid, ContactViewModel> _table = default!;

    private string? _searchString;

    private HashSet<ContactDto> _selectedItems = new();

    private int[] _pageSizes = new int[] { 10, 15, 50, 100, 500, 1000 };

    private bool _canCreateSMS;

    [Inject]
    private IClipboardService? ClipboardService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canCreateSMS = await AuthService.HasPermissionAsync(state.User, FSHAction.Create, FSHResource.SMS);

        Context = new(
            entityName: "Contact",
            entityNamePlural: "Contacts",
            entityResource: FSHResource.Contacts,
            fields: new()
            {
                new(data => data.ContactType, "Contact Type", "ContactType"),
                new(data => data.PhoneNumber, "Phone Number", "PhoneNumber", Template: TemplatePhoneNumber),
                new(data => data.Name, "Name/Address", "Name", Template: TemplateNameAddress),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: true,
            hasExtraActionsFunc: () => _canCreateSMS,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<ContactSearchRequest>()))
                .Adapt<PaginationResponse<ContactDto>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<ContactCreateRequest>()),
            updateFunc: async (id, Contact) => await Client.UpdateAsync(id, Contact),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
    }

    private async void OnCopyPhoneNumbersChecked()
    {
        string[] phoneNumbers = _selectedItems.Select(x => x.PhoneNumber).ToArray()!;

        if (phoneNumbers.Length > 0)
        {
            await ClipboardService!.CopyToClipboard(string.Join(",", phoneNumbers));

            Snackbar.Add("Selected Phone Number(s) were copied to Clipboard", Severity.Success);
        }
    }

    private async Task Send(ContactDto dto)
    {
        string[] phoneNumbers; //= _selectedItems.Select(x => x.PhoneNumber).ToArray()!;
        string recepients; //= string.Join(",", phoneNumbers);

        if (_selectedItems.Count > 0)
        {
            phoneNumbers = _selectedItems.Select(x => x.PhoneNumber).ToArray()!;
            recepients = string.Join(",", phoneNumbers);

            //Navigation.NavigateTo($"/sms/send/{recepients}");
        }
        else
        {
            recepients = dto.PhoneNumber;
            //Navigation.NavigateTo($"/sms/send/{dto.PhoneNumber}");
        }

        DialogParameters parameters = new()
        {
            { nameof(SendMessageDialog.Recepients), recepients },
        };
        DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
        IDialogReference dialog = DialogService.Show<SendMessageDialog>("Send Message", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Cancelled)
        {

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

public class ContactViewModel : ContactUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}