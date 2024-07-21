using System;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;
using PropertyBrokers.OrchardCore.WorkflowAdditions.GenerateSecureUrlToken.ViewModels;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GenerateSecureUrlToken.Drivers
{
    public class GenerateSecureUrlTokenTaskDisplayDriver : ActivityDisplayDriver<GenerateSecureUrlTokenTask, GenerateSecureUrlTokenTaskViewModel>
    {
        protected override void EditActivity(GenerateSecureUrlTokenTask source, GenerateSecureUrlTokenTaskViewModel model)
        {
            model.Email = source.Email.Expression;
            model.ValidityInHours = source.ValidityInHours.Expression;
        }

        protected override void UpdateActivity(GenerateSecureUrlTokenTaskViewModel model, GenerateSecureUrlTokenTask activity)
        {
            activity.Email = new WorkflowExpression<string>(model.Email);
            activity.ValidityInHours = new WorkflowExpression<int>(model.ValidityInHours);
        }
    }
}