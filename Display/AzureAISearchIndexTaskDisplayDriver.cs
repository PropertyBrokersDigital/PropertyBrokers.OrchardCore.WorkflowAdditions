using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using PropertyBrokers.OrchardCore.WorkflowAdditions.AzureAISearchTask;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.Display
{
    public class AzureAISearchIndexTaskDisplayDriver : ActivityDisplayDriver<AzureAISearchIndexTask, AzureAISearchIndexTaskViewModel>
    {
        protected override void EditActivity(AzureAISearchIndexTask activity, AzureAISearchIndexTaskViewModel model)
        {
            model.SearchServiceUrlExpression = activity.SearchServiceUrl.Expression;
            model.ApiKeyExpression = activity.ApiKey.Expression;
            model.IndexNameExpression = activity.IndexName.Expression;
            model.DocumentIdExpression = activity.DocumentId.Expression;
            model.JsonPayloadExpression = activity.JsonPayload.Expression;
            model.CreateIndexIfNotExists = activity.CreateIndexIfNotExists;
        }

        protected override void UpdateActivity(AzureAISearchIndexTaskViewModel model, AzureAISearchIndexTask activity)
        {
            activity.SearchServiceUrl = new WorkflowExpression<string>(model.SearchServiceUrlExpression);
            activity.ApiKey = new WorkflowExpression<string>(model.ApiKeyExpression);
            activity.IndexName = new WorkflowExpression<string>(model.IndexNameExpression);
            activity.DocumentId = new WorkflowExpression<string>(model.DocumentIdExpression);
            activity.JsonPayload = new WorkflowExpression<string>(model.JsonPayloadExpression);
            activity.CreateIndexIfNotExists = model.CreateIndexIfNotExists;
        }
    }
}