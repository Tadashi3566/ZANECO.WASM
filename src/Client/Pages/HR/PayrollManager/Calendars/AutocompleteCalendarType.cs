using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.HR.PayrollManager.Calendars;
public class AutocompleteCalendarType : MudAutocomplete<string>
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
        Label = "Calendar Type";
        Variant = Variant.Filled;
        Dense = true;
        Margin = Margin.Dense;
        ResetValueOnEmptyText = true;
        SearchFunc = SearchText;
        return base.SetParametersAsync(parameters);
    }

    private async Task<IEnumerable<string>> SearchText(string value)
    {
        var filter = new GroupSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "name", "description", "notes" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(() => Client.SearchAsync(filter), Snackbar) is PaginationResponseOfGroupDto response)
        {
            _groups = response.Data.Where(x => x.Parent.Equals("CALENDAR") || x.Parent.Equals("LEAVE")).ToList();
        }

        return _groups.Select(x => x.Name);
    }
}