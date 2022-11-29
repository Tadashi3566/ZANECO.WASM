using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Infrastructure.Theme;

namespace ZANECO.WASM.Client.Components.ThemeManager;

public partial class ThemeDrawer
{
    [Parameter]
    public bool ThemeDrawerOpen { get; set; }

    [Parameter]
    public EventCallback<bool> ThemeDrawerOpenChanged { get; set; }

    [EditorRequired]
    [Parameter]
    public ClientPreference ThemePreference { get; set; } = default!;

    [EditorRequired]
    [Parameter]
    public EventCallback<ClientPreference> ThemePreferenceChanged { get; set; }

    private ClientPreference _preference = new();

    private readonly List<string> _colors = CustomColors.ThemeColors;

    private async Task UpdateThemePrimaryColor(string color)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.PrimaryColor = color;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task UpdateThemeSecondaryColor(string color)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.SecondaryColor = color;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task UpdateBorderRadius(double radius)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.BorderRadius = radius;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task UpdateElevation(double elevation)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.Elevation = Convert.ToInt32(elevation);
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleDarkLightMode(bool isDarkMode)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.IsDarkMode = isDarkMode;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleEntityTableFixedHeader(bool isFixedHeader)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.TablePreference.IsFixedHeader = isFixedHeader;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleEntityTableFixedFooter(bool isFixedFooter)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.TablePreference.IsFixedFooter = isFixedFooter;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleEntityTableDense(bool isDense)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.TablePreference.IsDense = isDense;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleEntityTableStriped(bool isStriped)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.TablePreference.IsStriped = isStriped;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleEntityTableBorder(bool hasBorder)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.TablePreference.HasBorder = hasBorder;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleEntityTableHoverable(bool isHoverable)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.TablePreference.IsHoverable = isHoverable;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleEntityTableMultipleSelection(bool isMultipleSelection)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.TablePreference.IsMultiSelection = isMultipleSelection;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }
}