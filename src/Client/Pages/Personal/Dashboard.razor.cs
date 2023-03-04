using MediatR.Courier;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Notifications;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;
using ZANECO.WebApi.Shared.Notifications;

namespace ZANECO.WASM.Client.Pages.Personal;

public partial class Dashboard
{
    [Parameter]
    public int UserCount { get; set; }
    [Parameter]
    public int RoleCount { get; set; }

    [Parameter]
    public int ContactCount { get; set; }
    [Parameter]
    public int SMSLogCount { get; set; }
    [Parameter]
    public int SMSTemplateCount { get; set; }

    [Parameter]
    public int MemberCount { get; set; }
    [Parameter]
    public int AccountCount { get; set; }

    [Parameter]
    public int BrandCount { get; set; }
    [Parameter]
    public int ProductCount { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    [Inject]
    private IDashboardClient DashboardClient { get; set; } = default!;
    [Inject]
    private ICourier Courier { get; set; } = default!;

    private readonly string[] _dataBarChartXAxisLabelsMonth = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
    private readonly List<MudBlazor.ChartSeries> _dataBarChartSeriesSandurot = new();
    private readonly List<MudBlazor.ChartSeries> _dataBarChartSeriesSMSPerMonth = new();
    private readonly List<MudBlazor.ChartSeries> _dataBarChartSeriesSMSPerDay = new();
    private string[]? _daysOfMonth;

    private ChartOptions _chartOptionMonths = new();
    private ChartOptions _chartOptionDays = new();

    private bool _loaded;

    private ClientPreference _preference = new();

    private bool _isAdmin;
    private bool _isISD;
    private bool _isCAD;
    private bool _isSandurot;
    private bool _isContact;
    private bool _isSMS;

    private bool _isEmployee;

    protected override async Task OnParametersSetAsync()
    {
        var state = await AuthState;
        _isAdmin = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.Users);
        _isISD = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.ISD);
        _isCAD = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.CAD);
        _isSandurot = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.Sandurot);
        _isContact = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.Contacts);
        _isSMS = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.SMS);

        _isEmployee = await AuthService.HasPermissionAsync(state.User, FSHAction.Create, FSHResource.Attendance);
    }

    protected override async Task OnInitializedAsync()
    {
        _daysOfMonth = new string[DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)]; // Create a string array to store the day of the month for each date

        for (int day = 1; day <= DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month); day++) // Loop through the days of the month
        {
            _daysOfMonth[day - 1] = day.ToString(); // Add the day of the month to the array as a string
        }

        Courier.SubscribeWeak<NotificationWrapper<StatsChangedNotification>>(async _ =>
        {
            await LoadDataAsync();
            StateHasChanged();
        });

        await LoadDataAsync();

        _loaded = true;
    }

    private async Task LoadDataAsync()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(
            () => DashboardClient.GetAsync(), Snackbar) is StatsDto statsDto)
        {
            UserCount = statsDto.UserCount;
            RoleCount = statsDto.RoleCount;

            ContactCount = statsDto.ContactCount;

            MemberCount = statsDto.MemberCount;
            AccountCount = statsDto.AccountCount;

            BrandCount = statsDto.BrandCount;
            ProductCount = statsDto.ProductCount;

            _chartOptionMonths.YAxisTicks = 5000;
            _chartOptionDays.YAxisTicks = 2000;

            foreach (var item in statsDto.BarChartSandurot)
            {
                _dataBarChartSeriesSandurot.RemoveAll(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                _dataBarChartSeriesSandurot.Add(new MudBlazor.ChartSeries { Name = item.Name, Data = item.Data?.ToArray() });
            }

            foreach (var item in statsDto.BarChartSMSPerMonth)
            {
                _dataBarChartSeriesSMSPerMonth.RemoveAll(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                _dataBarChartSeriesSMSPerMonth.Add(new MudBlazor.ChartSeries { Name = item.Name, Data = item.Data?.ToArray() });
            }

            foreach (var item in statsDto.BarChartSMSPerDay)
            {
                _dataBarChartSeriesSMSPerDay.RemoveAll(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                _dataBarChartSeriesSMSPerDay.Add(new MudBlazor.ChartSeries { Name = item.Name, Data = item.Data?.ToArray() });
            }
        }
    }
}