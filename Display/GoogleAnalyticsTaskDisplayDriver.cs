using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleAnalyticsManager
{
    public class GoogleAnalyticsTaskDisplayDriver : ActivityDisplayDriver<GoogleAnalyticsManagerTask, GoogleAnalyticsTaskViewModel>
    {
        protected override void EditActivity(GoogleAnalyticsManagerTask activity, GoogleAnalyticsTaskViewModel model)
        {
            model.MeasurementId = activity.MeasurementId.Expression;
            model.ApiSecret = activity.ApiSecret.Expression;
            model.ClientId = activity.ClientId.Expression;
            model.EventName = activity.EventName.Expression;
            model.EventParamsExpression = activity.EventParamsExpression.Expression;
        }

        protected override void UpdateActivity(GoogleAnalyticsTaskViewModel model, GoogleAnalyticsManagerTask activity)
        {
            activity.MeasurementId = new WorkflowExpression<string>(model.MeasurementId);
            activity.ApiSecret = new WorkflowExpression<string>(model.ApiSecret);
            activity.ClientId = new WorkflowExpression<string>(model.ClientId);
            activity.EventName = new WorkflowExpression<string>(model.EventName);
            activity.EventParamsExpression = new WorkflowExpression<string>(model.EventParamsExpression);
        }
    }
}