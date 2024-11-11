﻿using OrchardCore.Workflows.Display;
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
            model.EventTimeStamp = activity.EventTimeStamp.Expression;
            model.RequestTimeStamp = activity.RequestTimeStamp.Expression;
            model.EventParamsExpression = activity.EventParamsExpression.Expression;
        }

        protected override void UpdateActivity(GoogleAnalyticsEventTaskViewModel model, GoogleAnalyticsEventTask activity)
        {
            activity.MeasurementId = new WorkflowExpression<string>(model.MeasurementId);
            activity.ApiSecret = new WorkflowExpression<string>(model.ApiSecret);
            activity.ClientId = new WorkflowExpression<string>(model.ClientId);
            activity.SessionId = new WorkflowExpression<string>(model.SessionId);
            activity.EventName = new WorkflowExpression<string>(model.EventName);
            activity.EventTimeStamp = new WorkflowExpression<string>(model.EventTimeStamp);
            activity.RequestTimeStamp = new WorkflowExpression<string>(model.RequestTimeStamp);
            activity.EventParamsExpression = new WorkflowExpression<string>(model.EventParamsExpression);
        }
    }
}