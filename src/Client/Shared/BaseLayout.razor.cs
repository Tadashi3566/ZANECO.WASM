using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Infrastructure.Theme;

namespace ZANECO.WASM.Client.Shared;

public partial class BaseLayout
{
    private ClientPreference? _themePreference;
    private MudTheme _currentTheme = new LightTheme();
    private bool _themeDrawerOpen;
    private bool _rightToLeft;

    private ClientPreference _preference = new();

    protected override async Task OnInitializedAsync()
    {
        _themePreference = await ClientPreferences.GetPreference() as ClientPreference;
        if (_themePreference == null) _themePreference = new ClientPreference();
        SetCurrentTheme(_themePreference);

        //This function will be intended for tickets or comments
        Snackbar.Add("Want to know our Company Information and Services? ", Severity.Normal, config =>
        {
            config.Action = "Come and visit our Official Website!";
            config.ActionColor = Color.Primary;
            config.CloseAfterNavigation = true;
            config.BackgroundBlurred = true;
            config.Icon = Icons.Custom.Brands.Chrome;
            config.Onclick = snackbar =>
            {
                Navigation.NavigateTo("https://www.zaneco.ph");
                return Task.CompletedTask;
            };
        });

        Snackbar.Add("How about sending us your comments, feedbacks and suggestions? ", Severity.Normal, config =>
        {
            config.Action = "Write it down here. Let's Go!";
            config.ActionColor = Color.Secondary;
            config.BackgroundBlurred = true;
            config.CloseAfterNavigation = true;
            config.Icon = Icons.Material.Filled.Comment;
            config.ShowTransitionDuration = 60;
            config.Onclick = snackbar =>
            {
                Navigation.NavigateTo("/survey/comment");
                return Task.CompletedTask;
            };
        });
    }

    private async Task ThemePreferenceChanged(ClientPreference themePreference)
    {
        SetCurrentTheme(themePreference);
        await ClientPreferences.SetPreference(themePreference);
    }

    private void SetCurrentTheme(ClientPreference themePreference)
    {
        _currentTheme = themePreference.IsDarkMode ? new DarkTheme() : new LightTheme();
        _currentTheme.Palette.Primary = themePreference.PrimaryColor;
        _currentTheme.Palette.Secondary = themePreference.SecondaryColor;
        _currentTheme.LayoutProperties.DefaultBorderRadius = $"{themePreference.BorderRadius}px";
        _currentTheme.LayoutProperties.DefaultBorderRadius = $"{themePreference.BorderRadius}px";
        _rightToLeft = themePreference.IsRTL;
    }
}