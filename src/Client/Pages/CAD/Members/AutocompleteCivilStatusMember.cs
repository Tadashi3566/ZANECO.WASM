using Microsoft.AspNetCore.Components;
using MudBlazor;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WASM.Client.Shared;

namespace ZANECO.WASM.Client.Pages.CAD.Members;

public class AutocompleteCivilStatusMember : MudAutocomplete<string>
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;
    [Inject]
    private IMembersClient Client { get; set; } = default!;

    private List<MemberDto> _list = new();

    // supply default parameters, but leave the possibility to override them
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Label = "Civil Status";
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
        var filter = new MemberSearchRequest
        {
            PageSize = 10,
            AdvancedSearch = new() { Fields = new[] { "civilstatus" }, Keyword = value }
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => Client.SearchAsync(filter), Snackbar)
            is PaginationResponseOfMemberDto response)
        {
            _list = response.Data.ToList();
        }

        return _list
            .Where(x => x.CivilStatus != string.Empty)
            .Select(x => x.CivilStatus).Distinct();
    }
}