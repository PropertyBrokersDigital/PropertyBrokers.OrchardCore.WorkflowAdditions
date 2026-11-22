namespace PropertyBrokers.OrchardCore.WorkflowAdditions.Abstractions
{
    /// <summary>
    /// Tells an <see cref="IReportFileRenderer"/> who is asking, so it can apply stricter rules where needed.
    /// </summary>
    public sealed class ReportFileRenderOptions
    {
        /// <summary>
        /// The "Run report to file" workflow activity, whose JSON data is often built from submitted forms.
        /// Hosts should refuse reports that query a database of their own on this path.
        /// </summary>
        public static ReportFileRenderOptions Workflow { get; } = new() { CalledFromWorkflow = true };

        /// <summary>True when a workflow activity is the caller.</summary>
        public bool CalledFromWorkflow { get; init; }
    }
}
