using Microsoft.AspNetCore.Components;

namespace ZANECO.WASM.Client.Pages.SMS;

public partial class SendMessage
{
    [Parameter]
    public string Recipients { get; set; } = string.Empty;
}