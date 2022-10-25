using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS.Contacts;
public partial class ContactList
{
    [Parameter]
    public string? Id { get; set; }
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IContactsClient ContactsClient { get; set; } = default!;

    private PaginationFilter _filter = new();

    // private IEnumerable<ContactDto> _contacts = default!;

    private PaginationResponse<ContactDto> context = default!;
    private ContactCreateRequest _contactCreateRequest = new();
    private ContactUpdateRequest _contactUpdateRequest = new();

    private CustomValidation? _customValidation;

    private string _searchString = string.Empty;

    private bool _canSearhContacts;
    private bool _canEditContacts;

    private bool _loaded;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canSearhContacts = await AuthService.HasPermissionAsync(state.User, FSHAction.View, FSHResource.Contacts);
        _canEditContacts = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.Contacts);

        var filter = _filter.Adapt<ContactSearchRequest>();

        var result = await ContactsClient.SearchAsync(filter);

        context = result.Adapt<PaginationResponse<ContactDto>>();

        _loaded = true;
    }

    private async Task SaveAsync()
    {
        var id = Guid.Empty;
        if (id == Guid.Empty)
        {
            await ApiHelper.ExecuteCallGuardedAsync(() => ContactsClient.CreateAsync(_contactCreateRequest), Snackbar, _customValidation, "Contact Profile has been successfully created.");
        }
        else
        {
            await ApiHelper.ExecuteCallGuardedAsync(() => ContactsClient.UpdateAsync(id, _contactUpdateRequest), Snackbar, _customValidation, "Contact Profile has been successfully updated.");
        }
    }

    private bool Search(ContactDto contact)
    {
        return string.IsNullOrWhiteSpace(_searchString)
            || contact.PhoneNumber?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) is true
            || contact.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) is true
            || contact.Address?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) is true
            || contact.Description?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) is true
            || contact.Notes?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) is true;
    }
}