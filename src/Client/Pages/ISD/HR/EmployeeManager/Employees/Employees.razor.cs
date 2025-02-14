﻿using Blazored.LocalStorage;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Components.Dialogs.ISD.HR.PayrollManager;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Employees;

public partial class Employees
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    [Inject]
    protected IEmployeesClient Client { get; set; } = default!;

    [Inject]
    protected IEmployeePayrollDetailClient PayrollClient { get; set; } = default!;

    [Inject]
    private ILocalStorageService? _localStorage { get; set; }

    protected EntityServerTableContext<EmployeeDto, Guid, EmployeeViewModel> Context { get; set; } = default!;

    private EntityTable<EmployeeDto, Guid, EmployeeViewModel>? _table;

    private HashSet<EmployeeDto> _selectedItems = new();

    private string? _searchString { get; set; }

    private bool _canViewEmployees;
    private bool _canCreateEmployees;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewEmployees = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Employees);
        _canCreateEmployees = await AuthService.HasPermissionAsync(state.User, FSHAction.Create, FSHResource.Employees);

        Context = new(
                entityName: "Employee",
                entityNamePlural: "Employees",
                entityResource: FSHResource.Employees,
                fields: new()
                {
                    new(data => data.ImagePath, "Image", Template: TemplateImage),
                    new(data => data.Number, "ID Number", "Number", Template: TemplateActiveIdNumber),
                    new(data => data.HireDate, "Date Hired", "HireDate", typeof(DateTime), Template: TemplateHireDate),
                    new(data => data.RegularDate, "Date Regular", "RegularDate", typeof(DateTime), Template: TemplateRegularDate),
                    new(data => data.NameFull, "Name", "LastName", Template: TemplateNameAddress),
                    new(data => data.Address, "Address", visible: false),
                    new(data => data.Area, "Department", "Area", Template: TemplateAreaDepartment),
                    new(data => data.Department, "Department", visible: false),
                    new(data => data.Position, "Designation", "Position", Template: TemplatePositionType),
                    new(data => data.EmploymentType, "Employment Type", visible: false),
                    new(data => data.Notes, "Notes", "Notes")
                },
                enableAdvancedSearch: true,
                idFunc: data => data.Id,
                searchFunc: async filter => (await Client
                    .SearchAsync(filter.Adapt<EmployeeSearchRequest>()))
                    .Adapt<PaginationResponse<EmployeeDto>>(),
                createFunc: async request =>
                {
                    if (!string.IsNullOrEmpty(request.ImageInBytes))
                    {
                        request.Image = new ImageUploadRequest() { Data = request.ImageInBytes, Extension = request.ImageExtension ?? string.Empty, Name = $"{request.LastName}_{Guid.NewGuid():N}" };
                    }

                    await Client.CreateAsync(request.Adapt<EmployeeCreateRequest>());
                    request.ImageInBytes = string.Empty;
                },
                updateFunc: async (id, request) =>
                {
                    if (!string.IsNullOrEmpty(request.ImageInBytes))
                    {
                        request.DeleteCurrentImage = true;
                        request.Image = new ImageUploadRequest() { Data = request.ImageInBytes, Extension = request.ImageExtension ?? string.Empty, Name = $"{request.LastName}_{Guid.NewGuid():N}" };
                    }

                    await Client.UpdateAsync(id, request.Adapt<EmployeeUpdateRequest>());
                    request.ImageInBytes = string.Empty;
                },
                deleteFunc: async id => await Client.DeleteAsync(id),
                hasExtraActionsFunc: () => _canViewEmployees,
                exportAction: string.Empty);

        //var filter = new EmployeeSearchRequest { PageSize = 1000 };

        //await GetSearchString();
        await GetPayrollId();

        //if (await ApiHelper.ExecuteCallGuardedAsync(() => Client.SearchAsync(filter), Snackbar)
        //    is PaginationResponseOfEmployeeDto response)
        //{
        //    _list = response.Data.ToList();
        //}
    }

    // Advanced Search
    private Guid _searchPayrollId;

    private Guid SearchPayrollId
    {
        get => _searchPayrollId;
        set
        {
            _searchPayrollId = value;
            _ = _table!.ReloadDataAsync();
        }
    }

    private List<BreadcrumbItem> _breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
    };

    private async Task EmployeeGenerateDailySchedule(Guid employeeId)
    {
        DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        DialogParameters parameters = new()
        {
            { nameof(GenerateSchedule.EmployeeId), employeeId },
        };
        var dialog = DialogService.Show<GenerateSchedule>("Calculate", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            Snackbar.Add("Employee Daily Schedule has been successfully generated.", Severity.Success);
        }
    }

    private static string LengthOfService(in DateTime dtStart)
    {
        int years = DateTimeFunctions.Years(dtStart, DateTime.Today);
        int months = dtStart.Months(DateTime.Today) % 12;

        string sYear;

        if (years <= 1)
            sYear = "Year";
        else
            sYear = "Years";

        string sMonth;
        if (months <= 1)
            sMonth = "Month";
        else
            sMonth = "Months";

        if (months.Equals(0))
        {
            if (years.Equals(0))
            {
                return $"{years:N0} {sYear}";
            }
            else
            {
                return $"{years:N0} {sYear}";
            }
        }
        else
        {
            return $"{years:N0} {sYear} {months} {sMonth}";
        }
    }

    private async Task GetPayrollId()
    {
        try
        {
            string? _payrollId = await _localStorage!.GetItemAsync<string>("payrollId");

            if (_payrollId is not null)
            {
                _searchPayrollId = Guid.Parse(_payrollId);
            }

            StateHasChanged();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void SetPayrollId(in Guid payrollId) => _localStorage?.SetItemAsStringAsync("payrollId", payrollId.ToString());

    private async Task EmployeePayrollGenerate(Guid payrollId, EmployeeDto employee)
    {
        if (payrollId.Equals(Guid.Empty))
        {
            Snackbar.Add("Payroll is invalid or not specified.", Severity.Error);
            return;
        }

        //EmployeePayrollDetailGenerateRequest request = new()
        //{
        //    EmployeeId = employeeId,
        //    PayrollId = payrollId
        //};

        //if (await ApiHelper.ExecuteCallGuardedAsync(() => PayrollClient.GenerateAsync(request.Adapt<EmployeePayrollDetailGenerateRequest>()), Snackbar))
        //{
        //    Snackbar.Add("Employee Payroll has been successfully generated", Severity.Success);
        //    Navigation.NavigateTo($"/payroll/employeepayrolldetails/{employeeId}/{payrollId}");
        //}

        if (_selectedItems.Count == 0)
        {
            _selectedItems.Add(employee);
        }

        string transactionTitle = "Generate Employee(s) Payroll";
        string transactionContent = $"Are you sure you want to Generate Employee(s) Payroll?";
        DialogParameters parameters = new()
        {
            { nameof(TransactionConfirmation.TransactionTitle), transactionTitle },
            { nameof(TransactionConfirmation.ContentText), transactionContent },
        };
        DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        IDialogReference dialog = DialogService.Show<TransactionConfirmation>(transactionTitle, parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled && _selectedItems.Count > 0)
        {
            foreach (var employeeId in _selectedItems.Select(x => x.Id))
            {
                EmployeePayrollDetailGenerateRequest request = new()
                {
                    PayrollId = payrollId,
                    EmployeeId = employeeId,
                };

                if (await ApiHelper.ExecuteCallGuardedAsync(() => PayrollClient.GenerateAsync(request.Adapt<EmployeePayrollDetailGenerateRequest>()), Snackbar))
                {
                    Snackbar.Add("Employee Payroll has been successfully generated!", Severity.Success);
                }
            }
        }
    }

    private void NavigatePage(string page, in Guid employeeId, in Guid payrollId)
    {
        if (payrollId.Equals(Guid.Empty))
        {
            Snackbar.Add("Payroll is invalid or not specified.", Severity.Error);
            return;
        }

        Navigation.NavigateTo($"{page}/{employeeId}/{payrollId}");
    }

    private static int DisplayYearsOld(in DateTime dtBirthDate)
    {
        return DateTimeFunctions.Years(dtBirthDate, DateTime.Today);
    }

    // TODO : Make this as a shared service or something? Since it's used by Profile Component also for now, and literally any other component that will have image upload.
    // The new service should ideally return $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}"
    private async Task UploadImage(InputFileChangeEventArgs e)
    {
        if (e.File != null)
        {
            string? extension = Path.GetExtension(e.File.Name);
            if (!ApplicationConstants.SupportedImageFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("Image Format Not Supported.", Severity.Error);
                return;
            }

            Context.AddEditModal.RequestModel.ImageExtension = extension;
            var imageFile = await e.File.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
            byte[]? buffer = new byte[imageFile.Size];
            await imageFile.OpenReadStream(ApplicationConstants.MaxAllowedSize).ReadAsync(buffer);
            Context.AddEditModal.RequestModel.ImageInBytes = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            Context.AddEditModal.ForceRender();
        }
    }

    private void ClearImageInBytes()
    {
        Context.AddEditModal.RequestModel.ImageInBytes = string.Empty;
        Context.AddEditModal.ForceRender();
    }

    private void SetDeleteCurrentImageFlag()
    {
        Context.AddEditModal.RequestModel.ImageInBytes = string.Empty;
        Context.AddEditModal.RequestModel.ImagePath = string.Empty;
        Context.AddEditModal.RequestModel.DeleteCurrentImage = true;
        Context.AddEditModal.ForceRender();
    }
}

public class EmployeeViewModel : EmployeeUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}