using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS.MessageIns;
public partial class MessageIns
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IMessageInsClient Client { get; set; } = default!;

    protected EntityServerTableContext<MessageInDto, int, MessageInUpdateRequest> Context { get; set; } = default!;

    private EntityTable<MessageInDto, int, MessageInUpdateRequest> _table = default!;

    private MessageInReadRequest _readRequest = new();

    private bool _canEditSMS;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canEditSMS = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.SMS);

        Context = new(
            entityName: "Inbox Message",
            entityNamePlural: "Inbox Messages",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.ReceiveTime, "Date/Time", "ReadOn", Template: TemplateReceivedTime),
                new(data => data.MessageFrom, "Sender/Receiver", "MessageFrom", Template: TemplateSenderReceiver),
                new(data => data.MessageText, "Message", "MessageText"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: true,
            hasExtraActionsFunc: () => _canEditSMS,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<MessageInSearchRequest>()))
                .Adapt<PaginationResponse<MessageInDto>>(),
            updateFunc: async (id, data) => await Client.UpdateAsync(id, data),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
    }

    private async Task ReadInbox(string messageFrom)
    {
        _readRequest.MessageFrom = messageFrom;

        await ApiHelper.ExecuteCallGuardedAsync(() => Client.ReadAsync(_readRequest), Snackbar, successMessage: "Messages from sender has been marked as read.");

        await _table.ReloadDataAsync();
    }
}