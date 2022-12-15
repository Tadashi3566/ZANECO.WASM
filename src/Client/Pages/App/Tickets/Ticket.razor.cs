using Microsoft.AspNetCore.Components;

namespace ZANECO.WASM.Client.Pages.App.Tickets;

public partial class Ticket
{
    [Parameter]
    public Guid Id { get; set; }
}