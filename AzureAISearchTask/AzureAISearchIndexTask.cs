using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Indexing;
using static OrchardCore.Indexing.DocumentIndexBase;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using PropertyBrokers.OrchardCore.WorkflowAdditions.Services;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.AzureAISearchTask
{
    public class AzureAISearchIndexTask : TaskActivity
    {
        private readonly AzureAIIndexDocumentManager _documentManager;
        private readonly AzureAISearchIndexManager _indexManager;
        private readonly AzureAISearchIndexSettingsService _indexSettingsService;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;
        private readonly ILogger<AzureAISearchIndexTask> _logger;

        public AzureAISearchIndexTask(
            AzureAIIndexDocumentManager documentManager,
            AzureAISearchIndexManager indexManager,
            AzureAISearchIndexSettingsService indexSettingsService,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<AzureAISearchIndexTask> localizer,
            ILogger<AzureAISearchIndexTask> logger
        )
        {
            _documentManager = documentManager;
            _indexManager = indexManager;
            _indexSettingsService = indexSettingsService;
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
            _logger = logger;
        }

        public override string Name => nameof(AzureAISearchIndexTask);
        public override LocalizedString DisplayText => S["Azure AI Search Index Task"];
        public override LocalizedString Category => S["Search"];

        public WorkflowExpression<string> IndexName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> DocumentId
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> JsonPayload
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public bool CreateIndexIfNotExists
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Success"], S["ValidationFailed"], S["IndexCreated"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            try
            {
                var indexName = await _expressionEvaluator.EvaluateAsync(IndexName, workflowContext, null);
                var documentId = await _expressionEvaluator.EvaluateAsync(DocumentId, workflowContext, null);
                var jsonPayload = await _expressionEvaluator.EvaluateAsync(JsonPayload, workflowContext, null);

                // Validate inputs
                var indexNameValidation = AzureAISearchIndexValidator.ValidateIndexName(indexName);
                if (!indexNameValidation.IsValid)
                {
                    workflowContext.LastResult = new { Error = indexNameValidation.ErrorMessage };
                    return Outcomes("ValidationFailed");
                }

                var jsonValidation = AzureAISearchIndexValidator.ValidateJsonPayload(jsonPayload);
                if (!jsonValidation.IsValid)
                {
                    workflowContext.LastResult = new { Error = jsonValidation.ErrorMessage };
                    return Outcomes("ValidationFailed");
                }

                _logger.LogInformation("Starting Azure AI Search indexing for index: {IndexName}", indexName);

                // Check if index exists and get settings
                var indexSettings = await _indexSettingsService.GetAsync(indexName);
                bool indexCreated = false;

                if (indexSettings == null)
                {
                    if (CreateIndexIfNotExists)
                    {
                        // Create a basic index with minimal settings
                        indexSettings = new AzureAISearchIndexSettings
                        {
                            IndexName = indexName,
                            AnalyzerName = "standard.lucene",
                            IndexLatest = false,
                            IndexedContentTypes = new string[0] // Empty array for generic indexing
                        };

                        var indexExists = await _indexManager.ExistsAsync(indexName);
                        if (!indexExists)
                        {
                            await _indexManager.CreateAsync(indexSettings);
                            indexCreated = true;
                            _logger.LogInformation("Created new Azure AI Search index: {IndexName}", indexName);
                        }

                        // Save the index settings
                        await _indexSettingsService.UpdateAsync(indexSettings);
                    }
                    else
                    {
                        workflowContext.LastResult = new { Error = $"Index '{indexName}' does not exist and CreateIndexIfNotExists is disabled." };
                        return Outcomes("Failed");
                    }
                }

                // Convert JSON to DocumentIndex
                var documentIndex = ConvertJsonToDocumentIndex(jsonPayload, documentId);

                // Upload document using OrchardCore's service
                await _documentManager.UploadDocumentsAsync(indexName, new[] { documentIndex }, indexSettings);

                var finalDocumentId = documentIndex.ContentItemId;
                _logger.LogInformation("Successfully indexed document with ID: {DocumentId} to index: {IndexName}", finalDocumentId, indexName);

                workflowContext.LastResult = new
                {
                    DocumentKey = finalDocumentId,
                    IndexCreated = indexCreated,
                    IndexName = indexName
                };

                return indexCreated ? Outcomes("IndexCreated") : Outcomes("Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing document to Azure AI Search");
                workflowContext.LastResult = new { Error = ex.Message };
                return Outcomes("Failed");
            }
        }

        private DocumentIndex ConvertJsonToDocumentIndex(string jsonPayload, string documentId)
        {
            using var document = JsonDocument.Parse(jsonPayload);
            
            // Use provided document ID or generate one
            var finalDocumentId = !string.IsNullOrWhiteSpace(documentId) 
                ? documentId 
                : Guid.NewGuid().ToString();

            var documentIndex = new DocumentIndex(finalDocumentId, finalDocumentId);

            // Convert JSON properties to DocumentIndexEntry objects
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Name.ToLowerInvariant() == "id" && string.IsNullOrWhiteSpace(documentId))
                {
                    // Use ID from JSON if no explicit document ID was provided
                    var idValue = GetPropertyValue(property.Value);
                    if (idValue != null)
                    {
                        finalDocumentId = idValue.ToString();
                        documentIndex = new DocumentIndex(finalDocumentId, finalDocumentId);
                    }
                    continue;
                }

                var value = GetPropertyValue(property.Value);
                if (value != null)
                {
                    var entry = CreateDocumentIndexEntry(property.Name, value);
                    documentIndex.Entries.Add(entry);
                }
            }

            return documentIndex;
        }

        private object GetPropertyValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt32(out var intValue) ? intValue : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => element.EnumerateArray().Select(GetPropertyValue).ToArray(),
                JsonValueKind.Object => element.GetRawText(),
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }

        private DocumentIndexEntry CreateDocumentIndexEntry(string name, object value)
        {
            return value switch
            {
                bool boolValue => new DocumentIndexEntry(name, boolValue, Types.Boolean, DocumentIndexOptions.Store),
                int intValue => new DocumentIndexEntry(name, intValue, Types.Integer, DocumentIndexOptions.Store),
                long longValue => new DocumentIndexEntry(name, longValue, Types.Integer, DocumentIndexOptions.Store),
                double doubleValue => new DocumentIndexEntry(name, doubleValue, Types.Number, DocumentIndexOptions.Store),
                DateTime dateTimeValue => new DocumentIndexEntry(name, dateTimeValue, Types.DateTime, DocumentIndexOptions.Store),
                DateTimeOffset dateTimeOffsetValue => new DocumentIndexEntry(name, dateTimeOffsetValue, Types.DateTime, DocumentIndexOptions.Store),
                _ => new DocumentIndexEntry(name, value.ToString(), Types.Text, DocumentIndexOptions.Store)
            };
        }
    }
}