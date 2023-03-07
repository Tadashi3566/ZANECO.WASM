using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.ISD.HR.PayrollManager.Calendars;

public partial class Calendars
{
    //[Parameter]
    //public Guid EmployeeId { get; set; } = Guid.Empty;
    [Inject]
    protected ICalendarClient Client { get; set; } = default!;
    protected EntityServerTableContext<CalendarDto, Guid, CalendarUpdateRequest> Context { get; set; } = default!;

    private EntityTable<CalendarDto, Guid, CalendarUpdateRequest>? _table;

    private string? _searchString;

    //protected override void OnParametersSet()
    //{
    //    if (EmployeeId != Guid.Empty)
    //    {
    //        _searchEmployeeId = EmployeeId;
    //    }
    //}

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Calendar",
            entityNamePlural: "Calendar",
            entityResource: FSHResource.Payroll,
            fields: new()
            {
                new(data => data.CalendarDate, "Date", "CalendarDate", Template: TemplateCalendarDateType),
                new(data => data.CalendarType, "Calendar Type", visible: false),
                new(data => data.Name, "Name", "Name", Template: TemplateNameDay),
                new(data => data.Description, "Description/Notes", "Description", Template: TemplateDescriptionNotes),
                new(data => data.Notes, "Notes", visible: false),
            },
            idFunc: Calendar => Calendar.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<CalendarSearchRequest>();

                //filter.EmployeeId = SearchEmployeeId == default ? null : SearchEmployeeId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<CalendarDto>>();
            },
            createFunc: async data =>
            {
                //data.EmployeeId = SearchEmployeeId;

                await Client.CreateAsync(data.Adapt<CalendarCreateRequest>());
            },
            updateFunc: async (id, data) =>
            {
                //data.EmployeeId = SearchEmployeeId;

                await Client.UpdateAsync(id, data.Adapt<CalendarUpdateRequest>());
            },
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);

    // Advanced Search
    //private Guid _searchEmployeeId;
    //private Guid SearchEmployeeId
    //{
    //    get => _searchEmployeeId;
    //    set
    //    {
    //        _searchEmployeeId = value;
    //        _ = _table!.ReloadDataAsync();
    //    }
    //}
}