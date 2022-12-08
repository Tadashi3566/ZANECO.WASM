using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS;
public partial class AGMARegistration
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

    private bool _canCreateSMS;
    private bool _autoRead;
    private int _seconds = 20;

    private Timer? _timer;

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
                //new(data => data.Id, "Id", "Id"),
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

        _timer = new System.Threading.Timer(async _ =>  // async void
        {
            await CheckAGMARegistration();

            // we need StateHasChanged() because this is an async void handler
            // we need to Invoke it because we could be on the wrong Thread          
            await InvokeAsync(StateHasChanged);
        }, null, 0, _seconds * 1000);
    }

    public void Dispose() => _timer?.Dispose();

    private async Task CheckAGMARegistration(bool forceRead = false)
    {
        if (_autoRead || forceRead)
        {
            //_timer.Change(0, _seconds * 1000);

            _readRequest.MessageFrom = "";
            _readRequest.IsAgma = true;

            await ApiHelper.ExecuteCallGuardedAsync(() => Client.ReadAsync(_readRequest), Snackbar, successMessage: $"Unread Messages were checked for AGMA Registration");

            await _table.ReloadDataAsync();
        }
    }
}