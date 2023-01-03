using MediatR.Courier;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.Notifications;
using ZANECO.WASM.Client.Infrastructure.Preferences;

namespace ZANECO.WASM.Client.Components.Common;

public class FshTable<T> : MudTable<T>
{
    [Inject]
    protected ICourier Courier { get; set; } = default!;
    [Inject]
    private IClientPreferenceManager ClientPreferences { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (await ClientPreferences.GetPreference() is ClientPreference clientPreference)
        {
            SetTablePreference(clientPreference.TablePreference);
        }

        Courier.SubscribeWeak<NotificationWrapper<FshTablePreference>>(wrapper =>
        {
            SetTablePreference(wrapper.Notification);
            StateHasChanged();
        });

        await base.OnInitializedAsync();
    }

    private void SetTablePreference(FshTablePreference tablePreference)
    {
        FixedHeader = tablePreference.IsFixedHeaderFooter;
        FixedFooter = tablePreference.IsFixedHeaderFooter;
        AllowUnsorted = tablePreference.IsAllowUnsorted;

        Dense = tablePreference.IsDense;
        Striped = tablePreference.IsStriped;
        Bordered = tablePreference.HasBorder;
        Hover = tablePreference.IsHoverable;

        MultiSelection = tablePreference.IsMultiSelection;
        Virtualize = tablePreference.IsVirtualize;
    }
}