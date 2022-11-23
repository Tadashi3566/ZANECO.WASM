using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
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

    private readonly MessageInUpdateRequest _readRequest = new();

    private bool _canCreateSMS;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canCreateSMS = await AuthService.HasPermissionAsync(state.User, FSHAction.Create, FSHResource.SMS);

        Context = new(
            entityName: "Inbox Message",
            entityNamePlural: "Inbox Messages",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.Id, "Id", "Id"),
                new(data => data.ReceiveTime, "Receive Date/Time", "ReceiveTime", Template: TemplateReceivedTime),
                new(data => data.MessageFrom, "Sender/Receiver", "MessageFrom", Template: TemplateSenderReceiver),
                new(data => data.MessageText, "Message", "MessageText"),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
            },
            enableAdvancedSearch: true,
            hasExtraActionsFunc: () => _canCreateSMS,
            idFunc: data => data.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<MessageInSearchRequest>()))
                .Adapt<PaginationResponse<MessageInDto>>(),
            updateFunc: async (id, data) => await Client.UpdateAsync(id, data),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
    }

    private async Task ReadMessages(int id, string phoneNumber)
    {
        _readRequest.Id = id;
        _readRequest.MessageFrom = phoneNumber;

        if (await ApiHelper.ExecuteCallGuardedAsync(() => Client.UpdateAsync(id, _readRequest), Snackbar, successMessage: $"Messages from {phoneNumber} has been read.") > 0)
        {
            Navigation.NavigateTo($"/sms/messages/{phoneNumber}");
        }

        //await Client.UpdateAsync(id, _readRequest);
        //Navigation.NavigateTo($"/sms/messages/{phoneNumber}");
        //Snackbar.Add($"Messages from {phoneNumber} has been read.", Severity.Normal);
    }
}
