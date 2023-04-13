using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using ZANECO.WASM.Client.Infrastructure.Auth;

namespace ZANECO.WASM.Client.Pages
{
    public partial class Index
    {
        [CascadingParameter]
        protected Task<AuthenticationState> AuthState { get; set; } = default!;

        [Inject]
        protected IAuthenticationService AuthService { get; set; } = default!;

        public IEnumerable<Claim>? Claims { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthState;
            Claims = authState.User.Claims;
        }
    }
}