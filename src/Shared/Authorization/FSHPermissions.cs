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

    public const string Documents = nameof(Documents);
    public const string FileManager = nameof(FileManager);

    public const string Sandurot = nameof(Sandurot);
    public const string Brands = nameof(Brands);
    public const string Products = nameof(Products);

    public const string Groups = nameof(Groups);
    public const string Tickets = nameof(Tickets);
    public const string Contacts = nameof(Contacts);
    public const string SMS = nameof(SMS);

    public const string Rating = nameof(Rating);
    public const string Surveys = nameof(Surveys);

    public const string Accounting = nameof(Accounting);

    public const string ISD = nameof(ISD);
    public const string Employees = nameof(Employees);
    public const string Attendance = nameof(Attendance);

    public const string Schedules = nameof(Schedules);

    public const string Payroll = nameof(Payroll);

    public const string CAD = nameof(CAD);

    public const string Raffles = nameof(Raffles);
}

public static class FSHPermissions
{
    private static readonly FSHPermission[] _all = new FSHPermission[]
    {
        new("View Dashboard", FSHAction.View, FSHResource.Dashboard, IsBasic: true),
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

        new("View Documents", FSHAction.View, FSHResource.Documents),
        new("Search Documents", FSHAction.Search, FSHResource.Documents),
        new("Create Documents", FSHAction.Create, FSHResource.Documents),
        new("Update Documents", FSHAction.Update, FSHResource.Documents),
        new("Delete Documents", FSHAction.Delete, FSHResource.Documents),

        new("View FileManager", FSHAction.View, FSHResource.FileManager),
        new("Search FileManager", FSHAction.Search, FSHResource.FileManager),
        new("Create FileManager", FSHAction.Create, FSHResource.FileManager),
        new("Update FileManager", FSHAction.Update, FSHResource.FileManager),
        new("Delete FileManager", FSHAction.Delete, FSHResource.FileManager),

        new("View Sandurot", FSHAction.View, FSHResource.Sandurot),
        new("Search Sandurot", FSHAction.Search, FSHResource.Sandurot),
        new("Create Sandurot", FSHAction.Create, FSHResource.Sandurot),
        new("Update Sandurot", FSHAction.Update, FSHResource.Sandurot),
        new("Delete Sandurot", FSHAction.Delete, FSHResource.Sandurot),

        new("View Brands", FSHAction.View, FSHResource.Brands),
        new("Search Brands", FSHAction.Search, FSHResource.Brands),
        new("Create Brands", FSHAction.Create, FSHResource.Brands),
        new("Update Brands", FSHAction.Update, FSHResource.Brands),
        new("Delete Brands", FSHAction.Delete, FSHResource.Brands),
        new("Generate Brands", FSHAction.Generate, FSHResource.Brands),
        new("Clean Brands", FSHAction.Clean, FSHResource.Brands),

        new("View Products", FSHAction.View, FSHResource.Products),
        new("Search Products", FSHAction.Search, FSHResource.Products),
        new("Create Products", FSHAction.Create, FSHResource.Products),
        new("Update Products", FSHAction.Update, FSHResource.Products),
        new("Delete Products", FSHAction.Delete, FSHResource.Products),
        new("Export Products", FSHAction.Export, FSHResource.Products),

        new("View Tenants", FSHAction.View, FSHResource.Tenants, IsRoot: true),
        new("Create Tenants", FSHAction.Create, FSHResource.Tenants, IsRoot: true),
        new("Update Tenants", FSHAction.Update, FSHResource.Tenants, IsRoot: true),
        new("Upgrade Tenant Subscription", FSHAction.UpgradeSubscription, FSHResource.Tenants, IsRoot: true),

        new("View Groups", FSHAction.View, FSHResource.Groups),
        new("Search Groups", FSHAction.Search, FSHResource.Groups),
        new("Create Groups", FSHAction.Create, FSHResource.Groups),
        new("Update Groups", FSHAction.Update, FSHResource.Groups),
        new("Delete Groups", FSHAction.Delete, FSHResource.Groups),
        new("Export Groups", FSHAction.Export, FSHResource.Groups),

        new("View Tickets", FSHAction.View, FSHResource.Tickets, IsBasic: true),
        new("Search Tickets", FSHAction.Search, FSHResource.Tickets, IsBasic: true),
        new("Create Tickets", FSHAction.Create, FSHResource.Tickets, IsBasic: true),
        new("Update Tickets", FSHAction.Update, FSHResource.Tickets),
        new("Delete Tickets", FSHAction.Delete, FSHResource.Tickets),
        new("Export Tickets", FSHAction.Export, FSHResource.Tickets),

        new("View Contacts", FSHAction.View, FSHResource.Contacts),
        new("Search Contacts", FSHAction.Search, FSHResource.Contacts),
        new("Create Contacts", FSHAction.Create, FSHResource.Contacts),
        new("Update Contacts", FSHAction.Update, FSHResource.Contacts),
        new("Delete Contacts", FSHAction.Delete, FSHResource.Contacts),
        new("Export Contacts", FSHAction.Export, FSHResource.Contacts),

        new("View SMS", FSHAction.View, FSHResource.SMS),
        new("Search SMS", FSHAction.Search, FSHResource.SMS),
        new("Create SMS", FSHAction.Create, FSHResource.SMS),
        new("Update SMS", FSHAction.Update, FSHResource.SMS),
        new("Delete SMS", FSHAction.Delete, FSHResource.SMS),
        new("Export SMS", FSHAction.Export, FSHResource.SMS),

        new("View Rating", FSHAction.View, FSHResource.Rating),
        new("Search Rating", FSHAction.Search, FSHResource.Rating),
        new("Create Rating", FSHAction.Create, FSHResource.Rating),
        new("Update Rating", FSHAction.Update, FSHResource.Rating),
        new("Delete Rating", FSHAction.Delete, FSHResource.Rating),
        new("Export Rating", FSHAction.Export, FSHResource.Rating),

        new("View Surveys", FSHAction.View, FSHResource.Surveys),
        new("Search Surveys", FSHAction.Search, FSHResource.Surveys),
        new("Create Surveys", FSHAction.Create, FSHResource.Surveys, IsBasic: true),
        new("Update Surveys", FSHAction.Update, FSHResource.Surveys),
        new("Delete Surveys", FSHAction.Delete, FSHResource.Surveys),
        new("Export Surveys", FSHAction.Export, FSHResource.Surveys),

        new("View Accounting", FSHAction.View, FSHResource.Accounting),
        new("Search Accounting", FSHAction.Search, FSHResource.Accounting),
        new("Create Accounting", FSHAction.Create, FSHResource.Accounting),
        new("Update Accounting", FSHAction.Update, FSHResource.Accounting),
        new("Delete Accounting", FSHAction.Delete, FSHResource.Accounting),
        new("Export Accounting", FSHAction.Export, FSHResource.Accounting),

        new("View Employees", FSHAction.View, FSHResource.Employees),
        new("Search Employees", FSHAction.Search, FSHResource.Employees),
        new("Create Employees", FSHAction.Create, FSHResource.Employees),
        new("Update Employees", FSHAction.Update, FSHResource.Employees),
        new("Delete Employees", FSHAction.Delete, FSHResource.Employees),
        new("Export Employees", FSHAction.Export, FSHResource.Employees),

        new("View Schedules", FSHAction.View, FSHResource.Schedules),
        new("Search Schedules", FSHAction.Search, FSHResource.Schedules),
        new("Create Schedules", FSHAction.Create, FSHResource.Schedules),
        new("Update Schedules", FSHAction.Update, FSHResource.Schedules),
        new("Delete Schedules", FSHAction.Delete, FSHResource.Schedules),
        new("Export Schedules", FSHAction.Export, FSHResource.Schedules),

        new("View Attendance", FSHAction.View, FSHResource.Attendance),
        new("Search Attendance", FSHAction.Search, FSHResource.Attendance),
        new("Create Attendance", FSHAction.Create, FSHResource.Attendance),
        new("Update Attendance", FSHAction.Update, FSHResource.Attendance),
        new("Delete Attendance", FSHAction.Delete, FSHResource.Attendance),
        new("Export Attendance", FSHAction.Export, FSHResource.Attendance),

        new("View Payroll", FSHAction.View, FSHResource.Payroll),
        new("Search Payroll", FSHAction.Search, FSHResource.Payroll),
        new("Create Payroll", FSHAction.Create, FSHResource.Payroll),
        new("Update Payroll", FSHAction.Update, FSHResource.Payroll),
        new("Delete Payroll", FSHAction.Delete, FSHResource.Payroll),
        new("Export Payroll", FSHAction.Export, FSHResource.Payroll),

        new("View CAD Aplications", FSHAction.View, FSHResource.CAD),
        new("Search CAD Aplications", FSHAction.Search, FSHResource.CAD),
        new("Create CAD Aplications", FSHAction.Create, FSHResource.CAD),
        new("Update CAD Aplications", FSHAction.Update, FSHResource.CAD),
        new("Delete CAD Aplications", FSHAction.Delete, FSHResource.CAD),
        new("Export CAD Aplications", FSHAction.Export, FSHResource.CAD),

        new("View ISD Aplications", FSHAction.View, FSHResource.ISD),
        new("Search ISD Aplications", FSHAction.Search, FSHResource.ISD),
        new("Create ISD Aplications", FSHAction.Create, FSHResource.ISD),
        new("Update ISD Aplications", FSHAction.Update, FSHResource.ISD),
        new("Delete ISD Aplications", FSHAction.Delete, FSHResource.ISD),
        new("Export ISD Aplications", FSHAction.Export, FSHResource.ISD),

        new("View Raffle", FSHAction.View, FSHResource.Raffles),
        new("Search Raffle", FSHAction.Search, FSHResource.Raffles),
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