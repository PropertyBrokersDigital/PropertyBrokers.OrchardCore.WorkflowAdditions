using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleAnalyticsEvent
{
    public class GoogleAnalyticsEventTaskDisplayDriver : ActivityDisplayDriver<GoogleAnalyticsEventTask, GoogleAnalyticsEventTaskViewModel>
    {
        protected override void EditActivity(GoogleAnalyticsEventTask activity, GoogleAnalyticsEventTaskViewModel model)
        {
            model.MeasurementId = activity.MeasurementId.Expression;
            model.ApiSecret = activity.ApiSecret.Expression;
            model.ClientId = activity.ClientId.Expression;
            model.SessionId = activity.SessionId.Expression;
            model.EventName = activity.EventName.Expression;
            model.EventTimeStamp = long.TryParse(activity.EventTimeStamp.Expression, out long et) ? et : 0;
            model.RequestTimeStamp = long.TryParse(activity.RequestTimeStamp.Expression, out long rt) ? rt : 0;
            model.EventParamsExpression = activity.EventParamsExpression.Expression;
        }

        protected override void UpdateActivity(GoogleAnalyticsEventTaskViewModel model, GoogleAnalyticsEventTask activity)
        {
            activity.MeasurementId = new WorkflowExpression<string>(model.MeasurementId);
            activity.ApiSecret = new WorkflowExpression<string>(model.ApiSecret);
            activity.ClientId = new WorkflowExpression<string>(model.ClientId);
            model.SessionId = activity.SessionId.Expression;
            activity.EventName = new WorkflowExpression<string>(model.EventName);
            activity.EventTimeStamp = new WorkflowExpression<long>(model.EventTimeStamp.ToString());
            activity.RequestTimeStamp = new WorkflowExpression<long>(model.RequestTimeStamp.ToString());
            activity.EventParamsExpression = new WorkflowExpression<string>(model.EventParamsExpression);
        }
    }
}