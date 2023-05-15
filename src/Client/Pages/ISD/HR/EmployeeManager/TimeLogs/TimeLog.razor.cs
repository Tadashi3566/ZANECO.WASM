using Blazored.LocalStorage;
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

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.TimeLogs;

public partial class TimeLog
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected IAuthenticationService AuthService { get; set; } = default!;

    [Inject]
    protected ITimeLogsClient Client { get; set; } = default!;

    [Inject]
    protected ILocalStorageService LocalStorage { get; set; } = default!;

    private readonly TimeLogCreateRequest _timeLog = new();

    private string? _employeeId;

    private CustomValidation? _customValidation;

    private ClientPreference _preference = new();

    protected override async Task OnInitializedAsync()
    {
        _employeeId = await LocalStorage.GetItemAsStringAsync("employeeId");

        _timeLog.LogDate = DateTime.Today;
    }

    private async Task CreateTimeLogAsync()
    {
        await ApiHelper.ExecuteCallGuardedAsync(() => Client.CreateAsync(_timeLog), Snackbar, _customValidation);

        Snackbar.Add("You have successfully created a Time Log.", Severity.Success);
    }

    private async Task UploadImage(InputFileChangeEventArgs e)
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

            string? fileName = $"{_employeeId}_{DateTime.Today:yyyy_MM_dd}_{_timeLog.LogType}_{Guid.NewGuid():N}";
            fileName = fileName[..Math.Min(fileName.Length, 90)];
            var imageFile = await file.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
            byte[]? buffer = new byte[imageFile.Size];
            await imageFile.OpenReadStream(ApplicationConstants.MaxAllowedSize).ReadAsync(buffer);
            string? base64String = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            _timeLog.Image = new ImageUploadRequest() { Name = fileName, Data = base64String, Extension = extension };

            await CreateTimeLogAsync();
        }
    }
}