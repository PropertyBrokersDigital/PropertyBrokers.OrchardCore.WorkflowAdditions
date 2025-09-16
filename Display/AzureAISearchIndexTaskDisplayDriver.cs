using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using PropertyBrokers.OrchardCore.WorkflowAdditions.AzureAISearchTask;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.Display
{
    public class AzureAISearchIndexTaskDisplayDriver : ActivityDisplayDriver<AzureAISearchIndexTask, AzureAISearchIndexTaskViewModel>
    {
        protected override void EditActivity(AzureAISearchIndexTask activity, AzureAISearchIndexTaskViewModel model)
        {
            model.IndexNameExpression = activity.IndexName.Expression;
            model.JsonPayloadExpression = activity.JsonPayload.Expression;
            model.ServiceEndpointExpression = activity.ServiceEndpoint.Expression;
            model.ApiKeyExpression = activity.ApiKey.Expression;
            model.CreateIndexIfNotExists = activity.CreateIndexIfNotExists;
        }

        protected override void UpdateActivity(AzureAISearchIndexTaskViewModel model, AzureAISearchIndexTask activity)
        {
            activity.IndexName = new WorkflowExpression<string>(model.IndexNameExpression);
            activity.JsonPayload = new WorkflowExpression<string>(model.JsonPayloadExpression);
            activity.ServiceEndpoint = new WorkflowExpression<string>(model.ServiceEndpointExpression);
            activity.ApiKey = new WorkflowExpression<string>(model.ApiKeyExpression);
            activity.CreateIndexIfNotExists = model.CreateIndexIfNotExists;
        }
    }
}