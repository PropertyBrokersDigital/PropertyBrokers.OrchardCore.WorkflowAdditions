using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.Services
{
    public class AzureAISearchService : IAzureAISearchService
    {
        private readonly ILogger<AzureAISearchService> _logger;

        public AzureAISearchService(ILogger<AzureAISearchService> logger)
        {
            _logger = logger;
        }

        public async Task<AzureAISearchResult> IndexDocumentAsync(string searchServiceUrl, string apiKey, string indexName, string documentId, string jsonPayload)
        {
            try
            {
                var searchIndexClient = CreateSearchIndexClient(searchServiceUrl, apiKey);
                var searchClient = searchIndexClient.GetSearchClient(indexName);

                var document = JsonSerializer.Deserialize<JsonDocument>(jsonPayload);
                var documentData = document.RootElement.EnumerateObject();
                
                var searchDocument = new SearchDocument();
                
                if (!string.IsNullOrWhiteSpace(documentId))
                {
                    searchDocument["id"] = documentId;
                }
                else
                {
                    searchDocument["id"] = Guid.NewGuid().ToString();
                }

                foreach (var property in documentData)
                {
                    if (property.Name.ToLowerInvariant() != "id")
                    {
                        searchDocument[property.Name] = GetPropertyValue(property.Value);
                    }
                }

                var response = await searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(new[] { searchDocument }));

                return new AzureAISearchResult
                {
                    IsSuccess = true,
                    DocumentKey = searchDocument["id"].ToString()
                };
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Azure AI Search request failed: {Message}", ex.Message);
                return new AzureAISearchResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing document to Azure AI Search");
                return new AzureAISearchResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> IndexExistsAsync(string searchServiceUrl, string apiKey, string indexName)
        {
            try
            {
                var searchIndexClient = CreateSearchIndexClient(searchServiceUrl, apiKey);
                var response = await searchIndexClient.GetIndexAsync(indexName);
                return response.Value != null;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if index exists: {IndexName}", indexName);
                throw;
            }
        }

        public async Task<AzureAISearchResult> CreateIndexAsync(string searchServiceUrl, string apiKey, string indexName)
        {
            try
            {
                var searchIndexClient = CreateSearchIndexClient(searchServiceUrl, apiKey);

                var fields = new[]
                {
                    new SearchField("id", SearchFieldDataType.String) { IsKey = true, IsSearchable = false, IsFilterable = true },
                    new SearchField("content", SearchFieldDataType.String) { IsSearchable = true, IsFilterable = false },
                    new SearchField("title", SearchFieldDataType.String) { IsSearchable = true, IsFilterable = true },
                    new SearchField("category", SearchFieldDataType.String) { IsSearchable = true, IsFilterable = true, IsFacetable = true },
                    new SearchField("tags", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsSearchable = true, IsFilterable = true, IsFacetable = true },
                    new SearchField("createdDate", SearchFieldDataType.DateTimeOffset) { IsFilterable = true, IsSortable = true }
                };

                var definition = new SearchIndex(indexName, fields);
                var response = await searchIndexClient.CreateIndexAsync(definition);

                return new AzureAISearchResult
                {
                    IsSuccess = true,
                    IndexCreated = true
                };
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to create Azure AI Search index: {IndexName}", indexName);
                return new AzureAISearchResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Azure AI Search index: {IndexName}", indexName);
                return new AzureAISearchResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private SearchIndexClient CreateSearchIndexClient(string searchServiceUrl, string apiKey)
        {
            var endpoint = new Uri(searchServiceUrl);
            var credential = new AzureKeyCredential(apiKey);
            return new SearchIndexClient(endpoint, credential);
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