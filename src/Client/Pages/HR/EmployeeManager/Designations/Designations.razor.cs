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

namespace ZANECO.WASM.Client.Pages.HR.EmployeeManager.Designations;
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

    private EntityTable<DesignationDto, Guid, DesignationViewModel> _table = default!;

    private bool _canViewRoleClaims;

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
        _canViewRoleClaims = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.RoleClaims);

        Context = new(
            entityName: "Designation",
            entityNamePlural: "Designations",
            entityResource: FSHResource.Designations,
            fields: new()
            {
                new(data => data.IdNumber, "IdNumber", "ID"),
                new(data => data.EmployeeName, "Employee", "EmployeeName"),
                new(data => data.Area, "Area", "Area"),

                // new(data => data.Department, "Department", "Department"),
                // new(data => data.Division, "Division", "Division"),
                // new(data => data.Section, "Section / Unit", "Section"),

                new(data => data.Position, "Designation", "Position"),
                new(data => data.EmploymentType, "Employment", "EmploymentType"),
                new(data => data.StartDate.ToString("MMM dd, yyyy"), "Effectivity", "StartDate"),
                new(data => data.EndDate.ToString("MMM dd, yyyy"), "Until", "EndDate"),
                new(data => data.SalaryRank, "Rank", "SalaryRank"),
                new(data => data.SalaryAmount.ToString("N2"), "SalaryAmount", "RatePerHour"),
                new(data => data.RatePerDay.ToString("N2"), "Rate/Day", "RatePerDay"),
                new(data => data.RatePerHour.ToString("N2"), "Rate/Hour", "RatePerHour"),

                // new(data => data.Description, "Description", "Description"),
                // new(data => data.Notes, "Notes", "Notes"),
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
                    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.EmployeeId}_{Guid.NewGuid():N}" };
                }

                await Client.CreateAsync(data.Adapt<DesignationCreateRequest>());

                data.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, data) =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.DeleteCurrentImage = true;
                    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.EmployeeId}_{Guid.NewGuid():N}" };
                }

                if (data.RateType.Equals("DAILY"))
                {
                    data.DaysPerMonth = 0;
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
            _ = _table.ReloadDataAsync();
        }
    }

    private string GetEmployeeId(Guid contextId)
    {
        if (SearchEmployeeId.Equals(Guid.Empty))
            return contextId.ToString();
        else
            return SearchEmployeeId.ToString();
    }

    private bool DisableInput(string rateType)
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