using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.CAD.Accounts;

public partial class Ledgers
{
    [Parameter]
    public Guid AccountId { get; set; } = Guid.Empty;
    [Inject]
    protected ILedgersClient Client { get; set; } = default!;

    protected EntityServerTableContext<LedgerDto, Guid, LedgerUpdateRequest> Context { get; set; } = default!;

    private EntityTable<LedgerDto, Guid, LedgerUpdateRequest>? _table;

    private string? _searchString;

    //Advanced Search
    private Guid _searchAccountId;

    protected override void OnParametersSet()
    {
        if (AccountId != Guid.Empty)
        {
            _searchAccountId = AccountId;
        }
    }

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Ledger",
            entityNamePlural: "Ledger",
            entityResource: FSHResource.CAD,
            fields: new()
            {
                //new(data => data.Id, "Id", "Id"),
                //new(data => data.AccountId, "AccountId", "AccountId"),
                new(data => data.BillMonth, "Bill Month", "BillMonth"),
                new(data => data.BillNumber, "Bill Number", "BillNumber"),
                new(data => data.Debit, "Debit", "Debit", typeof(decimal)),
                new(data => data.Credit, "Credit", "Credit", typeof(decimal)),
                new(data => data.Balance, "Balance", "Balance", typeof(decimal)),
                new(data => data.PostingDate, "PostingDate", "PostingDate", typeof(DateOnly)),
            },
            enableAdvancedSearch: false,
            idFunc: data => data.Id,
            searchFunc: async _filter =>
            {
                var result = await Client.AccountLedgerAsync(_searchAccountId);
                return result.Adapt<PaginationResponse<LedgerDto>>();
            },
            //searchFunc: async _filter =>
            //{
            //    var filter = _filter.Adapt<LedgerSearchRequest>();
            //    filter.AccountId = SearchAccountId == default ? null : SearchAccountId;
            //    var result = await Client.SearchAsync(filter);
            //    return result.Adapt<PaginationResponse<LedgerDto>>();
            //},
            createFunc: null, //async data => await Client.CreateAsync(data.Adapt<LedgerCreateRequest>()),
            updateFunc: null, //async (id, Ledger) => await Client.UpdateAsync(id, Ledger.Adapt<LedgerUpdateRequest>()),
            deleteFunc: null, //async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}

