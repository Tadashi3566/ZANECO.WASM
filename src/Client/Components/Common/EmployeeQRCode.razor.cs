using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Infrastructure.Auth;

namespace ZANECO.WASM.Client.Components.Common
{
    public partial class EmployeeQRCode
    {
        [CascadingParameter]
        protected Task<AuthenticationState> AuthState { get; set; } = default!;

        [Inject]
        protected IAuthenticationService AuthService { get; set; } = default!;

        [Inject]
        protected IPersonalClient Client { get; set; } = default!;

        [Inject]
        private ILocalStorageService? _localStorage { get; set; }

        private UserDetailsDto? _userDto = new();
        private string? _employeeId;
        private string? _sandurotId;
        private string _name = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            if ((await AuthState).User is { } user)
            {
                _name = await _localStorage!.GetItemAsStringAsync("employeeName");
                _employeeId = await _localStorage.GetItemAsStringAsync("employeeId");
                _sandurotId = await _localStorage.GetItemAsStringAsync("sandurotId");
                StateHasChanged();
                var userDto = await Client.GetProfileAsync();
                if (userDto is not null)
                {
                    _name = $"{userDto.FirstName} {userDto.LastName}";
                    await _localStorage!.SetItemAsStringAsync("employeeName", _name);
                    if (userDto.EmployeeId is not null)
                    {
                        _employeeId = userDto.EmployeeId.ToString();
                        await _localStorage.SetItemAsStringAsync("employeeId", _employeeId);
                    }

                    if (userDto.SandurotId is not null)
                    {
                        _sandurotId = userDto.SandurotId.ToString();
                        await _localStorage.SetItemAsStringAsync("sandurotId", _sandurotId);
                    }
                }
            }

            StateHasChanged();
        }
    }
}