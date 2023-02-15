using MediatR.Courier;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Notifications;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Pages.Identity.Users;
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

    private readonly string[] _dataEnterBarChartXAxisLabels = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
    private readonly List<MudBlazor.ChartSeries> _dataBarChartSeriesSandurot = new();
    private readonly List<MudBlazor.ChartSeries> _dataBarChartSeriesSMS = new();
    private ChartOptions _chartOptions = new();

    private bool _loaded;

    private ClientPreference _preference = new();

    private bool _isAdmin;
    private bool _isISD;
    private bool _isCAD;
    private bool _isSandurot;
    private bool _isContact;
    private bool _isSMS;

    protected override async Task OnParametersSetAsync()
    {
        var state = await AuthState;
        _isAdmin = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.Users);
        _isISD = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.ISD);
        _isCAD = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.CAD);
        _isSandurot = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.Sandurot);
        _isContact = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.Contacts);
        _isSMS = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.SMS);
    }

    protected override async Task OnInitializedAsync()
    {
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
            SMSLogCount = statsDto.SMSLogCount;
            SMSTemplateCount = statsDto.SMSTemplateCount;

            MemberCount = statsDto.MemberCount;
            AccountCount = statsDto.AccountCount;

            BrandCount = statsDto.BrandCount;
            ProductCount = statsDto.ProductCount;

            _chartOptions.YAxisTicks = 1000;
            _chartOptions.LineStrokeWidth = 1;

            foreach (var item in statsDto.BarChartSandurot)
            {
                _dataBarChartSeriesSandurot.RemoveAll(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                _dataBarChartSeriesSandurot.Add(new MudBlazor.ChartSeries { Name = item.Name, Data = item.Data?.ToArray() });
            }

            foreach (var item in statsDto.BarChartSMS)
            {
                _dataBarChartSeriesSMS.RemoveAll(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                _dataBarChartSeriesSMS.Add(new MudBlazor.ChartSeries { Name = item.Name, Data = item.Data?.ToArray() });
            }
        }
    }
}