using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.CAD;
public class AutocompleteMunicipality : MudAutocomplete<Guid>
{
    [Inject]
    private IAreasClient AreasClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<AreaDto> _areas = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = "Area";
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.Dense;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchAreas;
        ToStringFunc = GetAreaName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one Area to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => AreasClient.GetAsync(_value), Snackbar) is { } area)
        {
            _areas.Add(area);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchAreas(string value)
    {
        var filter = new AreaSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name", "description", "notes" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => AreasClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfAreaDto response)
        {
            _areas = response.Data.ToList();
        }

        return _areas.Select(x => x.Id);
    }

    private string GetAreaName(Guid id) =>
        _areas.Find(b => b.Id == id)?.Name ?? string.Empty;
}