using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.HR;
public class AutocompleteEmployee : MudAutocomplete<Guid>
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]
    private IEmployeesClient Client { get; set; } = default!;

    private List<EmployeeDto> _employees = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = "Employee";
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

    // when the value parameter is set, we have to load that one Employee to be able to show the name
    // we can't do that in OnInitialized because of a strange bug (https://github.com/MudBlazor/MudBlazor/issues/3818)
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _value != default
            && await ApiHelper.ExecuteCallGuardedAsync(() => Client.GetAsync(_value), Snackbar) is { } employee)
        {
            _employees.Add(employee);
            ForceRender(true);
        }
    }

    private async Task<IEnumerable<Guid>> SearchText(string value)
    {
        var filter = new EmployeeSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "lastname", "firstname", "middlename" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfEmployeeDto response)
        {
            _employees = response.Data.ToList();
        }

        return _employees
            .Select(x => x.Id);
    }

    private string GetText(Guid id) =>
        _employees.Find(b => b.Id == id)?.NameFull ?? string.Empty;
}