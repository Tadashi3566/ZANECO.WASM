using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Infrastructure.Notifications;
using ZANECO.WASM.Client.Infrastructure.Preferences;

namespace ZANECO.WASM.Client.Components.ThemeManager;

public partial class BackgroundPanel
{
    [Parameter]
    public bool BackgroundJob { get; set; }
    [Parameter]
    public bool Scheduled { get; set; }
    [Parameter]
    public int Minutes { get; set; } = 1;
    [Inject]
    protected INotificationPublisher Notifications { get; set; } = default!;

    private BackgroundPreference _backgroundPreference = new();

    protected override async Task OnInitializedAsync()
    {
        if (await ClientPreferences.GetPreference() is ClientPreference clientPreference)
        {
            _backgroundPreference = clientPreference.BackgroundPreference;
        }

        BackgroundJob = _backgroundPreference.IsBackgroundJob;
        Scheduled = _backgroundPreference.IsScheduled;
        Minutes = _backgroundPreference.InMinutes;
    }

    [Parameter]
    public EventCallback<bool> OnBackgroundJobSwitchToggled { get; set; }
    [Parameter]
    public EventCallback<bool> OnScheduledSwitchToggled { get; set; }
    [Parameter]
    public EventCallback<int> OnMinuteValueChanged { get; set; }

    private async Task ToggleBackgroundJobSwitch()
    {
        _backgroundPreference.IsBackgroundJob = !_backgroundPreference.IsBackgroundJob;
        await OnBackgroundJobSwitchToggled.InvokeAsync(_backgroundPreference.IsBackgroundJob);
        await Notifications.PublishAsync(_backgroundPreference);
    }

    private async Task ToggleScheduledSwitch()
    {
        _backgroundPreference.IsScheduled = !_backgroundPreference.IsScheduled;
        await OnScheduledSwitchToggled.InvokeAsync(_backgroundPreference.IsScheduled);
        await Notifications.PublishAsync(_backgroundPreference);
    }

    private async Task ChangedMinuteValue(ChangeEventArgs args)
    {
        Minutes = int.Parse(args?.Value?.ToString() ?? "1");
        _backgroundPreference.InMinutes = Minutes;
        await OnMinuteValueChanged.InvokeAsync(Minutes);
        await Notifications.PublishAsync(_backgroundPreference);
    }
}