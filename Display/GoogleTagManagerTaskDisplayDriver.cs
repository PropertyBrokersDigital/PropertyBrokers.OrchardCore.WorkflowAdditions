using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleTagManager;


namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleTagManager
{
    public class GoogleTagManagerTaskDisplayDriver : ActivityDisplayDriver<GoogleTagManagerTask, GoogleTagManagerTaskViewModel>
    {
        protected override void EditActivity(GoogleTagManagerTask activity, GoogleTagManagerTaskViewModel model)
        {
            model.ContainerId = activity.ContainerId.Expression;
            model.EventExpression = activity.EventExpression.Expression;
            model.DataLayerExpression = activity.DataLayerExpression.Expression;
        }

        protected override void UpdateActivity(GoogleTagManagerTaskViewModel model, GoogleTagManagerTask activity)
        {
            activity.ContainerId = new WorkflowExpression<string>(model.ContainerId);
            activity.EventExpression = new WorkflowExpression<string>(model.EventExpression);
            activity.DataLayerExpression = new WorkflowExpression<string>(model.DataLayerExpression);
        }
    }
}