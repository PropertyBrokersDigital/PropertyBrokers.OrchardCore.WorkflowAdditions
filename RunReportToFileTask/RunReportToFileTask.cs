using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using PropertyBrokers.OrchardCore.WorkflowAdditions.Abstractions;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.RunReportToFile
{
    /// <summary>
    /// Renders a report content item with JSON data and saves the file to the media library.
    /// The rendering is done by an <see cref="IReportFileRenderer"/> that the host application registers.
    /// </summary>
    public class RunReportToFileTask : TaskActivity
    {
        public const string OutputPropertyName = "RunReportToFileTaskOutput";

        /// <summary>The largest JSON data, in UTF-8 bytes, the task passes to the renderer.</summary>
        public const int MaxJsonBytes = 5 * 1024 * 1024;

        private const string DoneOutcome = "Done";
        private const string FailedOutcome = "Failed";
        private const string PdfContentType = "application/pdf";
        private const string PdfExtension = ".pdf";

        private readonly IMediaFileStore _mediaFileStore;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IMediaNameNormalizerService _mediaNameNormalizerService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IStringLocalizer S;

        public RunReportToFileTask(
            IMediaFileStore mediaFileStore,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IMediaNameNormalizerService mediaNameNormalizerService,
            IServiceProvider serviceProvider,
            ILogger<RunReportToFileTask> logger,
            IStringLocalizer<RunReportToFileTask> localizer)
        {
            _mediaFileStore = mediaFileStore;
            _expressionEvaluator = expressionEvaluator;
            _mediaNameNormalizerService = mediaNameNormalizerService;
            // Resolved when the task runs, as the renderer is only registered by a host that can render reports.
            _serviceProvider = serviceProvider;
            _logger = logger;
            S = localizer;
        }

        public override string Name => nameof(RunReportToFileTask);
        public override LocalizedString DisplayText => S["Run report to file"];
        public override LocalizedString Category => S["Media"];

        public WorkflowExpression<string> ReportContentItemId
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> JsonData
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> MediaPath
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> FileName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public bool Overwrite
        {
            get => GetProperty(() => false);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            string reportContentItemId = null;
            try
            {
                // Inside the try so a Liquid error ends on Failed rather than faulting the workflow.
                var inputs = await EvaluateInputsAsync(workflowContext);
                reportContentItemId = inputs.ReportContentItemId;

                var inputError = ValidateInputs(inputs);
                if (inputError != null)
                {
                    return Fail(workflowContext, inputError);
                }

                var renderer = _serviceProvider.GetService<IReportFileRenderer>();
                if (renderer == null)
                {
                    return Fail(workflowContext, S["No report renderer is registered, so the report can't be run."].Value);
                }

                var rendered = await renderer.RenderAsync(inputs.ReportContentItemId, inputs.JsonData, ReportFileRenderOptions.Workflow);
                if (!rendered.Succeeded)
                {
                    return Fail(workflowContext, S["The report couldn't be run: {0}", rendered.ErrorMessage].Value);
                }

                if (rendered.Content == null || rendered.Content.Length == 0)
                {
                    return Fail(workflowContext, S["The report produced an empty file."].Value);
                }

                var fileName = ResolveFileName(inputs.FileName, rendered);
                var savedPath = await SaveToMediaAsync(NormalizeMediaPath(inputs.MediaPath), fileName, rendered.Content);

                workflowContext.LastResult = savedPath;
                workflowContext.Properties[OutputPropertyName] = savedPath;
                return Outcomes(DoneOutcome);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Run report to file failed for report {ReportContentItemId} in workflow {WorkflowId}.", reportContentItemId, workflowContext.WorkflowId);
                workflowContext.LastResult = S["Failed to run the report to a file: {0}", ex.Message].Value;
                return Outcomes(FailedOutcome);
            }
        }

        private async Task<TaskInputs> EvaluateInputsAsync(WorkflowExecutionContext workflowContext)
        {
            var reportContentItemId = await _expressionEvaluator.EvaluateAsync(ReportContentItemId, workflowContext, null);
            var jsonData = await _expressionEvaluator.EvaluateAsync(JsonData, workflowContext, null);
            var mediaPath = await _expressionEvaluator.EvaluateAsync(MediaPath, workflowContext, null);
            var fileName = await _expressionEvaluator.EvaluateAsync(FileName, workflowContext, null);

            return new TaskInputs(reportContentItemId?.Trim(), jsonData, mediaPath?.Trim(), fileName?.Trim());
        }

        private string ValidateInputs(TaskInputs inputs)
        {
            if (string.IsNullOrWhiteSpace(inputs.ReportContentItemId))
            {
                return S["A report content item ID is required."].Value;
            }

            if (string.IsNullOrWhiteSpace(inputs.MediaPath))
            {
                return S["A media path is required."].Value;
            }

            if (NormalizeMediaPath(inputs.MediaPath) == null)
            {
                return S["The media path must be a folder inside the media library: no leading /, no . or .. segments and no :."].Value;
            }

            if (!string.IsNullOrWhiteSpace(inputs.FileName) && !IsSafeFileName(inputs.FileName))
            {
                return S["The file name can't be . or .., contain :, or start with /."].Value;
            }

            if (inputs.JsonData != null && Encoding.UTF8.GetByteCount(inputs.JsonData) > MaxJsonBytes)
            {
                return S["The JSON data is larger than the 5 MB limit."].Value;
            }

            if (!IsJsonObject(inputs.JsonData))
            {
                return S["The JSON data must be a JSON object."].Value;
            }

            return null;
        }

        /// <summary>
        /// The media path with \ turned into /, or null when it could leave the folder it names:
        /// rooted, with a . or .. segment, or with a : (a drive or a scheme). The media store can be
        /// Azure Blob, where prefix/../x may resolve outside the tenant's base path.
        /// </summary>
        private static string NormalizeMediaPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var normalized = path.Replace('\\', '/').Trim();
            if (normalized.StartsWith('/') || normalized.Contains(':') || Path.IsPathRooted(normalized))
            {
                return null;
            }

            var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0 || segments.Any(IsDotSegment))
            {
                return null;
            }

            return string.Join('/', segments);
        }

        private static bool IsSafeFileName(string fileName)
        {
            var normalized = fileName.Replace('\\', '/').Trim();
            if (normalized.StartsWith('/') || normalized.Contains(':') || Path.IsPathRooted(normalized))
            {
                return false;
            }

            return !normalized.Split('/').Any(IsDotSegment);
        }

        private static bool IsDotSegment(string segment) => segment.Trim() is "." or "..";

        private static bool IsJsonObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(json);
                return document.RootElement.ValueKind == JsonValueKind.Object;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private string ResolveFileName(string fileName, ReportFileRenderResult rendered)
        {
            var name = string.IsNullOrWhiteSpace(fileName) ? UniqueDefaultName(rendered.FileName) : fileName;

            var extension = ExtensionFor(rendered.ContentType);
            if (extension != null && !name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                name += extension;
            }

            return _mediaNameNormalizerService.NormalizeFileName(name.Replace('/', ' ').Replace('\\', ' '));
        }

        // "<report name> <yyyyMMdd-HHmmss> <8 hex>", so two runs never collide and a name can't be guessed from the date.
        private static string UniqueDefaultName(string suggestedName)
        {
            var reportName = suggestedName ?? string.Empty;
            if (reportName.EndsWith(PdfExtension, StringComparison.OrdinalIgnoreCase))
            {
                reportName = reportName[..^PdfExtension.Length];
            }

            reportName = reportName.Replace(':', ' ').Replace('/', ' ').Replace('\\', ' ').Trim();
            if (reportName.Length == 0 || reportName.All(c => c == '.'))
            {
                reportName = "report";
            }

            return $"{reportName} {DateTime.UtcNow:yyyyMMdd-HHmmss} {Guid.NewGuid().ToString("N")[..8]}";
        }

        private static string ExtensionFor(string contentType)
        {
            // Reports render to PDF; an unknown type keeps whatever extension the file name has.
            if (string.IsNullOrWhiteSpace(contentType) || string.Equals(contentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
            {
                return PdfExtension;
            }

            return null;
        }

        private async Task<string> SaveToMediaAsync(string mediaPath, string fileName, byte[] content)
        {
            var filePath = _mediaFileStore.Combine(mediaPath, fileName);

            await using var stream = new MemoryStream(content);
            var savedPath = await _mediaFileStore.CreateFileFromStreamAsync(filePath, stream, Overwrite);

            // Media creating handlers can change the path, so prefer the one the store returns.
            return string.IsNullOrEmpty(savedPath) ? filePath : savedPath;
        }

        private ActivityExecutionResult Fail(WorkflowExecutionContext workflowContext, string message)
        {
            _logger.LogWarning("Run report to file failed in workflow {WorkflowId}: {Message}", workflowContext.WorkflowId, message);
            workflowContext.LastResult = message;
            return Outcomes(FailedOutcome);
        }

        private sealed record TaskInputs(string ReportContentItemId, string JsonData, string MediaPath, string FileName);
    }
}
