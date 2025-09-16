using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using PropertyBrokers.OrchardCore.WorkflowAdditions.Services;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.AzureAISearchTask
{
    public class AzureAISearchIndexTask : TaskActivity
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;
        private readonly ILogger<AzureAISearchIndexTask> _logger;

        public AzureAISearchIndexTask(
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<AzureAISearchIndexTask> localizer,
            ILogger<AzureAISearchIndexTask> logger
        )
        {
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


        public WorkflowExpression<string> JsonPayload
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ServiceEndpoint
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ApiKey
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
                var jsonPayload = await _expressionEvaluator.EvaluateAsync(JsonPayload, workflowContext, null);
                var serviceEndpoint = await _expressionEvaluator.EvaluateAsync(ServiceEndpoint, workflowContext, null);
                var apiKey = await _expressionEvaluator.EvaluateAsync(ApiKey, workflowContext, null);

                // Validate inputs using the comprehensive validator
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

                var endpointValidation = AzureAISearchIndexValidator.ValidateSearchServiceUrl(serviceEndpoint);
                if (!endpointValidation.IsValid)
                {
                    workflowContext.LastResult = new { Error = endpointValidation.ErrorMessage };
                    return Outcomes("ValidationFailed");
                }

                var apiKeyValidation = AzureAISearchIndexValidator.ValidateApiKey(apiKey);
                if (!apiKeyValidation.IsValid)
                {
                    workflowContext.LastResult = new { Error = apiKeyValidation.ErrorMessage };
                    return Outcomes("ValidationFailed");
                }

                _logger.LogInformation("Starting Azure AI Search indexing for index: {IndexName}", indexName);

                bool indexCreated = false;

                // Check if we should create the index
                if (CreateIndexIfNotExists)
                {
                    var indexExists = await CheckIndexExistsDirectlyAsync(indexName, serviceEndpoint, apiKey);
                    _logger.LogInformation("Index exists check result: {IndexExists}", indexExists);

                    if (!indexExists)
                    {
                        _logger.LogInformation("Creating new Azure AI Search index: {IndexName}", indexName);
                        await CreateIndexDirectlyAsync(indexName, serviceEndpoint, apiKey);
                        indexCreated = true;
                        _logger.LogInformation("Successfully created Azure AI Search index: {IndexName}", indexName);
                    }
                    else
                    {
                        _logger.LogInformation("Index already exists, skipping creation");
                    }
                }

                // Convert JSON to a simple dictionary for Azure AI Search
                var searchDocument = ConvertJsonToSearchDocument(jsonPayload);

                // Upload document directly using Azure client
                await UploadDocumentDirectlyAsync(indexName, searchDocument, serviceEndpoint, apiKey);

                var finalDocumentId = searchDocument.ContainsKey("id") ? searchDocument["id"].ToString() : "unknown";
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

        private async Task<bool> CheckIndexExistsDirectlyAsync(string indexName, string serviceEndpoint, string apiKey)
        {
            try
            {
                var endpoint = new Uri(serviceEndpoint);
                var credential = new AzureKeyCredential(apiKey);
                var searchIndexClient = new SearchIndexClient(endpoint, credential);
                
                var response = await searchIndexClient.GetIndexAsync(indexName);
                return response != null;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if index exists: {IndexName}", indexName);
                throw;
            }
        }

        private async Task CreateIndexDirectlyAsync(string indexName, string serviceEndpoint, string apiKey)
        {
            try
            {
                var endpoint = new Uri(serviceEndpoint);
                var credential = new AzureKeyCredential(apiKey);
                var searchIndexClient = new SearchIndexClient(endpoint, credential);

                // Create a simple index schema with essential fields
                var searchIndex = new SearchIndex(indexName)
                {
                    Fields = {
                        new SearchField("id", SearchFieldDataType.String) { IsKey = true, IsSearchable = false, IsFilterable = true },
                        new SearchField("Content", SearchFieldDataType.String) { IsSearchable = true },
                        new SearchField("Keywords", SearchFieldDataType.String) { IsSearchable = true, IsFilterable = true, IsFacetable = true },
                        new SearchField("Author", SearchFieldDataType.String) { IsSearchable = true, IsFilterable = true, IsFacetable = true }
                    }
                };

                await searchIndexClient.CreateIndexAsync(searchIndex);
                _logger.LogInformation("Successfully created Azure AI Search index directly: {IndexName}", indexName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Azure AI Search index directly: {IndexName}", indexName);
                throw;
            }
        }

        private async Task UploadDocumentDirectlyAsync(string indexName, Dictionary<string, object> searchDocument, string serviceEndpoint, string apiKey)
        {
            try
            {
                var endpoint = new Uri(serviceEndpoint);
                var credential = new AzureKeyCredential(apiKey);
                var searchClient = new SearchClient(endpoint, indexName, credential);

                var batch = IndexDocumentsBatch.Upload(new[] { searchDocument });
                await searchClient.IndexDocumentsAsync(batch);
                _logger.LogInformation("Successfully uploaded document to Azure AI Search index: {IndexName}", indexName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload document to Azure AI Search index: {IndexName}", indexName);
                throw;
            }
        }

        private Dictionary<string, object> ConvertJsonToSearchDocument(string jsonPayload)
        {
            using var document = JsonDocument.Parse(jsonPayload);
            var searchDocument = new Dictionary<string, object>();

            // Convert JSON properties directly to dictionary
            foreach (var property in document.RootElement.EnumerateObject())
            {
                var value = GetPropertyValue(property.Value);
                if (value != null)
                {
                    searchDocument[property.Name] = value;
                }
            }

            // Ensure we have an ID field for Azure AI Search
            if (!searchDocument.ContainsKey("id"))
            {
                searchDocument["id"] = Guid.NewGuid().ToString();
            }

            return searchDocument;
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

    }
}