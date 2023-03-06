using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.Loans;

public partial class Loans
{
    [Parameter]
    public Guid EmployeeId { get; set; } = default!;
    [Inject]
    protected ILoanClient Client { get; set; } = default!;

    protected EntityServerTableContext<LoanDto, Guid, LoanViewModel> Context { get; set; } = default!;

    private EntityTable<LoanDto, Guid, LoanViewModel>? _table;

    private string? _searchString;
    private DateTime? _dtend;
    private decimal _ammortization = 0;

    protected override void OnParametersSet()
    {
        if (EmployeeId != Guid.Empty)
        {
            _searchEmployeeId = EmployeeId;
        }
    }

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Employee Loan",
            entityNamePlural: "Employee Loans",
            entityResource: FSHResource.Payroll,
            fields: new()
            {
                new(data => data.ImagePath, "Image", Template: TemplateImage),
                new(data => data.EmployeeName, "Employee", "EmployeeName"),
                new(data => data.AdjustmentName, "Name", "Name"),
                new(data => data.PaymentSchedule, "Schedule", "PaymentSchedule"),
                new(data => data.DateStart, "Date Start-End", "DateStart", Template: TemplateDate),
                new(data => data.Months, "Months", "Months"),
                new(data => data.DateEnd, visible: false),
                new(data => data.Amount, "Amount", "Amount", typeof(decimal)),
                new(data => data.Ammortization, "Ammortization", "Ammortization", typeof(decimal)),
                new(data => data.Status, "Status", "Status"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, visible: false),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<LoanSearchRequest>();

                filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<LoanDto>>();
            },
            createFunc: async data =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new ImageUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.AdjustmentId}_{Guid.NewGuid():N}" };
                }

                data.EmployeeId = SearchEmployeeId;

                SetDateEnd(data.PaymentSchedule, data.Amount, data.DateStart, data.Months);
                data.DateEnd = _dtend;
                data.Ammortization = _ammortization;

                await Client.CreateAsync(data.Adapt<LoanCreateRequest>());

                data.ImageInBytes = string.Empty;
                _ammortization = 0;
            },
            updateFunc: async (id, data) =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.DeleteCurrentImage = true;
                    data.Image = new ImageUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.AdjustmentId}_{Guid.NewGuid():N}" };
                }

                SetDateEnd(data.PaymentSchedule, data.Amount, data.DateStart, data.Months);
                data.DateEnd = _dtend;
                data.Ammortization = _ammortization;

                await Client.UpdateAsync(id, data.Adapt<LoanUpdateRequest>());

                data.ImageInBytes = string.Empty;
                _ammortization = 0;
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);

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
        new BreadcrumbItem("Home", href: "/", icon: Icons.Material.Filled.Home),
        new BreadcrumbItem("Employees", href: "/hr/employees", icon: Icons.Material.Filled.Groups),
    };

    private void SetDateEnd(string schedule, decimal amount, DateTime? dtstart, int months)
    {
        if (dtstart != null)
        {
            _dtend = dtstart?.AddMonths(months);
        }

        if (schedule.Equals("PAYROLL"))
        {
            _ammortization = amount / (months * 2);
        }
        else
        {
            _ammortization = amount / months;
        }

        StateHasChanged();
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

public class LoanViewModel : LoanUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}