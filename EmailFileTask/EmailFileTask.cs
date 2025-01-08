using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

using OrchardCore.Email;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.EmailFile
{
    public class EmailFileTask : TaskActivity
    {
        private readonly IEmailService _emailService;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;
        private readonly HtmlEncoder _htmlEncoder;

        private static readonly HttpClient _httpClient = new HttpClient();

        public EmailFileTask(
            IEmailService emailService,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<EmailFileTask> localizer,
            HtmlEncoder htmlEncoder
        )
        {
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
            _htmlEncoder = htmlEncoder;
            _emailService = emailService;
        }

        public override string Name => nameof(EmailFileTask);
        public override LocalizedString DisplayText => S["Email Task"];
        public override LocalizedString Category => S["Messaging"];

        public WorkflowExpression<string> Author
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Sender
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ReplyTo
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        // TODO: Add support for the following format: Jack Bauer<jack@ctu.com>, ...
        public WorkflowExpression<string> Recipients
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Cc
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Bcc
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Subject
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Body
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> BodyText
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> AttachmentUrl
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }
        public bool SendWithNoAttachment
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public bool IsBodyHtml
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public bool IsBodyText
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var attachmentUrl = await _expressionEvaluator.EvaluateAsync(AttachmentUrl, workflowContext, null);
            var request = new HttpRequestMessage(new HttpMethod("GET"), attachmentUrl);
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            var attachments = new List<MailMessageAttachment>();

            // Email setup
            var author = await _expressionEvaluator.EvaluateAsync(Author, workflowContext, null);
            var sender = await _expressionEvaluator.EvaluateAsync(Sender, workflowContext, null);
            var replyTo = await _expressionEvaluator.EvaluateAsync(ReplyTo, workflowContext, null);
            var recipients = await _expressionEvaluator.EvaluateAsync(Recipients, workflowContext, null);
            var cc = await _expressionEvaluator.EvaluateAsync(Cc, workflowContext, null);
            var bcc = await _expressionEvaluator.EvaluateAsync(Bcc, workflowContext, null);
            var subject = await _expressionEvaluator.EvaluateAsync(Subject, workflowContext, null);
            var body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, IsBodyHtml ? _htmlEncoder : null);

            var message = new MailMessage
            {
                From = author?.Trim() ?? sender?.Trim(),
                To = recipients.Trim(),
                Cc = cc?.Trim(),
                Bcc = bcc?.Trim(),
                ReplyTo = replyTo?.Trim(),
                Subject = subject.Trim(),
                Body = body?.Trim(),
                IsHtmlBody = IsBodyHtml,
                Attachments = attachments
            };

            if (!String.IsNullOrWhiteSpace(sender))
            {
                message.Sender = sender.Trim();
            }

            // Handle attachment if response is successful
            if (response.IsSuccessStatusCode)
            {
                string fileName;

                // Try to get filename from Content-Disposition header
                if (response.Content.Headers.ContentDisposition != null)
                {
                    fileName = response.Content.Headers.ContentDisposition.FileName;
                }
                // If no Content-Disposition, extract filename from URL
                else
                {
                    fileName = Path.GetFileName(new Uri(attachmentUrl).LocalPath);

                    // If still no filename, generate one based on content type
                    if (string.IsNullOrEmpty(fileName))
                    {
                        var extension = response.Content.Headers.ContentType?.MediaType switch
                        {
                            "application/pdf" => ".pdf",
                            "image/jpeg" => ".jpg",
                            "image/png" => ".png",
                            _ => ".dat"
                        };
                        fileName = $"attachment_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    }
                }

                await using var ms = await response.Content.ReadAsStreamAsync();
                var attachment = new MailMessageAttachment
                {
                    Stream = ms,
                    Filename = fileName
                };
                attachments.Add(attachment);

                var result = await _emailService.SendAsync(message);
                workflowContext.LastResult = result;
                if (!result.Succeeded)
                {
                    return Outcomes("Failed");
                }
            }
            else if (SendWithNoAttachment)
            {
                var result = await _emailService.SendAsync(message);
                workflowContext.LastResult = result;
                if (!result.Succeeded)
                {
                    return Outcomes("Failed");
                }
            }

            return Outcomes("Done");
        }
    }
}
