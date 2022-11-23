﻿using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.HR.Ranks;
public partial class Ranks
{
    [Inject]
    protected IRanksClient Client { get; set; } = default!;

    protected EntityServerTableContext<RankDto, Guid, RankUpdateRequest> Context { get; set; } = default!;

    private EntityTable<RankDto, Guid, RankUpdateRequest> _table = default!;

    protected override async Task OnInitializedAsync() =>
        Context = new(
            entityName: "Rank",
            entityNamePlural: "Ranks",
            entityResource: FSHResource.Ranks,
            fields: new()
            {
                new(data => data.Id, "Id", "Id"),
                new(data => data.Number, "Number", "Number"),
                new(data => data.Name, "Name", "Name"),
                new(data => data.Amount.ToString("N2"), "Amount", "Amount"),
                new(data => data.Step.ToString("N2"), "Step Increment", "Step"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            idFunc: Rank => Rank.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<RankSearchRequest>()))
                .Adapt<PaginationResponse<RankDto>>(),
            createFunc: async Rank => await Client.CreateAsync(Rank.Adapt<RankCreateRequest>()),
            updateFunc: async (id, Rank) => await Client.UpdateAsync(id, Rank),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}