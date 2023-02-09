using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.ISD.HR.EmployeeManager.Employers;

public class AutocompletePreviousposition : MudAutocomplete<string>
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]
    private IEmployersClient Client { get; set; } = default!;

    private List<EmployerDto> _list = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = "Designation";
        CoerceText = true;
        CoerceValue = true;
        Clearable = true;
        Dense = true;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchText;
        Variant = Variant.Filled; return base.SetParametersAsync(parameters);
    }

    private async Task<IEnumerable<string>> SearchText(string value)
    {
        var filter = new EmployerSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "designation" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfEmployerDto response)
        {
            _list = response.Data.ToList();
        }

        return _list
            .Where(x => x.Designation != string.Empty)
            .Select(x => x.Designation).Distinct();
    }
}