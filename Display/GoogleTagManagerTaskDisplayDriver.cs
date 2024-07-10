using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleTagManager
{
    public class GoogleTagManagerTaskDisplayDriver : ActivityDisplayDriver<GoogleTagManagerTask, GoogleTagManagerTaskViewModel>
    {
        protected override void EditActivity(GoogleTagManagerTask activity, GoogleTagManagerTaskViewModel model)
        {
            model.MeasurementId = activity.MeasurementId.Expression;
            model.ApiSecret = activity.ApiSecret.Expression;
            model.ClientId = activity.ClientId.Expression;
            model.EventName = activity.EventName.Expression;
            model.EventParamsExpression = activity.EventParamsExpression.Expression;
        }

        protected override void UpdateActivity(GoogleTagManagerTaskViewModel model, GoogleTagManagerTask activity)
        {
            activity.MeasurementId = new WorkflowExpression<string>(model.MeasurementId);
            activity.ApiSecret = new WorkflowExpression<string>(model.ApiSecret);
            activity.ClientId = new WorkflowExpression<string>(model.ClientId);
            activity.EventName = new WorkflowExpression<string>(model.EventName);
            activity.EventParamsExpression = new WorkflowExpression<string>(model.EventParamsExpression);
        }
    }
}