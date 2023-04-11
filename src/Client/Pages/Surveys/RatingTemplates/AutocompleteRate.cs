﻿using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.Surveys.RatingTemplates;

public class AutocompleteRate : MudAutocomplete<Guid>
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private IRatesClient Client { get; set; } = default!;

    private List<RateDto> _rates = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = "Rate";
        CoerceText = true;
        CoerceValue = true;
        Clearable = true;
        Dense = true;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchText;
        ToStringFunc = GetText;
        Variant = Variant.Filled;
        return base.SetParametersAsync(parameters);
    }

    // when the value parameter is set, we have to load that one Rate to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _value != default
            && await ApiHelper.ExecuteCallGuardedAsync(() => Client.GetAsync(_value), Snackbar)
            is { } dto)
        {
            _rates.Add(dto);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchText(string value)
    {
        var filter = new RateSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfRateDto response)
        {
            _rates = response.Data.ToList();
        }

        return _rates.Select(x => x.Id);
    }

    private string GetText(Guid id) =>
        _rates.Find(b => b.Id == id)?.Name ?? string.Empty;
}