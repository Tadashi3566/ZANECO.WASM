using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.CAD.RemoteCollections;

public class AutocompleteCollector : MudAutocomplete<string>
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]
    private IRemoteCollectionsClient Client { get; set; } = default!;

    private List<RemoteCollectionDto> _list = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = "Colllector";
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
        var filter = new RemoteCollectionSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "collector" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
            () => Client.SearchAsync(filter),
                Snackbar
                )
            is PaginationResponseOfRemoteCollectionDto response)
        {
            _list = response.Data.ToList();
        }

        return _list.Select(x => x.Collector).Distinct();
    }
}