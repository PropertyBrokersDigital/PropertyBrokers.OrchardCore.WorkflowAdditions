using Fluid.Parser;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using PropertyBrokers.OrchardCore.WorkflowAdditions.EmailFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSql;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.ValidateJson
{
    public class ValidateJsonTask : TaskActivity
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;
        public ValidateJsonTask(
            IWorkflowExpressionEvaluator expressionEvaluator,
            ISession session,
            IStringLocalizer<EmailFileTask> localizer
        )
        {
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
        }

        public override string Name => nameof(ValidateJsonTask);
        public override LocalizedString DisplayText => S["Validate JSON Task"];
        public override LocalizedString Category => S["Validation"];

        public WorkflowExpression<string> SchemaContent
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> JsonContent
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> SchemaValidationState
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Passed"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            // Retrieve the schema content item
            string schemaContent = await _expressionEvaluator.EvaluateAsync(SchemaContent, workflowContext, null);
            JSchema schema = JSchema.Parse(schemaContent);

            // JSON body
            string contentStr = await _expressionEvaluator.EvaluateAsync(JsonContent, workflowContext, null);
            JObject content;
            try
            {
                content = JObject.Parse(contentStr);
            } catch (Exception ex)
            {
                SchemaValidationState = new WorkflowExpression<string>("Json Body Parse Error: " + ex.Message);
                return Outcomes("Failed");
            }

            List<string> errorMessages = new();
            // Gets all errors with JSON body, not just the first
            content.Validate(schema, (o, e) =>
            {   
                errorMessages.Add(e.Message);
            });

            if(errorMessages.Count > 0)
            {
                SchemaValidationState = new WorkflowExpression<string>("Json Body Parse Error: " + string.Join("\n", errorMessages));
                return Outcomes("Failed");
            }
            return Outcomes("Passed");
        }
    }
}
