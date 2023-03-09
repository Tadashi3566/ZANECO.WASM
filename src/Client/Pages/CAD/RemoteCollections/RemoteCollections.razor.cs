using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Text.RegularExpressions;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.CAD.RemoteCollections;

public partial class RemoteCollections
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IRemoteCollectionsClient Client { get; set; } = default!;

    protected EntityServerTableContext<RemoteCollectionDto, Guid, RemoteCollectionViewModel> Context { get; set; } = default!;

    private EntityTable<RemoteCollectionDto, Guid, RemoteCollectionViewModel>? _table;

    private HashSet<RemoteCollectionDto> _selectedItems = new();

    private string? _searchString;
    private bool _hasCreateAction;
    private List<string[]>? _remoteCollections;

    protected override async void OnInitialized()
    {
        var state = await AuthState;
        _hasCreateAction = await AuthService.HasPermissionAsync(state.User, FSHAction.Create, FSHResource.CAD);

        Context = new(
            entityName: "Remote Collection",
            entityNamePlural: "Remote Collections",
            entityResource: FSHResource.CAD,
            fields: new()
            {
                new(data => data.CollectorId, "Collector Id", visible: false),
                new(data => data.Collector, "Collector", "Collector", Template: TemplateCollector),
                new(data => data.Reference, "Reference", visible: false),
                new(data => data.AccountNumber, "Reference/Account", "AccountNumber", Template: TemplateReferenceAccount),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Amount, "Amount", "Amount", typeof(decimal)),
                new(data => data.TransactionDate, "Transaction Date", "TransactionDate", typeof(DateTime)),
                new(data => data.ReportDate, "Report Date", "ReportDate", typeof(DateOnly)),
                new(data => data.OrNumber, "Official Receipt", "OrNumber"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            enableAdvancedSearch: true,
            hasExtraActionsFunc: () => _hasCreateAction,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<RemoteCollectionSearchRequest>()))
                .Adapt<PaginationResponse<RemoteCollectionDto>>(),
            createFunc: async data =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new ImageUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Name}_{Guid.NewGuid():N}" };
                }

                await Client.CreateAsync(data.Adapt<RemoteCollectionCreateRequest>());
                data.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, data) =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.DeleteCurrentImage = true;
                    data.Image = new ImageUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Name}_{Guid.NewGuid():N}" };
                }

                await Client.UpdateAsync(id, data.Adapt<RemoteCollectionUpdateRequest>());
                data.ImageInBytes = string.Empty;
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
    }

    private async Task Send()
    {
        RemoteCollectionSMSRequest request = new();

        string transactionContent = $"Are you sure you want to Send SMS Confirmation(s)?";
        DialogParameters parameters = new()
            {
                { nameof(TransactionConfirmation.TransactionTitle), "Send SMS Confirmation(s)" },
                { nameof(TransactionConfirmation.ContentText), transactionContent },
                { nameof(TransactionConfirmation.ConfirmText), "Send" }
            };
        DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        IDialogReference dialog = DialogService.Show<TransactionConfirmation>("Send", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            foreach (var item in _selectedItems)
            {
                request.Collector = item.Collector;
                request.Reference = item.Reference;
                request.TransactionDate = item.TransactionDate;
                request.AccountNumber = item.AccountNumber;
                request.Amount = item.Amount;

                await ApiHelper.ExecuteCallGuardedAsync(() => Client.SendSmsAsync(request), Snackbar, successMessage: $"SMS Payment Confirmations were successfully sent to the selected Remote Collections.");
            }
        }
    }

    private async Task OnInputFileChange(InputFileChangeEventArgs e)
    {
        var singleFile = e.File;

        Regex regex = new Regex(".+\\.csv", RegexOptions.Compiled);
        if (!regex.IsMatch(singleFile.Name))
        {
            //show error invalidad format file
            return;
        }

        var stream = singleFile.OpenReadStream();
        List<string[]> csv = new();
        MemoryStream ms = new();
        await stream.CopyToAsync(ms);
        stream.Close();
        string outputFileString = System.Text.Encoding.UTF8.GetString(ms.ToArray());

        foreach (string item in outputFileString.Split(Environment.NewLine))
        {
            string[] remoteCollection = SplitCSV(item.ToString());

            if (remoteCollection is not null)
            {
                csv.Add(SplitCSV(item.ToString()));
            }
        }

        if (csv is not null)
        {
            _remoteCollections = csv;
        }
    }

    private async Task CreateRemoteCollections(List<string[]>? remoteCollections)
    {
        RemoteCollectionCreateRequest request = new();

        if (remoteCollections is not null)
        {
            string transactionContent = $"There are {remoteCollections.Count} payments in this file. Are you sure you want to import the collections?";
            DialogParameters parameters = new()
            {
                { nameof(TransactionConfirmation.TransactionTitle), "Import Remote Collections" },
                { nameof(TransactionConfirmation.ContentText), transactionContent },
                { nameof(TransactionConfirmation.ConfirmText), "Import" }
            };
            DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            IDialogReference dialog = DialogService.Show<TransactionConfirmation>("Import", parameters, options);
            DialogResult result = await dialog.Result;
            if (!result.Canceled)
            {
                foreach (string[] remoteCollection in remoteCollections)
                {
                    if (remoteCollection is not null)
                    {
                        try
                        {
                            request.CollectorId = Convert.ToDouble(remoteCollection[1]);
                            request.Collector = remoteCollection[2];
                            request.Reference = remoteCollection[5];
                            request.AccountNumber = remoteCollection[7];
                            request.Date = remoteCollection[3];
                            request.Time = remoteCollection[4];
                            request.ReportDate = Convert.ToDateTime(remoteCollection[10]);
                            request.Name = remoteCollection[8];

                            string inputString = remoteCollection[9];
                            string decimalPattern = @"(\d+\.\d+)";
                            Match match = Regex.Match(inputString, decimalPattern);

                            if (match.Success)
                            {
                                string decimalString = match.Groups[1].Value;
                                request.Amount = decimal.Parse(decimalString);
                            }

                            await ApiHelper.ExecuteCallGuardedAsync(() => Client.CreateAsync(request), Snackbar, successMessage: $"Remote Collection with Account {request.AccountNumber} was imported.");
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }

                await _table!.ReloadDataAsync();
            }
        }
    }

    private static string[] SplitCSV(string input)
    {
        //Excludes commas within quotes  
        Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);
        List<string> list = new List<string>();
        string? curr = null;
        foreach (Match match in csvSplit.Matches(input))
        {
            curr = match.Value;
            if (0 == curr.Length) list.Add("");

            list.Add(curr.TrimStart(','));
        }

        return list.ToArray();
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

public class RemoteCollectionViewModel : RemoteCollectionUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}