using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.Common;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.App.Tickets;

public partial class TicketComponent
{
    [Parameter]
    public Guid Id { get; set; }
    [Inject]
    protected ITicketsClient TicketClient { get; set; } = default!;

    private readonly TicketUpdateRequest _ticket = new();

    private TicketDetailsDto _ticketDto = new();

    private CustomValidation? _customValidation;

    //protected override void OnParametersSet()
    //{
    //    if (Id != Guid.Empty)
    //    {
    //        _context.Id = Id;
    //    }
    //}

    protected override async Task OnInitializedAsync()
    {
        if (Id != Guid.Empty)
        {
            _ticketDto = await TicketClient.GetAsync(Id);

            _ticket.Id = Id;
            _ticket.Impact = _ticketDto.Impact;
            _ticket.Urgency = _ticketDto.Urgency;
            _ticket.Priority = _ticketDto.Priority;
            _ticket.Name = _ticketDto.Name;
            _ticket.Description = _ticketDto.Description;
            _ticket.Notes = _ticketDto.Notes;
            _ticket.RequestedBy = _ticketDto.RequestedBy;
            _ticket.RequestThrough = _ticketDto.RequestThrough;
            _ticket.Reference = _ticketDto.Reference;
            _ticket.Status = _ticketDto.Status;
        }
    }

    private async Task OnSubmit() =>
        await ApiHelper.ExecuteCallGuardedAsync((Func<Task<Guid>>)(() =>
        TicketClient.UpdateAsync(this.Id, _ticket)), Snackbar, _customValidation, "The Ticket has been successfully updated.");
}