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
using System.Net.Http.Headers;
using Parlot.Fluent;
using Fluid.Parser;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleAnalyticsEvent
{
    public class GoogleAnalyticsEventTask : TaskActivity
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly HttpClient _httpClient;
        private readonly IStringLocalizer S;
        private const string error = "Error";

        public override LocalizedString DisplayText => S["Google Analytics 4 Event"];
        public override LocalizedString Category => S["Analytics"];
        public GoogleAnalyticsEventTask(
            IWorkflowExpressionEvaluator expressionEvaluator,
            HttpClient httpClient,
            IStringLocalizer<GoogleAnalyticsEventTask> localizer)
        {
            _expressionEvaluator = expressionEvaluator;
            _httpClient = httpClient;
            S = localizer;
        }

        public override string Name => nameof(GoogleAnalyticsEvent);

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

        public WorkflowExpression<string> SessionId
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }
        
        public WorkflowExpression<string> ClientId
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }
        
        public WorkflowExpression<string> RequestTimeStamp
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);

        }public WorkflowExpression<string> EventTimeStamp
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
            return Outcomes(S["Done"], S[error]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            try
            {
                var measurementId = await _expressionEvaluator.EvaluateAsync(MeasurementId, workflowContext, null);
                var apiSecret = await _expressionEvaluator.EvaluateAsync(ApiSecret, workflowContext, null);
                var clientId = await _expressionEvaluator.EvaluateAsync(ClientId, workflowContext, null);
                var eventName = await _expressionEvaluator.EvaluateAsync(EventName, workflowContext, null);
                var event_timestamp = await _expressionEvaluator.EvaluateAsync(EventTimeStamp, workflowContext, null);
                var sessionId = await _expressionEvaluator.EvaluateAsync(SessionId, workflowContext, null);
                var request_timestamp = await _expressionEvaluator.EvaluateAsync(RequestTimeStamp, workflowContext, null);
                var eventParamsExpression = await _expressionEvaluator.EvaluateAsync(EventParamsExpression, workflowContext, null);

                if (string.IsNullOrEmpty(measurementId) || string.IsNullOrEmpty(apiSecret))
                {
                    workflowContext.LastResult = S["Measurement ID and API Secret are required"].Value;
                    return Outcomes(error);
                }

                if (string.IsNullOrEmpty(eventName))
                {
                    workflowContext.LastResult = S["Event Name is required"].Value;
                    return Outcomes(error);
                }

                if (string.IsNullOrEmpty(sessionId))
                {
                    sessionId = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(clientId))
                {
                    clientId = Guid.NewGuid().ToString();
                }

                var eventParams = JsonConvert.DeserializeObject<Dictionary<string, object>>(eventParamsExpression);
                if (!eventParams.ContainsKey("event"))
                {
                    eventParams["event"] = eventName;
                }

                var payload = new
                {
                    //Event Level, Page load
                    client_id = clientId,
                    timestamp_micros = event_timestamp,
                    events = new[]
                    {
                        new
                        {
                            //Request Level eg. form submitted
                            name = eventName,
                            timestamp_micros = request_timestamp,
                            session_id = sessionId,
                            @params = eventParams
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                
                var url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurementId}&api_secret={apiSecret}";
                
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    workflowContext.LastResult = S["Successfully sent event '{0}' to GA4. Status: {1}, Response: {2}", eventName, response.StatusCode, responseContent].Value;
                    return Outcomes("Done");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    workflowContext.LastResult = S["Failed to send event to GA4. Status: {0}, Error: {1}", response.StatusCode, errorMessage].Value;
                    return Outcomes(error);
                }
            }
            catch (Exception ex)
            {
                workflowContext.LastResult = S["Error: {0}", ex.Message].Value;
                return Outcomes(error);
            }
        }
    }
}