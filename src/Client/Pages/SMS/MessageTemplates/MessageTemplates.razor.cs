using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Components.Services;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS.MessageTemplates;

public partial class MessageTemplates
{
    [Inject]
    protected IMessageTemplatesClient Client { get; set; } = default!;

    [Inject]
    protected IMessageOutsClient MessageOut { get; set; } = default!;

    [Inject]
    private IClipboardService? ClipboardService { get; set; }

    protected EntityServerTableContext<MessageTemplateDto, Guid, MessageTemplateViewModel> Context { get; set; } = default!;

    private EntityTable<MessageTemplateDto, Guid, MessageTemplateViewModel>? _table;

    private MessageOutCreateRequest _messageOut = new();

    private ClientPreference _clientPreference = new();

    private BackgroundPreference _backgroundPreference = new();

    private DateTime _scheduleDateTime { get; set; } = DateTime.Now;

    private string? _searchString;

    protected override void OnInitialized()
    {
        Context = new(
            entityName: "Message Template",
            entityNamePlural: "Message Templates",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.ImagePath, "Image", Template: TemplateImage),
                new(data => data.TemplateType, "Type", "TemplateType"),
                new(data => data.IsAPI, "API", "IsAPI", typeof(bool), Template: TemplateApiFastMode),
                new(data => data.ScheduleDate, "Schedule", "ScheduleDate", typeof(DateOnly)),
                new(data => data.Subject, "Subject", visible : false),
                new(data => data.Message, "Message", "Message", Template: TemplateSubjectMessage),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: true,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<MessageTemplateSearchRequest>()))
                .Adapt<PaginationResponse<MessageTemplateDto>>(),
            createFunc: async data =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.Image = new ImageUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Subject}_{Guid.NewGuid():N}" };
                }

                await Client.CreateAsync(data.Adapt<MessageTemplateCreateRequest>());
                data.ImageInBytes = string.Empty;
            },
            getDuplicateFunc: entityToDuplicate =>
            {
                var newEntity = Client.Adapt<MessageTemplateViewModel>();
                return Task.FromResult(newEntity);
            },
            updateFunc: async (id, data) =>
            {
                if (!string.IsNullOrEmpty(data.ImageInBytes))
                {
                    data.DeleteCurrentImage = true;
                    data.Image = new ImageUploadRequest() { Data = data.ImageInBytes, Extension = data.ImageExtension ?? string.Empty, Name = $"{data.Subject}_{Guid.NewGuid():N}" };
                }

                await Client.UpdateAsync(id, data);
                data.ImageInBytes = string.Empty;
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
    }

    private async Task MessageTemplateCopy(string message)
    {
        await ClipboardService!.CopyToClipboard(message);

        Snackbar.Add("The Template Message was copied to Clipboard", Severity.Success);
    }

    private async Task MessageTemplateDuplicate(MessageTemplateDto dto)
    {
        string transactionContent = $"Are you sure you want to duplicate this Message Template?";
        DialogParameters parameters = new()
        {
            { nameof(TransactionConfirmation.TransactionTitle), "Duplicate Message Template" },
            { nameof(TransactionConfirmation.ContentText), transactionContent },
            { nameof(TransactionConfirmation.ConfirmText), "Duplicate" }
        };
        DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        IDialogReference dialog = DialogService.Show<TransactionConfirmation>("Duplicate", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            MessageTemplateCreateRequest newMessageTemplate = new()
            {
                TemplateType = dto.TemplateType,
                MessageType = dto.MessageType,
                IsAPI = dto.IsAPI,
                ScheduleDate = dto.ScheduleDate,
                Recipients = "0123456789",
                Subject = dto.Subject,
                Message = dto.Message,
            };

            await ApiHelper.ExecuteCallGuardedAsync(() => Client.CreateAsync(newMessageTemplate), Snackbar, successMessage: "Message Template successfully duplicated.");

            await _table!.ReloadDataAsync();
        }
    }

    private async void SendSMS(MessageTemplateDto request)
    {
        if (request.ScheduleDate < DateTime.Today)
        {
            Snackbar.Add("SMS Template Schedule should be at least _logDate!", Severity.Error);
            return;
        }

        int maxChars = 160;
        int totalChars = request.Message.Length;
        decimal sms = totalChars / maxChars;
        decimal cost = 0.33m;

        decimal totalCost = 0;

        if (totalChars % maxChars > 0)
        {
            totalCost = cost * (sms + 1);
        }
        else
        {
            totalCost = cost * sms;
        }

        int Recipients = ClassSms.GetDistinctRecipients(request.Recipients);

        string transactionContent = $"The cost for this service is P{totalCost} for each Recipient and P{totalCost * Recipients:N2} for this template. Are you sure you want to send SMS to {Recipients:N0} Recipient(s)?";
        DialogParameters parameters = new()
        {
            { nameof(TransactionConfirmation.ContentText), transactionContent },
            { nameof(TransactionConfirmation.ConfirmText), "Send" }
        };
        DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
        IDialogReference dialog = DialogService.Show<TransactionConfirmation>("Send", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled && await ClientPreferences.GetPreference() is ClientPreference clientPreference)
        {
            _backgroundPreference = clientPreference.BackgroundPreference;

            _messageOut.IsBackgroundJob = _backgroundPreference.IsBackgroundJob;
            _messageOut.IsScheduled = _backgroundPreference.IsScheduled;
            _messageOut.Schedule = request.ScheduleDate;

            _messageOut.IsAPI = request.IsAPI;
            _messageOut.MessageType = request.MessageType;
            _messageOut.MessageTo = request.Recipients;
            _messageOut.Subject = request.Subject;
            _messageOut.MessageText = request.Message;
            _messageOut.Description = request.Subject;

            if (_messageOut.IsBackgroundJob)
            {
                Snackbar.Add("SMS are being created and sent to Background Job Worker. The same SMS will be then sent on the Day of the schedule.", Severity.Info);
            }
            else
            {
                Snackbar.Add("SMS are being created and sent directly to Recipients.", Severity.Info);
            }

            if (await ApiHelper.ExecuteCallGuardedAsync(() => MessageOut.CreateAsync(_messageOut), Snackbar) > 0)
            {
                MessageTemplateSendRequest sendRequest = new();
                sendRequest.Id = request.Id;

                await ApiHelper.ExecuteCallGuardedAsync(() => Client.SentAsync(sendRequest),
                    Snackbar,
                    successMessage: "SMS successfully sent.");

                await _table!.ReloadDataAsync();
            }
        }
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

public class MessageTemplateViewModel : MessageTemplateUpdateRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }
}