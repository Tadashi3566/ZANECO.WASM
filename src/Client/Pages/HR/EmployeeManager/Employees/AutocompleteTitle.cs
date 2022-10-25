﻿using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.HR.EmployeeManager.Employees;

public class AutocompleteTitle : MudAutocomplete<string>
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]
    private IEmployeesClient Client { get; set; } = default!;

    private List<EmployeeDto> _list = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        CoerceText = true;
        CoerceValue = true;
        Label = "Title";
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.Dense;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchText;
        Clearable = true;
        return base.SetParametersAsync(parameters);
    }

    private async Task<IEnumerable<string>> SearchText(string value)
    {
        var filter = new EmployeeSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "title" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfEmployeeDto response)
        {
            _list = response.Data.ToList();
        }

        return _list
            .Where(x => x.Title != string.Empty)
            .Select(x => x.Title).Distinct();
    }
}