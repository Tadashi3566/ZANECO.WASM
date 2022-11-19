using Microsoft.AspNetCore.Components;

namespace ZANECO.WASM.Client.Pages.SMS;

public partial class SendMessage
{
    [Parameter]
    public string Recepients { get; set; } = string.Empty;
}