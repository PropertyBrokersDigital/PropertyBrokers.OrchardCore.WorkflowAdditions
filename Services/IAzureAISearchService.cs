using System.Threading.Tasks;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.Services
{
    public interface IAzureAISearchService
    {
        Task<AzureAISearchResult> IndexDocumentAsync(string searchServiceUrl, string apiKey, string indexName, string documentId, string jsonPayload);
        Task<bool> IndexExistsAsync(string searchServiceUrl, string apiKey, string indexName);
        Task<AzureAISearchResult> CreateIndexAsync(string searchServiceUrl, string apiKey, string indexName);
    }

    public class AzureAISearchResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string DocumentKey { get; set; }
        public bool IndexCreated { get; set; }
    }
}