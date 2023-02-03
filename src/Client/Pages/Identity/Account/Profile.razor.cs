using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.Identity.Account;
public partial class Profile
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthenticationService AuthService { get; set; } = default!;
    [Inject]
    protected IPersonalClient PersonalClient { get; set; } = default!;

    private readonly UpdateUserRequest _user = new();

    private string? _imageUrl;
    private string? _userId;
    private char _firstLetterOfName;

    private CustomValidation? _customValidation;

    private ClientPreference _preference = new();

    protected override async Task OnInitializedAsync()
    {
        if ((await AuthState).User is { } user)
        {
            _userId = user.GetUserId();
            _user.Email = user.GetEmail() ?? string.Empty;
            _user.FirstName = user.GetFirstName() ?? string.Empty;
            _user.LastName = user.GetSurname() ?? string.Empty;
            _user.PhoneNumber = user.GetPhoneNumber();
            _imageUrl = string.IsNullOrEmpty(user?.GetImageUrl()) ? string.Empty : (Config[ConfigNames.ApiBaseUrl] + user?.GetImageUrl());
            if (_userId is not null) _user.Id = _userId;
        }

        if (_user.FirstName?.Length > 0)
        {
            _firstLetterOfName = _user.FirstName.ToUpper().FirstOrDefault();
        }
    }

    private async Task UpdateProfileAsync()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(() => PersonalClient.UpdateProfileAsync(_user),
            Snackbar,
            _customValidation))
        {
            Snackbar.Add(L["Your Profile has been updated. Please Login again to Continue."], Severity.Success);
            await AuthService.ReLoginAsync(Navigation.Uri);
        }
    }

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

            string? fileName = $"{_userId}-{Guid.NewGuid():N}";
            fileName = fileName[..Math.Min(fileName.Length, 90)];
            var imageFile = await file.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
            byte[]? buffer = new byte[imageFile.Size];
            await imageFile.OpenReadStream(ApplicationConstants.MaxAllowedSize).ReadAsync(buffer);
            string? base64String = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            _user.Image = new FileUploadRequest() { Name = fileName, Data = base64String, Extension = extension };

            await UpdateProfileAsync();
        }
    }

    private async Task RemoveImageAsync()
    {
        string deleteContent = L["You're sure you want to delete your Profile Image?"];
        var parameters = new DialogParameters
        {
            { nameof(DeleteConfirmation.ContentText), deleteContent }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<DeleteConfirmation>(L["Delete"], parameters, options);
        var result = await dialog.Result;
        if (!result.Cancelled)
        {
            _user.DeleteCurrentImage = true;
            await UpdateProfileAsync();
        }
    }
}