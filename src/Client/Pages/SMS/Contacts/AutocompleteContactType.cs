using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.SMS.Contacts;

public class AutocompleteContactType : MudAutocomplete<string>
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]
    private IContactsClient Client { get; set; } = default!;

    private List<ContactDto> _list = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = "Contact Type";
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
        var filter = new ContactSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "contacttype" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfContactDto response)
        {
            _list = response.Data.ToList();
        }

        return _list.Select(x => x.ContactType).Distinct();
    }
}