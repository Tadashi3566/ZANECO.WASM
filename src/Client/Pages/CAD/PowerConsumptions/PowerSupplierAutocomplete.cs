using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.CAD;

public class PowerSupplierAutocomplete : MudAutocomplete<Guid>
{
    [Inject]
    private IStringLocalizer<PowerSupplierAutocomplete> L { get; set; } = default!;
    [Inject]
    private IPowerConsumptionsClient PowerConsumptionsClient { get; set; } = default!;
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<PowerConsumptionDto> _powerConsumptions = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Clearable = true;
        Dense = true;
        Margin = Margin.Dense;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchPowerConsumptions;
        ToStringFunc = GetPowerConsumptionName;
        Variant = Variant.Filled;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one PowerConsumption to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender &&
            _value != default &&
            await ApiHelper.ExecuteCallGuardedAsync(
                () => PowerConsumptionsClient.GetAsync(_value), Snackbar) is { } powerConsumption)
        {
            _powerConsumptions.Add(powerConsumption);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchPowerConsumptions(string value)
    {
        var filter = new PowerConsumptionSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => PowerConsumptionsClient.SearchAsync(filter), Snackbar)
            is PaginationResponseOfPowerConsumptionDto response)
        {
            _powerConsumptions = response.Data.ToList();
        }

        return _powerConsumptions.Select(x => x.Id);
    }

    private string GetPowerConsumptionName(Guid id) =>
        _powerConsumptions.Find(b => b.Id == id)?.GroupName ?? string.Empty;
}