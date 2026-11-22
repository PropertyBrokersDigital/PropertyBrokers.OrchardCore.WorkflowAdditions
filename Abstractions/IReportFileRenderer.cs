using System.Threading;
using System.Threading.Tasks;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.Abstractions
{
    /// <summary>
    /// Renders a report content item to a file, filling it with JSON data.
    /// This module can't reference the reporting engine, so the host application implements this and registers it.
    /// When nothing is registered, the "Run report to file" activity ends on its Failed outcome.
    /// </summary>
    public interface IReportFileRenderer
    {
        /// <summary>
        /// Renders the report.
        /// </summary>
        /// <param name="reportContentItemId">The content item id of the report.</param>
        /// <param name="jsonData">A JSON object used as the report's data source.</param>
        /// <param name="options">Who is asking; see <see cref="ReportFileRenderOptions"/>.</param>
        /// <param name="cancellationToken">Cancels the render.</param>
        /// <returns>The rendered file, or a failed result saying why it couldn't be rendered.</returns>
        Task<ReportFileRenderResult> RenderAsync(
            string reportContentItemId,
            string jsonData,
            ReportFileRenderOptions options,
            CancellationToken cancellationToken = default);
    }
}
