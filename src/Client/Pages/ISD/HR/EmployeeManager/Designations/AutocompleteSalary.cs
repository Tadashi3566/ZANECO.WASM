using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Designations;

public class AutocompleteSalary : MudAutocomplete<int>
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]
    private ISalarysClient Client { get; set; } = default!;

    private List<SalaryDto> _groups = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = "Salary";
        CoerceText = true;
        CoerceValue = true;
        Clearable = true;
        Dense = true;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchText;
        Variant = Variant.Filled;
        return base.SetParametersAsync(parameters);
    }

    private async Task<IEnumerable<int>> SearchText(string value)
    {
        var filter = new SalarySearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "number" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfSalaryDto response)
        {
            _groups = response.Data
                .Where(x => x.IsActive == true)
                .ToList();
        }

        return _groups
            .Select(x => x.Number);
    }
}