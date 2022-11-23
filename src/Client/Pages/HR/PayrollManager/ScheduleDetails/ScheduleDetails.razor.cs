using Mapster;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.HR.PayrollManager.ScheduleDetails;
public partial class ScheduleDetails
{
    [Parameter]
    public Guid ScheduleId { get; set; } = Guid.Empty;
    [Inject]
    protected IScheduleDetailsClient Client { get; set; } = default!;

    protected EntityServerTableContext<ScheduleDetailDto, Guid, ScheduleDetailUpdateRequest> Context { get; set; } = default!;

    private EntityTable<ScheduleDetailDto, Guid, ScheduleDetailUpdateRequest> _table = default!;

    public IMask TimeSpanMask = new RegexMask("^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$");

    protected override void OnParametersSet()
    {
        if (ScheduleId != Guid.Empty)
        {
            _searchScheduleId = ScheduleId;
        }
    }

    protected override async Task OnInitializedAsync() =>
        Context = new(
            entityName: "Schedule Detail",
            entityNamePlural: "Schedule Details",
            entityResource: FSHResource.ScheduleDetails,
            fields: new()
            {
                new(data => data.ScheduleName, "Schedule", "ScheduleName"),
                new(data => data.ScheduleType, "Type", "ScheduleType"),
                new(data => data.Day, "Day", "Day"),
                new(data => data.TimeIn1, "Time-In 1", "TimeIn1"),
                new(data => data.TimeOut1, "Time-Out 1", "TimeOut1"),
                new(data => data.TimeIn2, "Time-In 2", "TimeIn2"),
                new(data => data.TimeOut2, "Time-Out 2", "TimeOut2"),
                new(data => data.TimeIn3, "Time-In 3", "TimeIn3"),
                new(data => data.TimeOut3, "Time-Out 3", "TimeOut3"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var filter = _filter.Adapt<ScheduleDetailSearchRequest>();

                filter.ScheduleId = SearchScheduleId == default ? null : SearchScheduleId;

                var result = await Client.SearchAsync(filter);
                return result.Adapt<PaginationResponse<ScheduleDetailDto>>();
            },
            createFunc: async data =>
            {
                data.ScheduleId = SearchScheduleId;
                await Client.CreateAsync(data.Adapt<ScheduleDetailCreateRequest>());
            },
            updateFunc: async (id, data) => await Client.UpdateAsync(id, data.Adapt<ScheduleDetailUpdateRequest>()),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);

    // Advanced Search
    private Guid _searchScheduleId;
    private Guid SearchScheduleId
    {
        get => _searchScheduleId;
        set
        {
            _searchScheduleId = value;
            _ = _table.ReloadDataAsync();
        }
    }
}