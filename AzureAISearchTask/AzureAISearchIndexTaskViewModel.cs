namespace PropertyBrokers.OrchardCore.WorkflowAdditions.AzureAISearchTask
{
    public class AzureAISearchIndexTaskViewModel
    {
        public string IndexNameExpression { get; set; }
        public string JsonPayloadExpression { get; set; }
        public string ServiceEndpointExpression { get; set; }
        public string ApiKeyExpression { get; set; }
        public bool CreateIndexIfNotExists { get; set; } = true;
    }
}