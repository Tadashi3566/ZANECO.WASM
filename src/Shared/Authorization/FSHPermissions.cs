using System.Collections.ObjectModel;

namespace ZANECO.WebApi.Shared.Authorization;

public static class FSHAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Export = nameof(Export);
    public const string Generate = nameof(Generate);
    public const string Clean = nameof(Clean);
    public const string UpgradeSubscription = nameof(UpgradeSubscription);
}

public static class FSHResource
{
    public const string Tenants = nameof(Tenants);
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);
    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);

    public const string Brands = nameof(Brands);
    public const string Products = nameof(Products);

    public const string Groups = nameof(Groups);
    public const string Tickets = nameof(Tickets);
    public const string Contacts = nameof(Contacts);
    public const string SMS = nameof(SMS);

    public const string Rating = nameof(Rating);
    public const string Surveys = nameof(Surveys);

    public const string Accounting = nameof(Accounting);
    public const string Employees = nameof(Employees);
    public const string Employers = nameof(Employers);
    public const string Designations = nameof(Designations);
    public const string Dependents = nameof(Dependents);
    public const string Powerbills = nameof(Powerbills);

    public const string Ranks = nameof(Ranks);
    public const string Adjustments = nameof(Adjustments);
    public const string EmployeeAdjustments = nameof(EmployeeAdjustments);

    public const string Schedules = nameof(Schedules);
    public const string ScheduleDetails = nameof(ScheduleDetails);
    public const string Attendance = nameof(Attendance);

    public const string Payroll = nameof(Payroll);

    public const string CAD = nameof(CAD);
    public const string Raffles = nameof(Raffles);
}

