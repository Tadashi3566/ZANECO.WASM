﻿using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.CAD.PowerConsumptions;
public class AutocompletePowerSupplier : MudAutocomplete<string>
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]
    private IGroupsClient Client { get; set; } = default!;

    private List<GroupDto> _groups = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Clearable = true;
        Dense = true;
        Margin = Margin.Dense;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchText;
        Variant = Variant.Filled;
        return base.SetParametersAsync(parameters);
    }

    private async Task<IEnumerable<string>> SearchText(string value)
    {
        var filter = new GroupSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfGroupDto response)
        {
            _groups = response.Data
                .Where(x => x.Parent.Equals("POWERSUPPLIER"))
                .OrderBy(x => x.Name)
                .ToList();
        }

        return _groups
            .Select(x => x.Name);
    }
}