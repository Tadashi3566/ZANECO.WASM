using Microsoft.AspNetCore.Authorization;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Infrastructure.Auth;
public class MustHavePermissionAttribute : AuthorizeAttribute
{
    public MustHavePermissionAttribute(string action, string resource) =>
        Policy = FSHPermission.NameFor(action, resource);
}