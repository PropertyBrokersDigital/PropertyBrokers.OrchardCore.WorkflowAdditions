using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleAnalytics
{
    public class GoogleAnalyticsTaskDisplayDriver : ActivityDisplayDriver<GoogleAnalyticsTask, GoogleAnalyticsTaskViewModel>
    {
        protected override void EditActivity(GoogleAnalyticsTask activity, GoogleAnalyticsTaskViewModel model)
        {
            model.MeasurementId = activity.MeasurementId.Expression;
            model.ApiSecret = activity.ApiSecret.Expression;
            model.ClientId = activity.ClientId.Expression;
            model.EventName = activity.EventName.Expression;
            model.EventParameters = activity.EventParameters.Expression;
        }

        protected override void UpdateActivity(GoogleAnalyticsTaskViewModel model, GoogleAnalyticsTask activity)
        {
            activity.MeasurementId = new WorkflowExpression<string>(model.MeasurementId);
            activity.ApiSecret = new WorkflowExpression<string>(model.ApiSecret);
            activity.ClientId = new WorkflowExpression<string>(model.ClientId);
            activity.EventName = new WorkflowExpression<string>(model.EventName);
            activity.EventParameters = new WorkflowExpression<string>(model.EventParameters);
        }
    }
}