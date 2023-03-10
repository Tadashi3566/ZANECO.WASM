using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.Personal;

public partial class Calendar
{
    //[Inject]
    //public Guid EmployeeId { get; set; } = Guid.Empty;
    //[Inject]
    //private IAppointmentsClient? Client { get; set; }

    //private AppointmentSearchRequest _request = new();

    //private List<AppointmentDto> _appointments = new();

    private DateTime _CurrentDate = DateTime.Today;


    protected override async Task OnInitializedAsync()
    {
        //if (await ApiHelper.ExecuteCallGuardedAsync(() => Client!.SearchAsync(_request), Snackbar) is ICollection<AppointmentDto> response)
        //{
        //    _appointments = response.ToList();
        //}
    }

    List<AppointmentData> _DataSource = new List<AppointmentData>
    {
        new AppointmentData { Id = 1, Subject = "Paris", StartTime = new DateTime(2020, 2, 13, 10, 0, 0) , EndTime = new DateTime(2020, 2, 13, 12, 0, 0) },
        new AppointmentData { Id = 2, Subject = "Germany", StartTime = new DateTime(2020, 2, 15, 10, 0, 0) , EndTime = new DateTime(2020, 2, 15, 12, 0, 0) }
    };

    public class AppointmentData
    {
        public int Id { get; set; }
        public string? Subject { get; set; }
        public string? Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Description { get; set; }
        public bool IsAllDay { get; set; }
        public string? RecurrenceRule { get; set; }
        public string? RecurrenceException { get; set; }
        public Nullable<int> RecurrenceID { get; set; }
    }
}
