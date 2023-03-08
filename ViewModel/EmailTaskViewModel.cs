using System.ComponentModel.DataAnnotations;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.EmailFile
{
    public class EmailFileTaskViewModel
    {
        public string AuthorExpression { get; set; }

        public string SenderExpression { get; set; }

        public string ReplyToExpression { get; set; }

        public string CcExpression { get; set; }

        public string BccExpression { get; set; }

        [Required]
        public string RecipientsExpression { get; set; }

        public string SubjectExpression { get; set; }
        public string Body { get; set; }
        public string AttachmentUrl { get; set; }
        public string BodyText { get; set; }
        public bool IsBodyHtml { get; set; }
        public bool IsBodyText { get; set; }
        public bool SendWithNoAttachment { get; set; }
    }
}
