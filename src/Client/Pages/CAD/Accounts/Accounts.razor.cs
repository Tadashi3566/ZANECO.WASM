using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.CAD.Accounts;

public partial class Accounts
{
    [Inject]
    protected IAccountsClient Client { get; set; } = default!;

    protected EntityServerTableContext<AccountDto, Guid, AccountViewModel> Context { get; set; } = default!;

    private EntityTable<AccountDto, Guid, AccountViewModel>? _table;

    private AccountMigrateAccountRequest _accountMigrateAccountRequest = new();

    private AccountMigrateLedgerRequest _accountMigrateLedgerRequest = new();

    private BackgroundPreference _preference = new();

    private string? _searchString;

    private HashSet<AccountDto> _selectedItems = new();

    private int[] _pageSizes = new int[] { 10, 15, 50, 100, 500, 1000, 5000, 10000, 50000, 100000 };

    protected override void OnInitialized()
    {
        Context = new(
            entityName: "Account",
            entityNamePlural: "Accounts",
            entityResource: FSHResource.CAD,
            fields: new()
            {
                new(data => data.AccountNumber, "Barcode", "AccountNumber", Template: TemplateBarcode),
                new(data => data.Name, "Name", "Name", Template: TemplateNameAddress),
                new(data => data.PresentReadingDate, "Reading Dates", "PresentReadingDate", Template: TemplateBillingDate),
                new(data => data.BillMonth, "Bill Month", "BillMonth"),
                new(data => data.UsedKWH, "KWH", "UsedKWH", typeof(double)),
                new(data => data.BillAmount, "Bill Amount", "BillAmount", typeof(decimal)),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: true,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<AccountSearchRequest>()))
                .Adapt<PaginationResponse<AccountDto>>(),
            createFunc: async data =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new ImageUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Name}_{Guid.NewGuid():N}" };
                }

                await Client.CreateAsync(data.Adapt<AccountCreateRequest>());
                data.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, Account) =>
            {
                if (!string.IsNullOrEmpty(Account.ImageInBytes))
                {
                    Account.DeleteCurrentImage = true;
                    Account.Image = new ImageUploadRequest() { Data = Account.ImageInBytes, Extension = Account.ImageExtension ?? string.Empty, Name = $"{Account.Name}_{Guid.NewGuid():N}" };
                }

                await Client.UpdateAsync(id, Account.Adapt<AccountUpdateRequest>());
                Account.ImageInBytes = string.Empty;
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
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

    private async Task MigrateAccount(string application)
    {
        string actionTitle = application.Equals("ACCOUNT") ? "Migrate Accounts" : "Migrate Ledger";
        string transactionTitle = actionTitle;
        string transactionContent = $"Are you sure you want to {actionTitle}";
        var parameters = new DialogParameters
        {
            { nameof(TransactionConfirmation.TransactionIcon), Icons.Material.Filled.Send },
            { nameof(TransactionConfirmation.TransactionTitle), transactionTitle },
            { nameof(TransactionConfirmation.ContentText), transactionContent },
            { nameof(TransactionConfirmation.ConfirmText), "Migrate" }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        var dialog = DialogService.Show<TransactionConfirmation>(transactionTitle, parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            if (await ClientPreferences.GetPreference() is ClientPreference clientPreference)
            {
                _preference = clientPreference.BackgroundPreference;

                _accountMigrateAccountRequest.IsBackgroundJob = _preference.IsBackgroundJob;
                _accountMigrateAccountRequest.IsScheduled = _preference.IsScheduled;

                _accountMigrateLedgerRequest.IsBackgroundJob = _preference.IsBackgroundJob;
                _accountMigrateLedgerRequest.IsScheduled = _preference.IsScheduled;
            }

            if (application.Equals("ACCOUNT"))
            {
                await ApiHelper.ExecuteCallGuardedAsync(() => Client.MigrateAccountAsync(_accountMigrateAccountRequest), Snackbar, successMessage: "Migration has been successfully sent to Background Job Worker.");
            }
            else if (application.Equals("LEDGER"))
            {
                string[] accountNumbers = _selectedItems.Select(x => x.AccountNumber).ToArray()!;

                if (accountNumbers.Length > 0)
                {
                    foreach (string accountNumber in accountNumbers)
                    {
                        _accountMigrateLedgerRequest.AccountNumber = accountNumber;

                        await ApiHelper.ExecuteCallGuardedAsync(() => Client.MigrateLedgerAsync(_accountMigrateLedgerRequest), Snackbar, successMessage: "Migration has been successfully sent to Background Job Worker.");
                    }
                }
            }
        }
    }
}

public class AccountViewModel : AccountUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}