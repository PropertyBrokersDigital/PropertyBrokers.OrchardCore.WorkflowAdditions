using Microsoft.Extensions.Localization;
using Mjml.Net;
using Newtonsoft.Json.Linq;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using PropertyBrokers.OrchardCore.WorkflowAdditions.EmailFile;
using Stubble.Core.Builders;
using Stubble.Extensions.JsonNet;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stubble.Core.Settings;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.ProcessMjmlTemplate
{

    public class ProcessMjmlTemplateTask : TaskActivity
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;
        public ProcessMjmlTemplateTask(
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<EmailFileTask> localizer
        )
        {
            _expressionEvaluator = expressionEvaluator;
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

            var builder = new StubbleBuilder().Configure(settings => settings.AddJsonNet()).Build();

           string parsedMustacheTemplate = builder.Render(mjmlTemplate, mergeTags, new RenderSettings
            {
                SkipHtmlEncoding = true
            });
            MjmlRenderer mjmlRenderer = new();
            MjmlOptions mjmlOptions = new()
            {
                Beautify = true
            };

            // errors don't prevent build, usually related to mjml attributes
            // if there is a real error, it should either fault, or be VERY obvious otherwise
            var (html, errors) = mjmlRenderer.Render(parsedMustacheTemplate, mjmlOptions);
            
            workflowContext.Properties["ProcessMjmlTemplateTaskOutput"] = html;
            return Outcomes("Done");
        }
    }
}
