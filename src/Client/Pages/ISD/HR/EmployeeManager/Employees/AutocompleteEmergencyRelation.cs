﻿using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Employees;

public class AutocompleteEmergencyRelation : MudAutocomplete<string>
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private IEmployeesClient Client { get; set; } = default!;

    private List<EmployeeDto> _list = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = "Emergency Relation";
        CoerceText = true;
        CoerceValue = true;
        Clearable = true;
        Dense = true;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchText;
        Variant = Variant.Filled;
        return base.SetParametersAsync(parameters);
    }

    private async Task<IEnumerable<string>> SearchText(string value)
    {
        var filter = new EmployeeSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "emergencyrelation" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfEmployeeDto response)
        {
            _list = response.Data.ToList();
        }

        return _list
            .Where(x => x.EmergencyRelation != string.Empty)
            .Select(x => x.EmergencyRelation).Distinct();
    }
}