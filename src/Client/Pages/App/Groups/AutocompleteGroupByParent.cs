using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.App.Groups;

public class AutocompleteGroupByParent : MudAutocomplete<string>
{
    [Parameter]
    public string Parent { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private IGroupsClient Client { get; set; } = default!;

    private List<GroupDto> _list = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
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
        var filter = new GroupSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name", "description", "notes" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfGroupDto response)
        {
            _list = response.Data.ToList();
        }

        return _list.Where(x => x.Parent.Equals(Parent)).Select(x => x.Name);
    }
}