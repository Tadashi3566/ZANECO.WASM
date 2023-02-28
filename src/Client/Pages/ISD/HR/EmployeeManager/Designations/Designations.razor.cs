using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Designations;
public partial class Designations
{
    [Parameter]
    public Guid EmployeeId { get; set; } = Guid.Empty;
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IDesignationsClient Client { get; set; } = default!;

    protected EntityServerTableContext<DesignationDto, Guid, DesignationViewModel> Context { get; set; } = default!;

    private EntityTable<DesignationDto, Guid, DesignationViewModel>? _table;

    private string? _searchString;

    private bool _canViewEmployees;

    protected override void OnParametersSet()
    {
        if (EmployeeId != Guid.Empty)
        {
            _searchEmployeeId = EmployeeId;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewEmployees = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Employees);

        Context = new(
            entityName: "Designation",
            entityNamePlural: "Designations",
            entityResource: FSHResource.Employees,
            fields: new()
            {
                new(data => data.IdNumber, "IdNumber", "ID", visible: EmployeeId.Equals(Guid.Empty)),
                new(data => data.EmployeeName, "Employee", "EmployeeName", visible: EmployeeId.Equals(Guid.Empty)),
                new(data => data.Area, "Area", "Area", Template: TemplateAreaDepartment),
                new(data => data.Department, "Department", visible: false),
                new(data => data.Division, "Division", "Division", Template: TemplateDivisionSection),
                new(data => data.Section, "Section / Unit", visible: false),
                new(data => data.Position, "Designation", "Position", Template: TemplatePositionType),
                new(data => data.EmploymentType, "Employment", visible: false),
                new(data => data.StartDate, "Effectivity", "StartDate", Template: TemplateEffectivityUntil),
                new(data => data.EndDate, "Until", "EndDate", visible: false),
                new(data => data.SalaryRank, "Rank", "SalaryRank"),
                new(data => data.SalaryAmount, "SalaryAmount", "RatePerHour", typeof(decimal)),
                new(data => data.RatePerHour, "Rate/Hour", "RatePerHour", typeof(decimal)),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", "Notes", visible: false),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<DesignationSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<DesignationDto>>();
            },
            createFunc: async data =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new ImageUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.EmployeeId}_{Guid.NewGuid():N}" };
                }

                data.EmployeeId = _searchEmployeeId;

                await Client.CreateAsync(data.Adapt<DesignationCreateRequest>());

                data.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, data) =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.DeleteCurrentImage = true;
                    data.Image = new ImageUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.EmployeeId}_{Guid.NewGuid():N}" };
                }

                if (data.RateType.Equals("MONTHLY"))
                {
                    data.RatePerDay = 0;
                    data.RatePerHour = 0;
                }

                await Client.UpdateAsync(id, data.Adapt<DesignationUpdateRequest>());

                data.ImageInBytes = string.Empty;
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
    }

    private async Task SetCurrentDesignation(Guid employeeId, Guid designationId)
    {
        DesignationCurrentRequest request = new()
        {
            EmployeeId = employeeId,
            DesignationId = designationId
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => Client.DesignationCurrentAsync(request.Adapt<DesignationCurrentRequest>()), Snackbar))
        {
            Snackbar.Add("Current Employee Designation has been successfully selected", Severity.Success);

            await _table!.ReloadDataAsync();
        }
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

    private List<BreadcrumbItem> _breadcrumbs = new List<BreadcrumbItem>
    {
        new BreadcrumbItem("Employees", href: "/hr/employees"),
    };

    private static bool DisableInput(string rateType)
    {
        if (rateType is not null)
        {
            return rateType.Equals("MONTHLY");
        }

        return false;
    }

    // TODO : Make this as a shared service or something? Since it's used by Profile Component also for now, and literally any other component that will have image upload.
    // The new service should ideally return $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}"

    private async Task UploadFiles(InputFileChangeEventArgs e)
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

public class DesignationViewModel : DesignationUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}