using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.SaveFileToMedia
{
    public class SaveFileToMediaTaskDisplayDriver : ActivityDisplayDriver<SaveFileToMediaTask, SaveFileToMediaTaskViewModel>
    {
        protected override void EditActivity(SaveFileToMediaTask source, SaveFileToMediaTaskViewModel model)
        {
            model.FileUrl = source.FileUrl.Expression;
            model.MediaPath = source.MediaPath.Expression;
            model.FileName = source.FileName.Expression;
        }

        protected override void UpdateActivity(SaveFileToMediaTaskViewModel model, SaveFileToMediaTask activity)
        {
            activity.FileUrl = new WorkflowExpression<string>(model.FileUrl);
            activity.MediaPath = new WorkflowExpression<string>(model.MediaPath);
            activity.FileName = new WorkflowExpression<string>(model.FileName);
        }
    }
}