using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.SaveFileToMedia
{
    public class SaveToMediaTask : TaskActivity
    {
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IMediaNameNormalizerService _mediaNameNormalizerService;
        private readonly IStringLocalizer S;
        private static readonly HttpClient _httpClient = new HttpClient();

        public SaveToMediaTask(
            IMediaFileStore mediaFileStore,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IMediaNameNormalizerService mediaNameNormalizerService,
            IStringLocalizer<SaveToMediaTask> localizer)
        {
            _mediaFileStore = mediaFileStore;
            _expressionEvaluator = expressionEvaluator;
            _mediaNameNormalizerService = mediaNameNormalizerService;
            S = localizer;
        }

        public override string Name => nameof(SaveToMediaTask);
        public override LocalizedString DisplayText => S["Save To Media Task"];
        public override LocalizedString Category => S["Media"];

        public WorkflowExpression<string> FileUrl
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> MediaPath
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var fileUrl = await _expressionEvaluator.EvaluateAsync(FileUrl, workflowContext, null);
            var mediaPath = await _expressionEvaluator.EvaluateAsync(MediaPath, workflowContext, null);

            if (string.IsNullOrWhiteSpace(fileUrl) || string.IsNullOrWhiteSpace(mediaPath))
            {
                workflowContext.LastResult = "Invalid file URL or media path";
                return Outcomes("Failed");
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, fileUrl);
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode();

                if (response.Content.Headers.ContentDisposition != null)
                {
                    var fileName = response.Content.Headers.ContentDisposition.FileName?.Trim('"');
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = Path.GetFileName(fileUrl);
                    }

                    var normalisedFileName = _mediaNameNormalizerService.NormalizeFileName(fileName.Replace("/", " "));
                    var filePath = _mediaFileStore.Combine(mediaPath, normalisedFileName);

                    await using var stream = await response.Content.ReadAsStreamAsync();
                    await _mediaFileStore.CreateFileFromStreamAsync(filePath, stream);

                    workflowContext.LastResult = $"File saved successfully: {filePath}";
                    return Outcomes("Done");
                }
                workflowContext.LastResult = "No file content and SaveWithNoFile is false";
                return Outcomes("Failed");
            }
            catch (Exception ex)
            {
                workflowContext.LastResult = $"Failed to save file: {ex.Message}";
                return Outcomes("Failed");
            }
        }
    }
}