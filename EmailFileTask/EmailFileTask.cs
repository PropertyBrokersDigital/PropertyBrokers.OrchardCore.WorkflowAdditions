using System;
using System.Collections.Generic;
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
        private readonly ISmtpService _smtpService;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;
        private readonly HtmlEncoder _htmlEncoder;
        private static readonly HttpClient _httpClient = new HttpClient();

        public EmailFileTask(
            ISmtpService smtpService,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IStringLocalizer<EmailFileTask> localizer,
            HtmlEncoder htmlEncoder
        )
        {
            _smtpService = smtpService;
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
            _htmlEncoder = htmlEncoder;
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
              var response =  await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            var attachments = new List<MailMessageAttachment>();

            var author = await _expressionEvaluator.EvaluateAsync(Author, workflowContext, null);
            var sender = await _expressionEvaluator.EvaluateAsync(Sender, workflowContext, null);
            var replyTo = await _expressionEvaluator.EvaluateAsync(ReplyTo, workflowContext, null);
            var recipients = await _expressionEvaluator.EvaluateAsync(Recipients, workflowContext, null);
            var cc = await _expressionEvaluator.EvaluateAsync(Cc, workflowContext, null);
            var bcc = await _expressionEvaluator.EvaluateAsync(Bcc, workflowContext, null);
            var subject = await _expressionEvaluator.EvaluateAsync(Subject, workflowContext, null);
            var body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, _htmlEncoder);
            var bodyText = await _expressionEvaluator.EvaluateAsync(BodyText, workflowContext, null);
            var message = new MailMessage
            {
                // Author and Sender are both not required fields.
                From = author?.Trim() ?? sender?.Trim(),
                To = recipients.Trim(),
                Cc = cc?.Trim(),
                Bcc = bcc?.Trim(),
                // Email reply-to header https://tools.ietf.org/html/rfc4021#section-2.1.4
                ReplyTo = replyTo?.Trim(),
                Subject = subject.Trim(),
                Body = body?.Trim(),
                BodyText = bodyText?.Trim(),
                IsBodyHtml = IsBodyHtml,
                IsBodyText = IsBodyText
            };
            if (!String.IsNullOrWhiteSpace(sender))
            {
                message.Sender = sender.Trim();
            }
            
            if (response.Content.Headers.ContentDisposition != null)
            {
                var fileName = response.Content.Headers.ContentDisposition.FileName;
                response.EnsureSuccessStatusCode();
                await using var ms = await response.Content.ReadAsStreamAsync();
                var attachment = new MailMessageAttachment();
                attachment.Stream = ms;
                attachment.Filename = fileName;
                attachments.Add(attachment);
                message.Attachments = attachments;
                
                var result = await _smtpService.SendAsync(message);
                workflowContext.LastResult = result;

                if (!result.Succeeded)
                {
                    return Outcomes("Failed");
                }
            }
            else if(SendWithNoAttachment)
            {
                var result = await _smtpService.SendAsync(message);
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
