namespace PropertyBrokers.OrchardCore.WorkflowAdditions.Abstractions
{
    /// <summary>
    /// The outcome of <see cref="IReportFileRenderer.RenderAsync"/>.
    /// </summary>
    public sealed class ReportFileRenderResult
    {
        private ReportFileRenderResult(bool succeeded, byte[] content, string contentType, string fileName, string errorMessage)
        {
            Succeeded = succeeded;
            Content = content;
            ContentType = contentType;
            FileName = fileName;
            ErrorMessage = errorMessage;
        }

        public bool Succeeded { get; }

        /// <summary>The rendered file's bytes.</summary>
        public byte[] Content { get; }

        /// <summary>The rendered file's media type, e.g. "application/pdf".</summary>
        public string ContentType { get; }

        /// <summary>A suggested file name (e.g. the report's title plus ".pdf"), used when the activity has none.</summary>
        public string FileName { get; }

        /// <summary>Why the report couldn't be rendered, when <see cref="Succeeded"/> is false.</summary>
        public string ErrorMessage { get; }

        public static ReportFileRenderResult Success(byte[] content, string contentType, string fileName)
            => new(true, content, contentType, fileName, null);

        public static ReportFileRenderResult Failure(string errorMessage)
            => new(false, null, null, null, errorMessage);
    }
}
