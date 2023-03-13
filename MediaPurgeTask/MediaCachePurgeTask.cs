using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Media;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.MediaCachePurge
{
    public class MediaCachePurgeTask : TaskActivity
    {
        private readonly IStringLocalizer S;
        private readonly IMediaFileStoreCache _mediaFileStoreCache;

        public MediaCachePurgeTask(
            IStringLocalizer<MediaCachePurgeTask> localizer,
            IServiceProvider serviceProvider
        )
        {
            // Resolve from service provider as the service will not be registered if configuration is invalid.
            _mediaFileStoreCache = serviceProvider.GetService<IMediaFileStoreCache>();
            S = localizer;
        }

        public override string Name => nameof(MediaCachePurgeTask);
        public override LocalizedString DisplayText => S["Media Purge Task"];
        public override LocalizedString Category => S["Media"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            if (_mediaFileStoreCache == null)
            {
                return Outcomes("Failed");
            }
            var hasErrors = await _mediaFileStoreCache.PurgeAsync();
            if (hasErrors)
            {
                return Outcomes("Done");
            }
            else
            {
                return Outcomes("Done");
            }
        }
    }
}
