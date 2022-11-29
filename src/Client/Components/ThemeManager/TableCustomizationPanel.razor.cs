using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Infrastructure.Notifications;
using ZANECO.WASM.Client.Infrastructure.Preferences;

namespace ZANECO.WASM.Client.Components.ThemeManager;
public partial class TableCustomizationPanel
{
    [Parameter]
    public bool IsFixedHeader { get; set; }
    [Parameter]
    public bool IsFixedFooter { get; set; }
    [Parameter]
    public bool IsDense { get; set; }
    [Parameter]
    public bool IsStriped { get; set; }
    [Parameter]
    public bool HasBorder { get; set; }
    [Parameter]
    public bool IsHoverable { get; set; }
    [Parameter]
    public bool IsMultiSelection { get; set; }
    [Inject]
    protected INotificationPublisher Notifications { get; set; } = default!;

    private FshTablePreference _tablePreference = new();

    protected override async Task OnInitializedAsync()
    {
        if (await ClientPreferences.GetPreference() is ClientPreference clientPreference)
        {
            _tablePreference = clientPreference.TablePreference;
        }

        IsFixedHeader = _tablePreference.IsFixedHeader;
        IsFixedFooter = _tablePreference.IsFixedFooter;

        IsDense = _tablePreference.IsDense;
        IsStriped = _tablePreference.IsStriped;
        HasBorder = _tablePreference.HasBorder;
        IsHoverable = _tablePreference.IsHoverable;

        IsMultiSelection = _tablePreference.IsMultiSelection;
    }

    [Parameter]
    public EventCallback<bool> OnFixedHeaderSwitchToggled { get; set; }

    [Parameter]
    public EventCallback<bool> OnFixedFooterSwitchToggled { get; set; }

    [Parameter]
    public EventCallback<bool> OnDenseSwitchToggled { get; set; }

    [Parameter]
    public EventCallback<bool> OnStripedSwitchToggled { get; set; }

    [Parameter]
    public EventCallback<bool> OnBorderdedSwitchToggled { get; set; }

    [Parameter]
    public EventCallback<bool> OnHoverableSwitchToggled { get; set; }

    [Parameter]
    public EventCallback<bool> OnMultipleSelectionSwitchToggled { get; set; }

    private async Task ToggleFixedHeaderSwitch()
    {
        _tablePreference.IsFixedHeader = !_tablePreference.IsFixedHeader;
        await OnFixedHeaderSwitchToggled.InvokeAsync(_tablePreference.IsFixedHeader);
        await Notifications.PublishAsync(_tablePreference);
    }

    private async Task ToggleFixedFooterSwitch()
    {
        _tablePreference.IsFixedFooter = !_tablePreference.IsFixedFooter;
        await OnFixedFooterSwitchToggled.InvokeAsync(_tablePreference.IsFixedFooter);
        await Notifications.PublishAsync(_tablePreference);
    }

    private async Task ToggleDenseSwitch()
    {
        _tablePreference.IsDense = !_tablePreference.IsDense;
        await OnDenseSwitchToggled.InvokeAsync(_tablePreference.IsDense);
        await Notifications.PublishAsync(_tablePreference);
    }

    private async Task ToggleStripedSwitch()
    {
        _tablePreference.IsStriped = !_tablePreference.IsStriped;
        await OnStripedSwitchToggled.InvokeAsync(_tablePreference.IsStriped);
        await Notifications.PublishAsync(_tablePreference);
    }

    private async Task ToggleBorderedSwitch()
    {
        _tablePreference.HasBorder = !_tablePreference.HasBorder;
        await OnBorderdedSwitchToggled.InvokeAsync(_tablePreference.HasBorder);
        await Notifications.PublishAsync(_tablePreference);
    }

    private async Task ToggleHoverableSwitch()
    {
        _tablePreference.IsHoverable = !_tablePreference.IsHoverable;
        await OnHoverableSwitchToggled.InvokeAsync(_tablePreference.IsHoverable);
        await Notifications.PublishAsync(_tablePreference);
    }

    private async Task ToggleMultipleSelectionSwitch()
    {
        _tablePreference.IsMultiSelection = !_tablePreference.IsMultiSelection;
        await OnMultipleSelectionSwitchToggled.InvokeAsync(_tablePreference.IsMultiSelection);
        await Notifications.PublishAsync(_tablePreference);
    }
}