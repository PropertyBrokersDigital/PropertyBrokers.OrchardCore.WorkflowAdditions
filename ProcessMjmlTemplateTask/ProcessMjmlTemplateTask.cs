using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using PropertyBrokers.OrchardCore.EmailTemplating.Services;
using PropertyBrokers.OrchardCore.WorkflowAdditions.EmailFile;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.ProcessMjmlTemplate
{

    public class ProcessMjmlTemplateTask : TaskActivity
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IMjmlTemplateService _mjmlTemplateService;
        private readonly IStringLocalizer S;

        public ProcessMjmlTemplateTask(
            IWorkflowExpressionEvaluator expressionEvaluator,
            IMjmlTemplateService mjmlTemplateService,
            IStringLocalizer<EmailFileTask> localizer
        )
        {
            _expressionEvaluator = expressionEvaluator;
            _mjmlTemplateService = mjmlTemplateService;
            S = localizer;
        }

        public override string Name => nameof(ProcessMjmlTemplateTask);
        public override LocalizedString DisplayText => S["Process MJML Template Task"];
        public override LocalizedString Category => S["Messaging"];

        public WorkflowExpression<string> EmailTemplateContent
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> MergeTagsContent
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            JObject mergeTags = new();
            string mergeTagContent = await _expressionEvaluator.EvaluateAsync(MergeTagsContent, workflowContext, null);
            if (mergeTagContent != "") mergeTags = JObject.Parse(mergeTagContent);

            string mjmlTemplate = await _expressionEvaluator.EvaluateAsync(EmailTemplateContent, workflowContext, null);

            var html = await _mjmlTemplateService.ProcessMjmlTemplateAsync(mjmlTemplate, mergeTags);

            workflowContext.Properties["ProcessMjmlTemplateTaskOutput"] = html;
            return Outcomes("Done");
        }
    }
}
