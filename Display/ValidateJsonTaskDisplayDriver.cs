using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using PropertyBrokers.OrchardCore.WorkflowAdditions.ValidateJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.Display
{
    public class ValidateJsonTaskDisplayDriver : ActivityDisplayDriver<ValidateJson.ValidateJsonTask, ValidateJsonTaskViewModel>
    {
        protected override void EditActivity(ValidateJson.ValidateJsonTask activity, ValidateJsonTaskViewModel model)
        {
            model.JsonContent = activity.JsonContent.Expression;
            model.SchemaContent = activity.SchemaContent.Expression;
            model.SchemaValidationState = activity.SchemaValidationState.Expression;
        }

        protected override void UpdateActivity(ValidateJsonTaskViewModel model, ValidateJson.ValidateJsonTask activity)
        {
            activity.JsonContent = new WorkflowExpression<string>(model.JsonContent);
            activity.SchemaContent = new WorkflowExpression<string>(model.SchemaContent);
            activity.SchemaValidationState = new WorkflowExpression<string>(model.SchemaValidationState);
        }
    }
}
