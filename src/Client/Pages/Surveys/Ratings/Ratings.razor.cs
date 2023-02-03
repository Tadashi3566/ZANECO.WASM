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

    private string? _searchString;
    protected override void OnInitialized() =>
        Context = new(
            entityName: "Rating",
            entityNamePlural: "Ratings",
            entityResource: FSHResource.Rating,
            fields: new()
            {
                new(dto => dto.RateNumber, "Stars", "RateNumber", Template: TemplateStars),
                new(dto => dto.CreatedOn, "Created On", "CreatedOn", typeof(DateTime)),
                new(dto => dto.Comment, "Comment", "Comment"),
                new(dto => dto.Description, "Description", "Description", Template: TemplateDescriptionNotes),
                new(dto => dto.Notes, "Notes", visible: false),
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
}