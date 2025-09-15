using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using PropertyBrokers.OrchardCore.WorkflowAdditions.Services;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.AzureAISearchTask
{
    public class AzureAISearchIndexTask : TaskActivity
    {
        private readonly IAzureAISearchService _azureAISearchService;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;

        public AzureAISearchIndexTask(
            IAzureAISearchService azureAISearchService,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<AzureAISearchIndexTask> localizer
        )
        {
            _azureAISearchService = azureAISearchService;
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
        }

        public override string Name => nameof(AzureAISearchIndexTask);
        public override LocalizedString DisplayText => S["Azure AI Search Index Task"];
        public override LocalizedString Category => S["Search"];

        public WorkflowExpression<string> SearchServiceUrl
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ApiKey
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> IndexName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> DocumentId
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> JsonPayload
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public bool CreateIndexIfNotExists
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Success"], S["ValidationFailed"], S["IndexCreated"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            try
            {
                var searchServiceUrl = await _expressionEvaluator.EvaluateAsync(SearchServiceUrl, workflowContext, null);
                var apiKey = await _expressionEvaluator.EvaluateAsync(ApiKey, workflowContext, null);
                var indexName = await _expressionEvaluator.EvaluateAsync(IndexName, workflowContext, null);
                var documentId = await _expressionEvaluator.EvaluateAsync(DocumentId, workflowContext, null);
                var jsonPayload = await _expressionEvaluator.EvaluateAsync(JsonPayload, workflowContext, null);

                var urlValidation = AzureAISearchIndexValidator.ValidateSearchServiceUrl(searchServiceUrl);
                if (!urlValidation.IsValid)
                {
                    workflowContext.LastResult = new { Error = urlValidation.ErrorMessage };
                    return Outcomes("ValidationFailed");
                }

                var apiKeyValidation = AzureAISearchIndexValidator.ValidateApiKey(apiKey);
                if (!apiKeyValidation.IsValid)
                {
                    workflowContext.LastResult = new { Error = apiKeyValidation.ErrorMessage };
                    return Outcomes("ValidationFailed");
                }

                var indexNameValidation = AzureAISearchIndexValidator.ValidateIndexName(indexName);
                if (!indexNameValidation.IsValid)
                {
                    workflowContext.LastResult = new { Error = indexNameValidation.ErrorMessage };
                    return Outcomes("ValidationFailed");
                }

                var jsonValidation = AzureAISearchIndexValidator.ValidateJsonPayload(jsonPayload);
                if (!jsonValidation.IsValid)
                {
                    workflowContext.LastResult = new { Error = jsonValidation.ErrorMessage };
                    return Outcomes("ValidationFailed");
                }

                bool indexExists = await _azureAISearchService.IndexExistsAsync(searchServiceUrl, apiKey, indexName);
                bool indexCreated = false;

                if (!indexExists)
                {
                    if (CreateIndexIfNotExists)
                    {
                        var createResult = await _azureAISearchService.CreateIndexAsync(searchServiceUrl, apiKey, indexName);
                        if (!createResult.IsSuccess)
                        {
                            workflowContext.LastResult = new { Error = createResult.ErrorMessage };
                            return Outcomes("Failed");
                        }
                        indexCreated = true;
                    }
                    else
                    {
                        workflowContext.LastResult = new { Error = $"Index '{indexName}' does not exist and CreateIndexIfNotExists is disabled." };
                        return Outcomes("Failed");
                    }
                }

                var result = await _azureAISearchService.IndexDocumentAsync(searchServiceUrl, apiKey, indexName, documentId, jsonPayload);

                if (result.IsSuccess)
                {
                    workflowContext.LastResult = new 
                    { 
                        DocumentKey = result.DocumentKey, 
                        IndexCreated = indexCreated,
                        IndexName = indexName
                    };

                    return indexCreated ? Outcomes("IndexCreated") : Outcomes("Success");
                }
                else
                {
                    workflowContext.LastResult = new { Error = result.ErrorMessage };
                    return Outcomes("Failed");
                }
            }
            catch (Exception ex)
            {
                workflowContext.LastResult = new { Error = ex.Message };
                return Outcomes("Failed");
            }
        }
    }
}