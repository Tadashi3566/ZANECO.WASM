using MediatR.Courier;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Notifications;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Notifications;

namespace ZANECO.WASM.Client.Pages.Personal;
public partial class Dashboard
{
    [Parameter]
    public double RegisteredCount { get; set; }
    [Parameter]
    public double SmsCount { get; set; }
    [Parameter]
    public double WebCount { get; set; }

    [Parameter]
    public int UserCount { get; set; }
    [Parameter]
    public int RoleCount { get; set; }

    [Inject]
    private IDashboardClient DashboardClient { get; set; } = default!;
    [Inject]
    private ICourier Courier { get; set; } = default!;

    private readonly string[] _regTypeLabels = { "SMS", "WEB" };
    private readonly List<double> _regTypeData = new();

    private readonly string[] _dataEnterBarChartXAxisLabels = { "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D10" };
    private readonly List<MudBlazor.ChartSeries> _dataEnterBarChartSeries = new();

    private ChartOptions _chartOptions = new();

    private bool _loaded;

    private ClientPreference _preference = new();

    protected override async Task OnInitializedAsync()
    {
        Courier.SubscribeWeak<NotificationWrapper<StatsChangedNotification>>(async _ =>
        {
            await LoadDataAsync();
            StateHasChanged();
        });

        await LoadDataAsync();

        _chartOptions.YAxisTicks = 100;
        _chartOptions.LineStrokeWidth = 1;

        _loaded = true;
    }

    private async Task LoadDataAsync()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => DashboardClient.GetAsync(),
                Snackbar)
            is StatsDto statsDto)
        {
            RegisteredCount = statsDto.RegisteredCount;
            SmsCount = statsDto.SmsCount;
            WebCount = statsDto.WebCount;

            _regTypeData.Clear();
            _regTypeData.Add(statsDto.SmsCount);
            _regTypeData.Add(statsDto.WebCount);

            UserCount = statsDto.UserCount;
            RoleCount = statsDto.RoleCount;

            foreach (var item in statsDto.DataEnterBarChart)
            {
                _dataEnterBarChartSeries
                    .RemoveAll(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));

                _dataEnterBarChartSeries.Add(new MudBlazor.ChartSeries { Name = item.Name, Data = item.Data?.ToArray() });
            }
        }
    }
}