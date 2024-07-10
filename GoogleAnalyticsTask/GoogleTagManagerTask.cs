using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using Microsoft.Extensions.Localization;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleTagManager
{
    public class GoogleTagManagerTask : TaskActivity
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly HttpClient _httpClient;
        private readonly IStringLocalizer S;

        public override LocalizedString DisplayText => S["Google Analytics 4 Event"];
        public override LocalizedString Category => S["Analytics"];

        public GoogleTagManagerTask(
            IWorkflowExpressionEvaluator expressionEvaluator,
            HttpClient httpClient,
            IStringLocalizer<GoogleTagManagerTask> localizer)
        {
            _expressionEvaluator = expressionEvaluator;
            _httpClient = httpClient;
            S = localizer;
        }

        public override string Name => nameof(GoogleTagManagerTask);

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

        public WorkflowExpression<string> EventParamsExpression
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Error"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            try
            {
                var measurementId = await _expressionEvaluator.EvaluateAsync(MeasurementId, workflowContext, null);
                var apiSecret = await _expressionEvaluator.EvaluateAsync(ApiSecret, workflowContext, null);
                var clientId = await _expressionEvaluator.EvaluateAsync(ClientId, workflowContext, null);
                var eventName = await _expressionEvaluator.EvaluateAsync(EventName, workflowContext, null);
                var eventParamsExpression = await _expressionEvaluator.EvaluateAsync(EventParamsExpression, workflowContext, null);

                if (string.IsNullOrEmpty(measurementId) || string.IsNullOrEmpty(apiSecret))
                {
                    workflowContext.LastResult = S["Measurement ID and API Secret are required"].Value;
                    return Outcomes("Error");
                }

                if (string.IsNullOrEmpty(eventName))
                {
                    workflowContext.LastResult = S["Event Name is required"].Value;
                    return Outcomes("Error");
                }

                if (string.IsNullOrEmpty(clientId))
                {
                    clientId = Guid.NewGuid().ToString();
                }

                var eventParams = !string.IsNullOrEmpty(eventParamsExpression)
                    ? await _expressionEvaluator.EvaluateAsync(new WorkflowExpression<object>(eventParamsExpression), workflowContext, null)
                    : new object();

                var payload = new
                {
                    client_id = clientId,
                    events = new[]
                    {
                        new
                        {
                            name = eventName,
                            @params = eventParams
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurementId}&api_secret={apiSecret}";

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    workflowContext.LastResult = S["Successfully sent event '{0}' to GA4", eventName].Value;
                    return Outcomes("Done");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    workflowContext.LastResult = S["Failed to send event to GA4: {0}", response.StatusCode].Value;
                    return Outcomes("Error");
                }
            }
            catch (Exception ex)
            {
                workflowContext.LastResult = S["Error: {0}", ex.Message].Value;
                return Outcomes("Error");
            }
        }
    }
}