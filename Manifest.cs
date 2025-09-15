using PropertyBrokers.OrchardCore.WorkflowAdditions;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Author = "Mike Williamson",
    Category = "Workflows",
    Description = "Provides workflow additions",
    Name = "PropertyBrokers workflow additions",
    Version = "0.2.0"
    )]

[assembly: Feature(
    Id = Constants.Features.ContentForEach,
    Name = "Content foreach",
    Category = "Workflows",
    Dependencies = new[] { "OrchardCore.Workflows", "OrchardCore.Contents" },
    Description = "Allows looping through content (I.e. user registrations)"
    )]
[assembly: Feature(
    Id = Constants.Features.EmailFile,
    Name = "Email File",
    Category = "Messaging",
    Dependencies = new[] { "OrchardCore.Workflows", "OrchardCore.Email" },
    Description = "Allows specifying a file attachment"
    )]
[assembly: Feature(
    Id = Constants.Features.UsersForEach,
    Name = "User for each",
    Category = "Workflows",
    Dependencies = new[] { "OrchardCore.Workflows", "OrchardCore.Contents" },
    Description = "Loop through users"
    )]
[assembly: Feature(
    Id = Constants.Features.MediaCachePurge,
    Name = "Media cache purge",
    Category = "Workflows",
    Dependencies = new[] { "OrchardCore.Workflows", "OrchardCore.Media" },
    Description = "Clear the media image cache"
    )]
[assembly: Feature(
    Id = Constants.Features.ValidateJson,
    Name = "Validate JSON",
    Category = "Workflows",
    Dependencies = new[] { "OrchardCore.Workflows" },
    Description = "Validates JSON given a Schema"
    )]
[assembly: Feature(
    Id = Constants.Features.ProcessMjmlTemplate,
    Name = "Process MJML Template",
    Category = "Workflows",
    Dependencies = new[] { "OrchardCore.Workflows" },
    Description = "Processes an MJML template, merge tags can be optionally included"
    )]
[assembly: Feature(
    Id = Constants.Features.GenerateSecureUrlToken,
    Name = "Generate secure URL",
    Category = "Workflows",
    Dependencies = new[] { "OrchardCore.Workflows" },
    Description = "Creates a hash for a secure URL"
    )]
[assembly: Feature(
    Id = Constants.Features.SaveFileToMedia,
    Name = "Save file to media",
    Category = "Workflows",
    Dependencies = new[] { "OrchardCore.Workflows" },
    Description = "Saves a file from a url to media"
    )]
[assembly: Feature(
    Id = Constants.Features.GoogleTagManager,
    Name = "Google Tag Manager Workflow",
    Description = "Adds Google Tag Manager integration to workflows.",
    Category = "Analytics"
)]
[assembly: Feature(
    Id = Constants.Features.AzureAISearchIndex,
    Name = "Azure AI Search Index",
    Category = "Workflows",
    Dependencies = new[] { "OrchardCore.Workflows" },
    Description = "Index data to Azure AI Search service"
)]
