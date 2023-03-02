﻿using Mapster;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Components.Dialogs;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Components.Services;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
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

    protected EntityServerTableContext<MessageTemplateDetail, Guid, MessageTemplateUpdateRequest> Context { get; set; } = default!;

    private EntityTable<MessageTemplateDetail, Guid, MessageTemplateUpdateRequest>? _table;

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
                new(data => data.TemplateType, "Type", "TemplateType"),
                new(data => data.IsAPI, "API", "IsAPI", typeof(bool), Template: TemplateApiFastMode),
                new(data => data.ScheduleDate, "Schedule", "ScheduleDate", typeof(DateOnly)),
                new(data => data.Subject, "Subject", "Subject", visible: false),
                new(data => data.Message, "Message", "Message", Template: TemplateSubjectMessage),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: true,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<MessageTemplateSearchRequest>()))
                .Adapt<PaginationResponse<MessageTemplateDetail>>(),
            createFunc: async data => await Client.CreateAsync(data.Adapt<MessageTemplateCreateRequest>()),
            getDuplicateFunc: entityToDuplicate =>
            {
                var newEntity = Client.Adapt<MessageTemplateUpdateRequest>();
                return Task.FromResult(newEntity);
            },
            updateFunc: async (id, data) => await Client.UpdateAsync(id, data),
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
                Recepients = "0123456789",
                Subject = dto.Subject,
                Message = dto.Message,
                Description = dto.Description!,
                Notes = dto.Notes!,
            };

            await ApiHelper.ExecuteCallGuardedAsync(() => Client.CreateAsync(newMessageTemplate),
                Snackbar,
                successMessage: "Message Template successfully duplicated.");

            await _table!.ReloadDataAsync();
        }
    }

    private async void SendSMS(MessageTemplateDetail request)
    {
        if (request.ScheduleDate < DateTime.Today)
        {
            Snackbar.Add("SMS Template Schedule should be at least _logDate!", Severity.Error);
            return;
        }

        //int dividend = 540;
        //int divisor = 160;
        //int quotient = dividend / divisor; // integer division
        //int remainder = dividend % divisor; // modulo operator

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

        int recepients = ClassSms.GetDistinctRecepients(request.Recepients);

        string transactionContent = $"The cost for this service is P{totalCost} for each recepient and P{totalCost * recepients:N2} for this template. Are you sure you want to send SMS to {recepients:N0} recepient(s)?";
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
            _messageOut.MessageTo = request.Recepients;
            _messageOut.Subject = request.Subject;
            _messageOut.MessageText = request.Message;
            _messageOut.Description = request.Subject;

            if (_messageOut.IsBackgroundJob)
            {
                Snackbar.Add("SMS are being created and sent to Background Job Worker. The same SMS will be then sent on the Day of the schedule.", Severity.Info);
            }
            else
            {
                Snackbar.Add("SMS are being created and sent directly to recepients.", Severity.Info);
            }

            if (await ApiHelper.ExecuteCallGuardedAsync(() => MessageOut.CreateAsync(_messageOut),
                Snackbar
                ) > 0)
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

    public class MessageTemplateDetail : MessageTemplateDto
    {
        public bool ShowRecepients { get; set; }
    }
}