using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.SaveFileToMedia
{
    public class SaveToMediaTaskDisplayDriver : ActivityDisplayDriver<SaveToMediaTask, SaveToMediaTaskViewModel>
    {
        protected override void EditActivity(SaveToMediaTask source, SaveToMediaTaskViewModel model)
        {
            model.FileUrl = source.FileUrl.Expression;
            model.MediaPath = source.MediaPath.Expression;
        }

        protected override void UpdateActivity(SaveToMediaTaskViewModel model, SaveToMediaTask activity)
        {
            activity.FileUrl = new WorkflowExpression<string>(model.FileUrl);
            activity.MediaPath = new WorkflowExpression<string>(model.MediaPath);
        }
    }
}