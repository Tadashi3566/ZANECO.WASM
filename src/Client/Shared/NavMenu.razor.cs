using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Shared;

public partial class NavMenu
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    [Inject]
    protected IPersonalClient User { get; set; } = default!;

    private Guid? _employeeId { get; set; }

    private string? _hangfireUrl;
    private bool _canViewHangfire;
    private bool _canViewDashboard;
    private bool _canViewRoles;
    private bool _canViewUsers;

    private bool _canViewDocuments;
    private bool _canViewFiles;

    private bool _canViewProducts;
    private bool _canViewBrands;
    private bool _canViewTenants;

    private bool _canViewGroups;
    private bool _canViewTickets;
    private bool _canViewContacts;
    private bool _canViewSMS;

    private bool _canViewRating;
    private bool _canViewSurveys;
    private bool _canCreateComment;

    private bool _canViewAccounting;

    private bool _canViewISD;
    private bool _canViewEmployees;
    private bool _canViewCalendar;
    private bool _canCreateAttendance;
    private bool _canViewAttendance;
    private bool _canViewAppointment;

    private bool _canViewPayroll;

    private bool _canViewCAD;
    private bool _canViewRaffle;

    private bool CanViewAdministrationGroup => _canViewUsers || _canViewRoles || _canViewTenants;

    protected override async Task OnParametersSetAsync()
    {
        _hangfireUrl = Config[ConfigNames.ApiBaseUrl] + "jobs";

        var user = (await AuthState).User;

        _canViewHangfire = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Hangfire);
        _canViewDashboard = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Dashboard);
        _canViewRoles = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Roles);
        _canViewUsers = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Users);

        _canViewDocuments = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Documents);
        _canViewFiles = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.FileManager);

        _canViewProducts = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Products);
        _canViewBrands = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Brands);
        _canViewTenants = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Tenants);

        _canViewGroups = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Groups);
        _canViewTickets = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Tickets);
        _canViewContacts = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Contacts);
        _canViewSMS = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.SMS);

        _canViewRating = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Rating);
        _canViewSurveys = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Surveys);
        _canCreateComment = await AuthService.HasPermissionAsync(user, FSHAction.Create, FSHResource.Surveys);

        _canViewISD = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.ISD);

        _canViewEmployees = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Employees);
        _canViewCalendar = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Calendar);
        _canCreateAttendance = await AuthService.HasPermissionAsync(user, FSHAction.Create, FSHResource.Attendance);
        _canViewAttendance = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Attendance);
        _canViewAppointment = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Appointment);

        _canViewPayroll = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Payroll);

        _canViewAccounting = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Accounting);

        _canViewCAD = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.CAD);

        _canViewRaffle = await AuthService.HasPermissionAsync(user, FSHAction.View, FSHResource.Raffles);
    }
}