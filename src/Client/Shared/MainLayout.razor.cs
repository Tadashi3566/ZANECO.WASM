using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Security.Cryptography.Xml;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Infrastructure.Preferences;

namespace ZANECO.WASM.Client.Shared;
public partial class MainLayout
{
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;
    [Parameter]
    public EventCallback OnDarkModeToggle { get; set; }
    [Parameter]
    public EventCallback<bool> OnRightToLeftToggle { get; set; }
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    private bool _drawerOpen;
    private bool _rightToLeft;

    private ClientPreference _preference = new();

    private DotNetObjectReference<MainLayout>? _reference;
    protected DotNetObjectReference<MainLayout> Reference
    {
        get
        {
            if (_reference == null)
            {
                _reference = DotNetObjectReference.Create(this);
            }

            return _reference;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (await ClientPreferences.GetPreference() is ClientPreference preference)
        {
            _rightToLeft = preference.IsRTL;
            _drawerOpen = preference.IsDrawerOpen;
        }

        //await _jsRuntime.InvokeAsync<object>("setRef", Reference);

        //OnInstallable = async () =>
        //{
        //    var parameters = new DialogParameters();
        //    var options = new DialogOptions() { CloseButton = false, NoHeader = true, MaxWidth = MaxWidth.Large, Position = DialogPosition.BottomCenter };
        //    var dialog = DialogService.Show<InstallApp>("", parameters, options);
        //    var result = await dialog.Result;
        //    if (!result.Canceled)
        //    {
        //        await _jsRuntime.InvokeVoidAsync("BlazorPWA.installPWA");
        //    }
        //};

        //await LoadDataAsync();
    }

    public async Task ToggleDarkMode()
    {
        await OnDarkModeToggle.InvokeAsync();
    }

    private async Task DrawerToggle()
    {
        _drawerOpen = await ClientPreferences.ToggleDrawerAsync();
    }

    private void Logout()
    {
        var parameters = new DialogParameters
            {
                { nameof(Dialogs.Logout.ContentText), $"{L["Logout Confirmation"]}"},
                { nameof(Dialogs.Logout.ButtonText), $"{L["Logout"]}"},
                { nameof(Dialogs.Logout.Color), Color.Error}
            };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        DialogService.Show<Dialogs.Logout>(L["Logout"], parameters, options);
    }

    private void Profile()
    {
        Navigation.NavigateTo("/account");
    }
}