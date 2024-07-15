using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using PropertyBrokers.OrchardCore.WorkflowAdditions.GenerateSecureUrlToken.Services;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GenerateSecureUrlToken
{
    public class GenerateSecureUrlTokenTask : TaskActivity
    {
        private readonly ISecureUrlTokenService _tokenService;
        private readonly IStringLocalizer S;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public GenerateSecureUrlTokenTask(IWorkflowExpressionEvaluator expressionEvaluator, ISecureUrlTokenService tokenService, IStringLocalizer<GenerateSecureUrlTokenTask> localizer)
        {
            _tokenService = tokenService;
            S = localizer;
            _expressionEvaluator = expressionEvaluator;
        }

        public override string Name => nameof(GenerateSecureUrlTokenTask);
        public override LocalizedString DisplayText => S["Generate Secure URL Token"];
        public override LocalizedString Category => S["Security"];

        public WorkflowExpression<string> Email
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<int> ValidityInHours
        {
            get => GetProperty(() => new WorkflowExpression<int>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var email = await _expressionEvaluator.EvaluateAsync(Email, workflowContext, null);
            var validityHours = await _expressionEvaluator.EvaluateAsync(ValidityInHours, workflowContext, null);
            var token = _tokenService.GenerateToken(email, TimeSpan.FromHours(validityHours));

            workflowContext.Properties["SecureUrlToken"] = token;

            return Outcomes("Done");
        }
    }
}