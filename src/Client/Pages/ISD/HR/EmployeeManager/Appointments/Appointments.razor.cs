﻿using Blazored.LocalStorage;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Appointments;

public partial class Appointments
{
    [Parameter]
    public Guid EmployeeId { get; set; } = Guid.Empty;

    [Inject]
    protected IAppointmentsClient Client { get; set; } = default!;

    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    [Inject]
    protected IPersonalClient User { get; set; } = default!;

    [Inject]
    protected ILocalStorageService LocalStorage { get; set; } = default!;

    protected EntityServerTableContext<AppointmentDto, int, AppointmentViewModel> Context { get; set; } = default!;

    private EntityTable<AppointmentDto, int, AppointmentViewModel>? _table;

    private bool _canViewEmployees;

    private string? _searchString;

    private List<AppointmentDto>? _appointments;

    private DateTime _startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime _endDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

    protected override void OnParametersSet()
    {
        if (EmployeeId != Guid.Empty)
        {
            _searchEmployeeId = EmployeeId;
        }
    }

    protected override async void OnInitialized()
    {
        var state = await AuthState;
        _canViewEmployees = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Employees);

        Context = new(
            entityName: "Appointment",
            entityNamePlural: "Appointments",
            entityResource: FSHResource.Appointment,
            fields: new()
            {
                new(data => data.Id, "Id", "Id"),
                new(data => data.EmployeeName, "Employee", "EmployeeName"),
                new(data => data.AppointmentType, "Type", "AppointmentType"),
                new(data => data.Subject, "Subject", "Subject", Template: TemplateSubjectLocation),
                new(data => data.StartDateTime, "", Type: typeof(DateTime), Template: TemplateStartEnd),
                new(data => data.Hours, "Hours", "Hours"),
                new(data => data.Status, "Status", "Status"),
                new(data => data.ApprovedOn, "", Type: typeof(DateTime), Template: TemplateRecommendApprove),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            enableAdvancedSearch: true,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                if (SearchEmployeeId.Equals(Guid.Empty))
                {
                    var user = await User.GetProfileAsync();
                    if (user.EmployeeId is not null)
                    {
                        _searchEmployeeId = (Guid)user.EmployeeId!;
                    }
                }

                var filter = _filter.Adapt<AppointmentSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;
                filter.StartDate = _startDate;
                filter.EndDate = _endDate;

                var result = await Client.SearchAsync(filter);

                return result.Adapt<PaginationResponse<AppointmentDto>>();
            },
            createFunc: async request =>
            {
                if (!string.IsNullOrEmpty(request.ImageInBytes))
                {
                    request.Image = new ImageUploadRequest() { Data = request.ImageInBytes, Extension = request.ImageExtension ?? string.Empty, Name = $"{request.Subject}_{Guid.NewGuid():N}" };
                }

                request.EmployeeId = _searchEmployeeId;

                await Client.CreateAsync(request.Adapt<AppointmentCreateRequest>());

                request.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, request) =>
            {
                if (!string.IsNullOrEmpty(request.ImageInBytes))
                {
                    request.DeleteCurrentImage = true;
                    request.Image = new ImageUploadRequest() { Data = request.ImageInBytes, Extension = request.ImageExtension ?? string.Empty, Name = $"{request.Subject}_{Guid.NewGuid():N}" };
                }

                await Client.UpdateAsync(id, request.Adapt<AppointmentUpdateRequest>());

                request.ImageInBytes = string.Empty;
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            canUpdateEntityFunc: dto => dto.EmployeeId.Equals(SearchEmployeeId),
            canDeleteEntityFunc: dto => dto.EmployeeId.Equals(SearchEmployeeId),
            exportAction: string.Empty);
    }

    // Advanced Search
    private Guid _searchEmployeeId;

    private Guid SearchEmployeeId
    {
        get => _searchEmployeeId;
        set
        {
            _searchEmployeeId = value;
            _ = _table!.ReloadDataAsync();
        }
    }

    private List<MudBlazor.BreadcrumbItem> _breadcrumbs = new List<MudBlazor.BreadcrumbItem>
    {
        new BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
        new BreadcrumbItem("Employees", href: "/hr/employees", icon: Icons.Material.Filled.Groups),
    };

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

    private async void ForAction(int id, string action)
    {
        string transactionTitle = $"{action} Appointment";
        string transactionContent = $"Are you sure you want to {action} an Appointment?";
        DialogParameters parameters = new()
        {
            { nameof(TransactionConfirmation.TransactionTitle), transactionTitle },
            { nameof(TransactionConfirmation.ContentText), transactionContent },
        };
        DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        IDialogReference dialog = DialogService.Show<TransactionConfirmation>(transactionTitle, parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            var user = await User.GetProfileAsync();
            if (user.EmployeeId is not null)
            {
                AppointmentActionRequest request = new()
                {
                    Id = id,
                    UserEmployeeId = (Guid)user.EmployeeId,
                    Action = action
                };
                
                await Client.ActionAsync(request);

                await _table!.ReloadDataAsync();
            }
        }
    }
}

public class AppointmentViewModel : AppointmentUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}