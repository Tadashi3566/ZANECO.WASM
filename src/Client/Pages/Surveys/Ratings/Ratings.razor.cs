using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.Surveys.Ratings;
public partial class Ratings
{
    [Inject]
    protected IRatesClient RatesClient { get; set; } = default!;
    [Inject]
    protected IRatingsClient RatingsClient { get; set; } = default!;

    protected EntityServerTableContext<RatingDto, Guid, RatingUpdateRequest> Context { get; set; } = default!;

    private EntityTable<RatingDto, Guid, RatingUpdateRequest> _table = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Rating",
            entityNamePlural: "Ratings",
            entityResource: FSHResource.Rating,
            fields: new()
            {
                new(dto => dto.RateNumber, "Stars", "Number"),
                new(dto => dto.Comment, "Comment", "Comment"),
                new(dto => dto.Description, "Description", "Description"),
                new(dto => dto.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: true,
            idFunc: prod => prod.Id,
            searchFunc: async filter => (await RatingsClient
                .SearchAsync(filter.Adapt<RatingSearchRequest>()))
                .Adapt<PaginationResponse<RatingDto>>(),
            createFunc: async prod => await RatingsClient.CreateAsync(prod.Adapt<RatingCreateRequest>()),
            updateFunc: async (id, prod) => await RatingsClient.UpdateAsync(id, prod.Adapt<RatingUpdateRequest>()),
            deleteFunc: async id => await RatingsClient.DeleteAsync(id)
            //exportFunc: async filter =>
            //    {
            //        var exportFilter = filter.Adapt<RatingExportRequest>();

            //        exportFilter.RateId = SearchRateId == default ? null : SearchRateId;

            //        return await RatingsClient.ExportAsync(exportFilter);
            //    }
                );

    // Advanced Search
    private Guid _searchRateId;
    private Guid SearchRateId
    {
        get => _searchRateId;
        set
        {
            _searchRateId = value;
            _ = _table.ReloadDataAsync();
        }
    }
}