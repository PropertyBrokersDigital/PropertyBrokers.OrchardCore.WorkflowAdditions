using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.EnvironmentVariable
{
    public class EnvironmentVariableTask : TaskActivity
    {
        private const string TrueOutcome = "True";
        private const string FalseOutcome = "False";
        private const string NotFoundOutcome = "NotFound";

        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;

        public EnvironmentVariableTask(
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<EnvironmentVariableTask> localizer
        )
        {
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
        }

        public override string Name => nameof(EnvironmentVariableTask);
        public override LocalizedString DisplayText => S["Environment Variable Task"];
        public override LocalizedString Category => S["Control Flow"];

        public WorkflowExpression<string> VariableName
        {
            get => GetProperty(() => new WorkflowExpression<string>("Sync"));
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S[TrueOutcome], S[FalseOutcome], S[NotFoundOutcome]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            // Get the variable name to check
            string variableName = await _expressionEvaluator.EvaluateAsync(VariableName, workflowContext, null);

            if (string.IsNullOrWhiteSpace(variableName))
            {
                variableName = "sync"; // Default to 'sync' if not specified
            }

            // Get the environment variable value
            string variableValue = Environment.GetEnvironmentVariable(variableName);

            // Check if the variable exists
            if (variableValue == null)
            {
                // Store the result in workflow context for potential use
                workflowContext.Properties[$"{Name}_Result"] = NotFoundOutcome;
                workflowContext.Properties[$"{Name}_Value"] = null;
                return Outcomes(NotFoundOutcome);
            }

            // Check if the value is true/false (case-insensitive)
            bool isTrue = string.Equals(variableValue, "true", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(variableValue, "1", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(variableValue, "yes", StringComparison.OrdinalIgnoreCase);

            bool isFalse = string.Equals(variableValue, "false", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(variableValue, "0", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(variableValue, "no", StringComparison.OrdinalIgnoreCase);

            // Store the result in workflow context for potential use
            string result;
            if (isTrue)
            {
                result = TrueOutcome;
            }
            else if (isFalse)
            {
                result = FalseOutcome;
            }
            else
            {
                result = variableValue;
            }
            
            workflowContext.Properties[$"{Name}_Result"] = result;
            workflowContext.Properties[$"{Name}_Value"] = variableValue;

            if (isTrue)
            {
                return Outcomes(TrueOutcome);
            }
            else if (isFalse)
            {
                return Outcomes(FalseOutcome);
            }
            else
            {
                // If not a boolean value, return based on whether it has any value
                string outcome = string.IsNullOrEmpty(variableValue) ? FalseOutcome : TrueOutcome;
                return Outcomes(outcome);
            }
        }
    }
}