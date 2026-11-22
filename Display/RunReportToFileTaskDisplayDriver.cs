using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.RunReportToFile
{
    public class RunReportToFileTaskDisplayDriver : ActivityDisplayDriver<RunReportToFileTask, RunReportToFileTaskViewModel>
    {
        protected override void EditActivity(RunReportToFileTask activity, RunReportToFileTaskViewModel model)
        {
            model.ReportContentItemId = activity.ReportContentItemId.Expression;
            model.JsonData = activity.JsonData.Expression;
            model.MediaPath = activity.MediaPath.Expression;
            model.FileName = activity.FileName.Expression;
            model.Overwrite = activity.Overwrite;
        }

        protected override void UpdateActivity(RunReportToFileTaskViewModel model, RunReportToFileTask activity)
        {
            activity.ReportContentItemId = new WorkflowExpression<string>(model.ReportContentItemId);
            activity.JsonData = new WorkflowExpression<string>(model.JsonData);
            activity.MediaPath = new WorkflowExpression<string>(model.MediaPath);
            activity.FileName = new WorkflowExpression<string>(model.FileName);
            activity.Overwrite = model.Overwrite;
        }
    }
}
