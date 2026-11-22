# PropertyBrokers.OrchardCore.WorkflowAdditions
Workflow additions for Orchard Core
- Content for each, ability to loop through content items by type or using a lucene or elastic query. Good for updating items etc.
- User for each. Loop through all users. Good for bulk emails etc.
- Email file from an endpoint (Used for automated reporting etc)
- Run report to file. Renders a report content item with JSON data and saves the PDF to the media library. The host
  application provides the rendering by registering an `IReportFileRenderer` (`Abstractions/IReportFileRenderer.cs`);
  without one the activity ends on Failed.

