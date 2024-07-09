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
using System.Text;
using Microsoft.AspNetCore.Http;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleTagManager
{
    public class GoogleTagManagerTask : TaskActivity
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStringLocalizer S;
        private readonly HttpClient _httpClient;

        public GoogleTagManagerTask(
            IWorkflowExpressionEvaluator expressionEvaluator,
            IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<GoogleTagManagerTask> localizer,
            HttpClient httpClient)
        {
            _expressionEvaluator = expressionEvaluator;
            _httpContextAccessor = httpContextAccessor;
            S = localizer;
            _httpClient = httpClient;
        }

        public override string Name => nameof(GoogleTagManagerTask);
        public override LocalizedString DisplayText => S["Google Tag Manager Task"];
        public override LocalizedString Category => S["Analytics"];

        public WorkflowExpression<string> ContainerId
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> EventExpression
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> DataLayerExpression
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
            var containerId = await _expressionEvaluator.EvaluateAsync(ContainerId, workflowContext, null);
            var eventExpression = await _expressionEvaluator.EvaluateAsync(EventExpression, workflowContext, null);
            var dataLayerExpression = await _expressionEvaluator.EvaluateAsync(DataLayerExpression, workflowContext, null);

            if (string.IsNullOrEmpty(containerId))
            {
                workflowContext.LastResult = "Container ID is required";
                return Outcomes("Error");
            }

            if (string.IsNullOrEmpty(eventExpression))
            {
                workflowContext.LastResult = "Event Expression is required";
                return Outcomes("Error");
            }

            JObject dataLayer;
            try
            {
                var evaluatedDataLayer = await _expressionEvaluator.EvaluateAsync(new WorkflowExpression<object>(dataLayerExpression), workflowContext, null);
                dataLayer = JObject.FromObject(evaluatedDataLayer);
            }
            catch (Exception ex)
            {
                workflowContext.LastResult = $"Error evaluating data layer expression: {ex.Message}";
                return Outcomes("Error");
            }

            var gtmUrl = $"https://www.googletagmanager.com/gtag/js?id={containerId}";
            var dataLayerScript = $@"
                window.dataLayer = window.dataLayer || [];
                function gtag(){{dataLayer.push(arguments);}}
                gtag('js', new Date());
                gtag('config', '{containerId}');
                dataLayer.push({dataLayer.ToString(Newtonsoft.Json.Formatting.None)});
                gtag('event', '{eventExpression}', {dataLayer.ToString(Newtonsoft.Json.Formatting.None)});
            ";

            var htmlContent = $@"
                <html>
                <head>
                    <script async src='{gtmUrl}'></script>
                    <script>
                        {dataLayerScript}
                    </script>
                </head>
                <body>
                    <h1>GTM Event Fired</h1>
                    <p>Event: {eventExpression}</p>
                    <p>Data: {dataLayer.ToString(Newtonsoft.Json.Formatting.Indented)}</p>
                </body>
                </html>
            ";

            try
            {
                var content = new StringContent(htmlContent, Encoding.UTF8, "text/html");
                var response = await _httpClient.PostAsync("https://www.googletagmanager.com/gtag/js", content);
                response.EnsureSuccessStatusCode();
                workflowContext.LastResult = $"GTM event '{eventExpression}' sent successfully with data: {dataLayer}";
                return Outcomes("Done");
            }
            catch (Exception ex)
            {
                workflowContext.LastResult = $"Error sending GTM event: {ex.Message}";
                return Outcomes("Error");
            }
        }
    }
}