public static class FSHPermissions
{
    private static readonly FSHPermission[] _all = new FSHPermission[]
    {
        new("View Dashboard", FSHAction.View, FSHResource.Dashboard),
        new("View Hangfire", FSHAction.View, FSHResource.Hangfire),

        new("View Users", FSHAction.View, FSHResource.Users),
        new("Search Users", FSHAction.Search, FSHResource.Users),
        new("Create Users", FSHAction.Create, FSHResource.Users),
        new("Update Users", FSHAction.Update, FSHResource.Users),
        new("Delete Users", FSHAction.Delete, FSHResource.Users),
        new("Export Users", FSHAction.Export, FSHResource.Users),

        new("View UserRoles", FSHAction.View, FSHResource.UserRoles),
        new("Update UserRoles", FSHAction.Update, FSHResource.UserRoles),
        new("View Roles", FSHAction.View, FSHResource.Roles),
        new("Create Roles", FSHAction.Create, FSHResource.Roles),
        new("Update Roles", FSHAction.Update, FSHResource.Roles),
        new("Delete Roles", FSHAction.Delete, FSHResource.Roles),

        new("View RoleClaims", FSHAction.View, FSHResource.RoleClaims),
        new("Update RoleClaims", FSHAction.Update, FSHResource.RoleClaims),

        new("View Brands", FSHAction.View, FSHResource.Brands, IsBasic: true),
        new("Search Brands", FSHAction.Search, FSHResource.Brands, IsBasic: true),
        new("Create Brands", FSHAction.Create, FSHResource.Brands),
        new("Update Brands", FSHAction.Update, FSHResource.Brands),
        new("Delete Brands", FSHAction.Delete, FSHResource.Brands),
        new("Generate Brands", FSHAction.Generate, FSHResource.Brands),
        new("Clean Brands", FSHAction.Clean, FSHResource.Brands),

        new("View Products", FSHAction.View, FSHResource.Products, IsBasic: true),
        new("Search Products", FSHAction.Search, FSHResource.Products, IsBasic: true),
        new("Create Products", FSHAction.Create, FSHResource.Products),
        new("Update Products", FSHAction.Update, FSHResource.Products),
        new("Delete Products", FSHAction.Delete, FSHResource.Products),
        new("Export Products", FSHAction.Export, FSHResource.Products),

        new("View Tenants", FSHAction.View, FSHResource.Tenants, IsRoot: true),
        new("Create Tenants", FSHAction.Create, FSHResource.Tenants, IsRoot: true),
        new("Update Tenants", FSHAction.Update, FSHResource.Tenants, IsRoot: true),
        new("Upgrade Tenant Subscription", FSHAction.UpgradeSubscription, FSHResource.Tenants, IsRoot: true),

        new("View Groups", FSHAction.View, FSHResource.Groups, IsBasic: true),
        new("Search Groups", FSHAction.Search, FSHResource.Groups, IsBasic: true),
        new("Create Groups", FSHAction.Create, FSHResource.Groups),
        new("Update Groups", FSHAction.Update, FSHResource.Groups),
        new("Delete Groups", FSHAction.Delete, FSHResource.Groups),
        new("Export Groups", FSHAction.Export, FSHResource.Groups),

        new("View Tickets", FSHAction.View, FSHResource.Tickets, IsBasic: true),
        new("Search Tickets", FSHAction.Search, FSHResource.Tickets, IsBasic: true),
        new("Create Tickets", FSHAction.Create, FSHResource.Tickets),
        new("Update Tickets", FSHAction.Update, FSHResource.Tickets),
        new("Delete Tickets", FSHAction.Delete, FSHResource.Tickets),
        new("Export Tickets", FSHAction.Export, FSHResource.Tickets),

        new("View Contacts", FSHAction.View, FSHResource.Contacts, IsBasic: true),
        new("Search Contacts", FSHAction.Search, FSHResource.Contacts, IsBasic: true),
        new("Create Contacts", FSHAction.Create, FSHResource.Contacts),
        new("Update Contacts", FSHAction.Update, FSHResource.Contacts),
        new("Delete Contacts", FSHAction.Delete, FSHResource.Contacts),
        new("Export Contacts", FSHAction.Export, FSHResource.Contacts),

        new("View SMS", FSHAction.View, FSHResource.SMS, IsBasic: true),
        new("Search SMS", FSHAction.Search, FSHResource.SMS, IsBasic: true),
        new("Create SMS", FSHAction.Create, FSHResource.SMS),
        new("Update SMS", FSHAction.Update, FSHResource.SMS),
        new("Delete SMS", FSHAction.Delete, FSHResource.SMS),
        new("Export SMS", FSHAction.Export, FSHResource.SMS),

        new("View Rating", FSHAction.View, FSHResource.Rating, IsBasic: true),
        new("Search Rating", FSHAction.Search, FSHResource.Rating, IsBasic: true),
        new("Create Rating", FSHAction.Create, FSHResource.Rating),
        new("Update Rating", FSHAction.Update, FSHResource.Rating),
        new("Delete Rating", FSHAction.Delete, FSHResource.Rating),
        new("Export Rating", FSHAction.Export, FSHResource.Rating),

        new("View Rating Surveys", FSHAction.View, FSHResource.Surveys, IsBasic: true),
        new("Search Rating Surveys", FSHAction.Search, FSHResource.Surveys, IsBasic: true),
        new("Create Rating Surveys", FSHAction.Create, FSHResource.Surveys),
        new("Update Rating Surveys", FSHAction.Update, FSHResource.Surveys),
        new("Delete Rating Surveys", FSHAction.Delete, FSHResource.Surveys),
        new("Export Rating Surveys", FSHAction.Export, FSHResource.Surveys),

        new("View Accounting", FSHAction.View, FSHResource.Accounting, IsBasic: true),
        new("Search Accounting", FSHAction.Search, FSHResource.Accounting, IsBasic: true),
        new("Create Accounting", FSHAction.Create, FSHResource.Accounting),
        new("Update Accounting", FSHAction.Update, FSHResource.Accounting),
        new("Delete Accounting", FSHAction.Delete, FSHResource.Accounting),
        new("Export Accounting", FSHAction.Export, FSHResource.Accounting),

        new("View Employees", FSHAction.View, FSHResource.Employees, IsBasic: true),
        new("Search Employees", FSHAction.Search, FSHResource.Employees, IsBasic: true),
        new("Create Employees", FSHAction.Create, FSHResource.Employees),
        new("Update Employees", FSHAction.Update, FSHResource.Employees),
        new("Delete Employees", FSHAction.Delete, FSHResource.Employees),
        new("Export Employees", FSHAction.Export, FSHResource.Employees),

        new("View Employers", FSHAction.View, FSHResource.Employers, IsBasic: true),
        new("Search Employers", FSHAction.Search, FSHResource.Employers, IsBasic: true),
        new("Create Employers", FSHAction.Create, FSHResource.Employers),
        new("Update Employers", FSHAction.Update, FSHResource.Employers),
        new("Delete Employers", FSHAction.Delete, FSHResource.Employers),
        new("Export Employers", FSHAction.Export, FSHResource.Employers),

        new("View Designations", FSHAction.View, FSHResource.Designations, IsBasic: true),
        new("Search Designations", FSHAction.Search, FSHResource.Designations, IsBasic: true),
        new("Create Designations", FSHAction.Create, FSHResource.Designations),
        new("Update Designations", FSHAction.Update, FSHResource.Designations),
        new("Delete Designations", FSHAction.Delete, FSHResource.Designations),
        new("Export Designations", FSHAction.Export, FSHResource.Designations),

        new("View Dependents", FSHAction.View, FSHResource.Dependents, IsBasic: true),
        new("Search Dependents", FSHAction.Search, FSHResource.Dependents, IsBasic: true),
        new("Create Dependents", FSHAction.Create, FSHResource.Dependents),
        new("Update Dependents", FSHAction.Update, FSHResource.Dependents),
        new("Delete Dependents", FSHAction.Delete, FSHResource.Dependents),
        new("Export Dependents", FSHAction.Export, FSHResource.Dependents),

        new("View Powerbills", FSHAction.View, FSHResource.Powerbills, IsBasic: true),
        new("Search Powerbills", FSHAction.Search, FSHResource.Powerbills, IsBasic: true),
        new("Create Powerbills", FSHAction.Create, FSHResource.Powerbills),
        new("Update Powerbills", FSHAction.Update, FSHResource.Powerbills),
        new("Delete Powerbills", FSHAction.Delete, FSHResource.Powerbills),
        new("Export Powerbills", FSHAction.Export, FSHResource.Powerbills),

        new("View Ranks", FSHAction.View, FSHResource.Ranks, IsBasic: true),
        new("Search Ranks", FSHAction.Search, FSHResource.Ranks, IsBasic: true),
        new("Create Ranks", FSHAction.Create, FSHResource.Ranks),
        new("Update Ranks", FSHAction.Update, FSHResource.Ranks),
        new("Delete Ranks", FSHAction.Delete, FSHResource.Ranks),
        new("Export Ranks", FSHAction.Export, FSHResource.Ranks),

        new("View Adjustments", FSHAction.View, FSHResource.Adjustments, IsBasic: true),
        new("Search Adjustments", FSHAction.Search, FSHResource.Adjustments, IsBasic: true),
        new("Create Adjustments", FSHAction.Create, FSHResource.Adjustments),
        new("Update Adjustments", FSHAction.Update, FSHResource.Adjustments),
        new("Delete Adjustments", FSHAction.Delete, FSHResource.Adjustments),
        new("Export Adjustments", FSHAction.Export, FSHResource.Adjustments),

        new("View Employee Adjustments", FSHAction.View, FSHResource.EmployeeAdjustments, IsBasic: true),
        new("Search Employee Adjustments", FSHAction.Search, FSHResource.EmployeeAdjustments, IsBasic: true),
        new("Create Employee Adjustments", FSHAction.Create, FSHResource.EmployeeAdjustments),
        new("Update Employee Adjustments", FSHAction.Update, FSHResource.EmployeeAdjustments),
        new("Delete Employee Adjustments", FSHAction.Delete, FSHResource.EmployeeAdjustments),
        new("Export Employee Adjustments", FSHAction.Export, FSHResource.EmployeeAdjustments),

        new("View Schedules", FSHAction.View, FSHResource.Schedules, IsBasic: true),
        new("Search Schedules", FSHAction.Search, FSHResource.Schedules, IsBasic: true),
        new("Create Schedules", FSHAction.Create, FSHResource.Schedules),
        new("Update Schedules", FSHAction.Update, FSHResource.Schedules),
        new("Delete Schedules", FSHAction.Delete, FSHResource.Schedules),
        new("Export Schedules", FSHAction.Export, FSHResource.Schedules),

        new("View Schedule Details", FSHAction.View, FSHResource.ScheduleDetails, IsBasic: true),
        new("Search Schedule Details", FSHAction.Search, FSHResource.ScheduleDetails, IsBasic: true),
        new("Create Schedule Details", FSHAction.Create, FSHResource.ScheduleDetails),
        new("Update Schedule Details", FSHAction.Update, FSHResource.ScheduleDetails),
        new("Delete Schedule Details", FSHAction.Delete, FSHResource.ScheduleDetails),
        new("Export Schedule Details", FSHAction.Export, FSHResource.ScheduleDetails),

        new("View Attendance", FSHAction.View, FSHResource.Attendance, IsBasic: true),
        new("Search Attendance", FSHAction.Search, FSHResource.Attendance, IsBasic: true),
        new("Create Attendance", FSHAction.Create, FSHResource.Attendance),
        new("Update Attendance", FSHAction.Update, FSHResource.Attendance),
        new("Delete Attendance", FSHAction.Delete, FSHResource.Attendance),
        new("Export Attendance", FSHAction.Export, FSHResource.Attendance),

        new("View Payroll", FSHAction.View, FSHResource.Payroll, IsBasic: true),
        new("Search Payroll", FSHAction.Search, FSHResource.Payroll, IsBasic: true),
        new("Create Payroll", FSHAction.Create, FSHResource.Payroll),
        new("Update Payroll", FSHAction.Update, FSHResource.Payroll),
        new("Delete Payroll", FSHAction.Delete, FSHResource.Payroll),
        new("Export Payroll", FSHAction.Export, FSHResource.Payroll),

        new("View CAD Aplications", FSHAction.View, FSHResource.CAD, IsBasic: true),
        new("Search CAD Aplications", FSHAction.Search, FSHResource.CAD, IsBasic: true),
        new("Create CAD Aplications", FSHAction.Create, FSHResource.CAD),
        new("Update CAD Aplications", FSHAction.Update, FSHResource.CAD),
        new("Delete CAD Aplications", FSHAction.Delete, FSHResource.CAD),
        new("Export CAD Aplications", FSHAction.Export, FSHResource.CAD),

        new("View Raffle", FSHAction.View, FSHResource.Raffles, IsBasic: true),
        new("Search Raffle", FSHAction.Search, FSHResource.Raffles, IsBasic: true),
        new("Create Raffle", FSHAction.Create, FSHResource.Raffles),
        new("Update Raffle", FSHAction.Update, FSHResource.Raffles),
        new("Delete Raffle", FSHAction.Delete, FSHResource.Raffles),
        new("Export Raffle", FSHAction.Export, FSHResource.Raffles),
    };

    public static IReadOnlyList<FSHPermission> All { get; } = new ReadOnlyCollection<FSHPermission>(_all);
    public static IReadOnlyList<FSHPermission> Root { get; } = new ReadOnlyCollection<FSHPermission>(_all.Where(p => p.IsRoot).ToArray());
    public static IReadOnlyList<FSHPermission> Admin { get; } = new ReadOnlyCollection<FSHPermission>(_all.Where(p => !p.IsRoot).ToArray());
    public static IReadOnlyList<FSHPermission> Basic { get; } = new ReadOnlyCollection<FSHPermission>(_all.Where(p => p.IsBasic).ToArray());
}

public record FSHPermission(string Description, string Action, string Resource, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Resource);
    public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
}