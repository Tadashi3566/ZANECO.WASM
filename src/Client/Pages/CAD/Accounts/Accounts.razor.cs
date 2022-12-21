using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.CAD.Accounts;

public partial class Accounts
{
    [Inject]
    protected IAccountsClient Client { get; set; } = default!;

    protected EntityServerTableContext<AccountDto, Guid, AccountViewModel> Context { get; set; } = default!;

    private EntityTable<AccountDto, Guid, AccountViewModel> _table = default!;

    private AccountMigrateRequest _accountMigrateRequest = new();

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Account",
            entityNamePlural: "Accounts",
            entityResource: FSHResource.CAD,
            fields: new()
            {
                new(data => data.AccountCode, "Account Number", "AccountNumber"),
                new(data => data.MeterSerial, "Meter Serial", "MeterSerial"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Address, "Address", "Address"),
                new(data => data.PresentReadingDate.ToString("MMM dd, yyyy"), "Reading Date", "PresentReadingDate"),
                new(data => data.BillMonth, "Bill Month", "BillMonth"),
                new(data => data.UsedKWH.ToString("N2"), "KWH", "ConsumedKWH"),
                new(data => data.BillAmount.ToString("N2"), "Bill Amount", "BillAmount"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<AccountSearchRequest>()))
                .Adapt<PaginationResponse<AccountDto>>(),
            createFunc: async data =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new FileUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Name}_{Guid.NewGuid():N}" };
                }

                await Client.CreateAsync(data.Adapt<AccountCreateRequest>());
                data.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, Account) =>
            {
                if (!string.IsNullOrEmpty(Account.ImageInBytes))
                {
                    Account.DeleteCurrentImage = true;
                    Account.Image = new FileUploadRequest() { Data = Account.ImageInBytes, Extension = Account.ImageExtension ?? string.Empty, Name = $"{Account.Name}_{Guid.NewGuid():N}" };
                }

                await Client.UpdateAsync(id, Account.Adapt<AccountUpdateRequest>());
                Account.ImageInBytes = string.Empty;
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);

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

    private async Task MigrateAccount(int idCode, string accountNumber)
    {
        _accountMigrateRequest.IndexCode = idCode;
        _accountMigrateRequest.AccountNumber = accountNumber;

        await ApiHelper.ExecuteCallGuardedAsync(() => Client.MigrateAsync(_accountMigrateRequest), Snackbar, successMessage: "Messages successfully created and sent to queue.");
    }
}

public class AccountViewModel : AccountUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}