using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Email = OrchardCore.Email;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using PropertyBrokers.OrchardCore.WorkflowAdditions.ContentForEach;
using PropertyBrokers.OrchardCore.WorkflowAdditions.EmailFile;
using PropertyBrokers.OrchardCore.WorkflowAdditions.UserForEach;
using PropertyBrokers.OrchardCore.WorkflowAdditions.MediaCachePurge;
using System;
using PropertyBrokers.OrchardCore.WorkflowAdditions.Display;
using PropertyBrokers.OrchardCore.WorkflowAdditions.ValidateJson;
using PropertyBrokers.OrchardCore.WorkflowAdditions.ProcessMjmlTemplate;
using PropertyBrokers.OrchardCore.WorkflowAdditions.GenerateSecureUrlToken.Drivers;
using PropertyBrokers.OrchardCore.WorkflowAdditions.GenerateSecureUrlToken.Services;
using PropertyBrokers.OrchardCore.WorkflowAdditions.GenerateSecureUrlToken;
using PropertyBrokers.OrchardCore.WorkflowAdditions.GoogleAnalyticsEvent;
using PropertyBrokers.OrchardCore.WorkflowAdditions.SaveFileToMedia;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions
{
    [Feature(Constants.Features.ContentForEach)]
    public class Startup: StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<ContentForEachTask, ContentForEachTaskDisplayDriver>();
            services.AddActivity<EmailFileTask, EmailFileTaskDisplayDriver>();
            services.AddActivity<UserForEachTask, UserForEachTaskDisplayDriver>();
            services.AddActivity<MediaCachePurgeTask, MediaPurgeTaskDisplayDriver>();
            services.AddActivity<ValidateJsonTask, ValidateJsonTaskDisplayDriver>();
            services.AddActivity<ProcessMjmlTemplateTask, ProcessMjmlTemplateTaskDisplayDriver>();
            services.AddActivity<GoogleAnalyticsEventTask, GoogleAnalyticsEventTaskDisplayDriver>();
            services.AddScoped<ISmtpService, SmtpService>();
            services.AddScoped<ISecureUrlTokenService, SecureUrlTokenService>();
            services.AddActivity<GenerateSecureUrlTokenTask, GenerateSecureUrlTokenTaskDisplayDriver>();
            services.AddActivity<SaveFileToMediaTask, SaveFileToMediaTaskDisplayDriver>();

        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
