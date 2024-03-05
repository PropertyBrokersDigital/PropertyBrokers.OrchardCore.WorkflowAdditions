using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using PropertyBrokers.OrchardCore.WorkflowAdditions.ProcessMjmlTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.Display
{
    public class ProcessMjmlTemplateTaskDisplayDriver : ActivityDisplayDriver<ProcessMjmlTemplateTask, ProcessMjmlTemplateTaskViewModel>
    {
        protected override void EditActivity(ProcessMjmlTemplateTask activity, ProcessMjmlTemplateTaskViewModel model)
        {
            model.EmailTemplateContent = activity.EmailTemplateContent.Expression;
            model.MergeTagsContent = activity.MergeTagsContent.Expression;
        }

        protected override void UpdateActivity(ProcessMjmlTemplateTaskViewModel model, ProcessMjmlTemplateTask activity)
        {
            activity.EmailTemplateContent = new WorkflowExpression<string>(model.EmailTemplateContent);
            activity.MergeTagsContent = new WorkflowExpression<string>(model.MergeTagsContent);
        }
    }
}
