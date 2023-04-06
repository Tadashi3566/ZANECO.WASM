using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Infrastructure.Preferences;
using ZANECO.WASM.Client.Shared;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.Identity.Users;

public partial class UserProfile
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IUsersClient Client { get; set; } = default!;

    [Parameter]
    public string? Id { get; set; }
    [Parameter]
    public string? Title { get; set; }
    [Parameter]
    public string? Email { get; set; }

    private Guid _employeeId { get; set; }
    private string? _sandurotId { get; set; }
    private string? _accountNumber { get; set; }
    private bool _active;
    private bool _emailConfirmed;
    private char _firstLetterOfName;
    private string? _firstName;
    private string? _lastName;
    private string? _phoneNumber;
    private string? _description;
    private string? _notes;
    private string? _email;
    private string? _imageUrl;
    private bool _loaded;
    private bool _canToggleUserStatus;

    private ClientPreference _preference = new();

    private async Task ToggleUserStatus()
    {
        var request = new UserStatusRequest
        {
            UserId = Id,
            EmployeeId = _employeeId!,
            SandurotId = _sandurotId!,
            AccountNumber = _accountNumber,
            EmailConfirmed = _emailConfirmed,
            ActivateUser = _active,
        };
        await ApiHelper.ExecuteCallGuardedAsync(() => Client.ToggleStatusAsync(Id, request), Snackbar);
        Navigation.NavigateTo("/users");
    }

    [Parameter]
    public string? ImageUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(() => Client.GetByIdAsync(Id), Snackbar) is UserDetailsDto userDto)
        {
            _firstName = userDto.FirstName;
            _lastName = userDto.LastName;
            _email = userDto.Email;
            _phoneNumber = userDto.PhoneNumber;
            _description = userDto.Description;
            _notes = userDto.Notes;
            _active = userDto.IsActive;
            _emailConfirmed = userDto.EmailConfirmed;
            _imageUrl = string.IsNullOrEmpty(userDto.ImageUrl) ? string.Empty : (Config[ConfigNames.ApiBaseUrl] + userDto.ImageUrl);
            Title = $"{_firstName} {_lastName}'s {_localizer["Profile"]}";
            Email = _email;
            if (_firstName?.Length > 0)
            {
                _firstLetterOfName = _firstName.ToUpper().FirstOrDefault();
            }

            if (userDto.EmployeeId is not null)
            {
                _employeeId = (Guid)userDto.EmployeeId!;
            }

            if (userDto.SandurotId is not null)
            {
                _sandurotId = userDto.SandurotId!;
            }

            if (userDto.AccountNumber is not null)
            {
                _accountNumber = userDto.AccountNumber!;
            }
        }

        var state = await AuthState;
        _canToggleUserStatus = await AuthService.HasPermissionAsync(state.User, FSHAction.Update, FSHResource.Users);
        _loaded = true;
    }
}