namespace PropertyBrokers.OrchardCore.WorkflowAdditions.AzureAISearchTask
{
    public class AzureAISearchIndexTaskViewModel
    {
        public string SearchServiceUrlExpression { get; set; }
        public string ApiKeyExpression { get; set; }
        public string IndexNameExpression { get; set; }
        public string DocumentIdExpression { get; set; }
        public string JsonPayloadExpression { get; set; }
        public bool CreateIndexIfNotExists { get; set; } = true;
    }
}