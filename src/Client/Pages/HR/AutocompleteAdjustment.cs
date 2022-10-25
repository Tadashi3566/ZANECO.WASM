﻿using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.HR;
public class AutocompleteAdjustments : MudAutocomplete<Guid>
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]
    private IAdjustmentsClient Client { get; set; } = default!;

    private List<AdjustmentDto> _adjustment = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = "Adjustment";
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.Dense;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchAdjustment;
        ToStringFunc = GetAdjustmentName;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one Employee to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _value != default
            && await ApiHelper.ExecuteCallGuardedAsync(() => Client.GetAsync(_value), Snackbar) is { } adjustment)
        {
            _adjustment.Add(adjustment);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchAdjustment(string value)
    {
        var filter = new AdjustmentSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfAdjustmentDto response)
        {
            _adjustment = response.Data.ToList();
        }

        return _adjustment.Select(x => x.Id);
    }

    private string GetAdjustmentName(Guid id) =>
        _adjustment.Find(b => b.Id == id)?.Name ?? string.Empty;
}