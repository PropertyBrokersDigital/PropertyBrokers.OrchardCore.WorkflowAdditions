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
    Id = Constants.Features.GoogleAnalytics,
    Name = "Google Analytics Workflow",
    Description = "Adds Google Analytics integration to workflows.",
    Category = "Analytics"
)]