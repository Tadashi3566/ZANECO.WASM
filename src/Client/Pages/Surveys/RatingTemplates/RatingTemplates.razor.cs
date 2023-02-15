using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.Surveys.RatingTemplates;
public partial class RatingTemplates
{
    [Inject]
    protected IRatesClient RatesClient { get; set; } = default!;
    [Inject]
    protected IRatingTemplatesClient RatingTemplatesClient { get; set; } = default!;

    protected EntityServerTableContext<RatingTemplateDto, Guid, RatingTemplateUpdateRequest> Context { get; set; } = default!;

    private EntityTable<RatingTemplateDto, Guid, RatingTemplateUpdateRequest>? _table;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "Rating Template",
            entityNamePlural: "Rating Templates",
            entityResource: FSHResource.Rating,
            fields: new()
            {
                new(dto => dto.RateNumber, "Number", "Number"),
                new(dto => dto.RateName, "Name", "Name"),
                new(dto => dto.Comment, "Comment", "Comment"),
                new(dto => dto.Description, "Description", "Description"),
                new(dto => dto.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: true,
            idFunc: prod => prod.Id,
            searchFunc: async filter => (await RatingTemplatesClient
                .SearchAsync(filter.Adapt<RatingTemplateSearchRequest>()))
                .Adapt<PaginationResponse<RatingTemplateDto>>(),
            createFunc: async prod => await RatingTemplatesClient.CreateAsync(prod.Adapt<RatingTemplateCreateRequest>()),
            updateFunc: async (id, prod) => await RatingTemplatesClient.UpdateAsync(id, prod.Adapt<RatingTemplateUpdateRequest>()),
            deleteFunc: async id => await RatingTemplatesClient.DeleteAsync(id),
            exportFunc: async filter =>
                {
                    var exportFilter = filter.Adapt<RatingTemplateExportRequest>();

                    exportFilter.RateId = SearchRateId == default ? null : SearchRateId;

                    return await RatingTemplatesClient.ExportAsync(exportFilter);
                });

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