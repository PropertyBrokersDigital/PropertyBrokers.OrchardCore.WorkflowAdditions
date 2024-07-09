using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Json;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleAnalytics
{
    public class GoogleAnalyticsTask : TaskActivity
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;
        private readonly HttpClient _httpClient;

        public GoogleAnalyticsTask(
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<GoogleAnalyticsTask> localizer,
            HttpClient httpClient)
        {
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
            _httpClient = httpClient;
        }

        public override string Name => nameof(GoogleAnalyticsTask);
        public override LocalizedString DisplayText => S["Google Analytics Task"];
        public override LocalizedString Category => S["Analytics"];

        public WorkflowExpression<string> MeasurementId
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ApiSecret
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ClientId
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> EventName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> EventParameters
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Error"]);
        }

        //public override string GetTitleOrDefault(Func<LocalizedString> defaultTitle)
        //{
        //    return !string.IsNullOrWhiteSpace(EventName.Expression)
        //        ? S["Send GA Event: {0}", EventName.Expression].Value
        //        : defaultTitle().Value;
        //}

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var measurementId = await _expressionEvaluator.EvaluateAsync(MeasurementId, workflowContext, null);
            var apiSecret = await _expressionEvaluator.EvaluateAsync(ApiSecret, workflowContext, null);
            var clientId = await _expressionEvaluator.EvaluateAsync(ClientId, workflowContext, null);
            var eventName = await _expressionEvaluator.EvaluateAsync(EventName, workflowContext, null);
            var eventParametersContent = await _expressionEvaluator.EvaluateAsync(EventParameters, workflowContext, null);

            if (string.IsNullOrEmpty(measurementId) || string.IsNullOrEmpty(apiSecret) || string.IsNullOrEmpty(clientId))
            {
                workflowContext.LastResult = "Measurement ID, API Secret, and Client ID are required";
                return Outcomes("Error");
            }

            if (string.IsNullOrEmpty(eventName))
            {
                workflowContext.LastResult = "Event Name is required";
                return Outcomes("Error");
            }

            JObject eventParameters = new();
            if (!string.IsNullOrEmpty(eventParametersContent))
            {
                try
                {
                    eventParameters = JObject.Parse(eventParametersContent);
                }
                catch (Exception ex)
                {
                    workflowContext.LastResult = $"Error parsing event parameters: {ex.Message}";
                    return Outcomes("Error");
                }
            }

            var payload = new
            {
                client_id = clientId,
                events = new[]
                {
                    new
                    {
                        name = eventName,
                        @params = eventParameters
                    }
                }
            };

            var url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurementId}&api_secret={apiSecret}";

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, payload);
                response.EnsureSuccessStatusCode();
                workflowContext.LastResult = $"Event '{eventName}' sent successfully";
                return Outcomes("Done");
            }
            catch (Exception ex)
            {
                workflowContext.LastResult = $"Error sending event: {ex.Message}";
                return Outcomes("Error");
            }
        }
    }
}
