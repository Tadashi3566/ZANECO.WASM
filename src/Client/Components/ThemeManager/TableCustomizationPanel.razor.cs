using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Infrastructure.Notifications;
using ZANECO.WASM.Client.Infrastructure.Preferences;

namespace ZANECO.WASM.Client.Components.ThemeManager;
public partial class TableCustomizationPanel
{
    [Parameter]
    public bool IsFixedHeaderFooter { get; set; }
    [Parameter]
    public bool IsAllowUnsorted { get; set; }
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
    [Parameter]
    public bool IsVirtualize { get; set; }
    [Inject]
    protected INotificationPublisher Notifications { get; set; } = default!;

    private FshTablePreference _tablePreference = new();

    protected override async Task OnInitializedAsync()
    {
        if (await ClientPreferences.GetPreference() is ClientPreference clientPreference)
        {
            _tablePreference = clientPreference.TablePreference;
        }

        IsFixedHeaderFooter = _tablePreference.IsFixedHeaderFooter;
        IsAllowUnsorted = _tablePreference.IsAllowUnsorted;

        IsDense = _tablePreference.IsDense;
        IsStriped = _tablePreference.IsStriped;
        HasBorder = _tablePreference.HasBorder;
        IsHoverable = _tablePreference.IsHoverable;

        IsMultiSelection = _tablePreference.IsMultiSelection;
        IsVirtualize = _tablePreference.IsVirtualize;
    }

    [Parameter]
    public EventCallback<bool> OnFixedHeaderFooterSwitchToggled { get; set; }
    [Parameter]
    public EventCallback<bool> OnAllowUnsortedSwitchToggled { get; set; }
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
    [Parameter]
    public EventCallback<bool> OnVirtualizeSwitchToggled { get; set; }

    private async Task ToggleFixedHeaderFooterSwitch()
    {
        _tablePreference.IsFixedHeaderFooter = !_tablePreference.IsFixedHeaderFooter;
        await OnFixedHeaderFooterSwitchToggled.InvokeAsync(_tablePreference.IsFixedHeaderFooter);
        await Notifications.PublishAsync(_tablePreference);
    }

    private async Task ToggleAllowUnsortedSwitch()
    {
        _tablePreference.IsAllowUnsorted = !_tablePreference.IsAllowUnsorted;
        await OnAllowUnsortedSwitchToggled.InvokeAsync(_tablePreference.IsAllowUnsorted);
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

    private async Task ToggleVirtualizeSwitch()
    {
        _tablePreference.IsVirtualize = !_tablePreference.IsVirtualize;
        await OnVirtualizeSwitchToggled.InvokeAsync(_tablePreference.IsVirtualize);
        await Notifications.PublishAsync(_tablePreference);
    }
}