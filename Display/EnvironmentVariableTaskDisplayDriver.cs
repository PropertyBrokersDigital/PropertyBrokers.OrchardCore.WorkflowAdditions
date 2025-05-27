using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using PropertyBrokers.OrchardCore.WorkflowAdditions.EnvironmentVariable;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.Display
{
    public class EnvironmentVariableTaskDisplayDriver : ActivityDisplayDriver<EnvironmentVariableTask, EnvironmentVariableTaskViewModel>
    {
        protected override void EditActivity(EnvironmentVariableTask activity, EnvironmentVariableTaskViewModel model)
        {
            model.VariableName = activity.VariableName.Expression;
        }

        protected override void UpdateActivity(EnvironmentVariableTaskViewModel model, EnvironmentVariableTask activity)
        {
            activity.VariableName = new WorkflowExpression<string>(model.VariableName);
        }
    }
}