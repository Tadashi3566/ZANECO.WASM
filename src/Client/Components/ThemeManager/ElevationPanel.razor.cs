using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Infrastructure.Preferences;

namespace ZANECO.WASM.Client.Components.ThemeManager;

public partial class ElevationPanel
{
    [Parameter]
    public double Elevation { get; set; }
    [Parameter]
    public double MaxValue { get; set; } = 25;
    [Parameter]
    public EventCallback<double> OnSliderChanged { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (await ClientPreferences.GetPreference() is not ClientPreference themePreference) themePreference = new ClientPreference();
        Elevation = themePreference.Elevation;
    }

    private async Task ChangedSelection(ChangeEventArgs args)
    {
        Elevation = int.Parse(args?.Value?.ToString() ?? "0");
        await OnSliderChanged.InvokeAsync(Elevation);
    }
}