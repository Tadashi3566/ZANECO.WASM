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
    private IMessageTemplatesClient MessageTemplatesClient { get; set; } = default!;

    [Inject]
    private ICourier Courier { get; set; } = default!;

    private List<MessageTemplateDto> _messageTemplates = new();

    private readonly string[] _dataBarChartXAxisLabelsMonth = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
    private string[]? _daysOfMonth;
    private readonly List<MudBlazor.ChartSeries> _dataBarChartSeriesSandurot = new();
    private readonly List<MudBlazor.ChartSeries> _dataBarChartSeriesSMSPerMonth = new();
    private readonly List<MudBlazor.ChartSeries> _dataBarChartSeriesSMSPerDay = new();

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

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;

        _isAdmin = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Users);
        _isISD = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.ISD);
        _isCAD = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.CAD);
        _isSandurot = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Sandurot);
        _isContact = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Contacts);
        _isSMS = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.SMS);

        _isEmployee = await AuthService.HasPermissionAsync(state.User, FSHAction.Create, FSHResource.Attendance);

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


        await LoadIncomingPowerInterruptions();
        await LoadDataAsync();

        _loaded = true;
    }

    private async Task LoadIncomingPowerInterruptions()
    {
        _messageTemplates.Clear();

        var filter = new MessageTemplateSearchRequest
        {
            IncomingSchedule = true,
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => MessageTemplatesClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfMessageTemplateDto response)
        {
            _messageTemplates = response.Data.OrderBy(x => x.Schedule).ToList();
        }
    }

    private async Task LoadDataAsync()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(() => DashboardClient.GetAsync(), Snackbar) is StatsDto statsDto)
        {
            if (_isAdmin)
            {
                UserCount = statsDto.UserCount;
                RoleCount = statsDto.RoleCount;
            }

            if (_isContact)
            {
                ContactCount = statsDto.ContactCount;
            }

            if (_isSMS)
            {
                _chartOptionMonths.YAxisTicks = 5000;
                _chartOptionDays.YAxisTicks = 1000;

                ContactCount = statsDto.ContactCount;
                SMSLogCount = statsDto.SmsLogCount;
                SMSTemplateCount = statsDto.SmsTemplateCount;

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

            if (_isCAD)
            {
                MemberCount = statsDto.MemberCount;
                AccountCount = statsDto.AccountCount;
            }

            BrandCount = statsDto.BrandCount;
            ProductCount = statsDto.ProductCount;
        }
    }
}