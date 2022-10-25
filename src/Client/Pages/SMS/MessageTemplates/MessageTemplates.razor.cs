using Mapster;
using Microsoft.AspNetCore.Components;
using ZANECO.WASM.Client.Components.EntityTable;
using ZANECO.WASM.Client.Infrastructure.ApiClient;
using ZANECO.WebApi.Shared.Authorization;

namespace ZANECO.WASM.Client.Pages.SMS.MessageTemplates;
public partial class MessageTemplates
{
    [Inject]
    protected IMessageTemplatesClient Client { get; set; } = default!;

    protected EntityServerTableContext<MessageTemplateDto, Guid, MessageTemplateUpdateRequest> Context { get; set; } = default!;

    private EntityTable<MessageTemplateDto, Guid, MessageTemplateUpdateRequest> _table = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: "MessageTemplate",
            entityNamePlural: "MessageTemplates",
            entityResource: FSHResource.SMS,
            fields: new()
            {
                new(data => data.TemplateType, "Template Type", "TemplateType"),
                new(data => data.Subject, "Subject", "Subject"),
                new(data => data.Message, "Message", "Message"),
                new(data => data.Description, "Description", "Description"),
                new(data => data.Notes, "Notes", "Notes"),
            },
            enableAdvancedSearch: true,
            idFunc: MessageTemplate => MessageTemplate.Id,
            searchFunc: async filter => (await Client
                .SearchAsync(filter.Adapt<MessageTemplateSearchRequest>()))
                .Adapt<PaginationResponse<MessageTemplateDto>>(),
            createFunc: async MessageTemplate => await Client.CreateAsync(MessageTemplate.Adapt<MessageTemplateCreateRequest>()),
            updateFunc: async (id, MessageTemplate) => await Client.UpdateAsync(id, MessageTemplate),
            deleteFunc: async id => await Client.DeleteAsync(id),
            exportAction: string.Empty);
}
