using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Workflows;
using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Workflows.Services;
using OrchardCore.Queries;
using OrchardCore.Environment.Shell.Descriptor.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.ContentForEach
{
    public class ContentForEachTask : TaskActivity
    {
        readonly IStringLocalizer S;
        private readonly ISession _session;
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        private readonly IContentManager _contentManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IServiceProvider _serviceProvider;
        private int _currentPage = 0;

        public ContentForEachTask(IWorkflowScriptEvaluator scriptEvaluator, IContentManager contentManager, IStringLocalizer<ContentForEachTask> localizer, ISession session, ShellDescriptor shellDescriptor, IServiceProvider serviceProvider)
        {
            _scriptEvaluator = scriptEvaluator;
            _contentManager = contentManager;
            _session = session;
            S = localizer;
            _shellDescriptor = shellDescriptor;
            _serviceProvider = serviceProvider;
        }

        public override string Name => nameof(ContentForEachTask);

        public override LocalizedString DisplayText => S["Content For Each Task"];

        public override LocalizedString Category => S["Content"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Iterate"], S["Done"]);
        }
        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {

            if (UseQuery && !_shellDescriptor.Features.Any(feature => feature.Id == "OrchardCore.Queries"))
            {
                throw new InvalidOperationException($"The '{nameof(ContentForEachTask)}' can't process the query as the feature OrchardCore.Queries is not enabled");
            }

            if (Index >= ContentItems.Count && !await FetchNextBatchAsync())
            {
                return Outcomes("Done");
            }

            ProcessContentItem(workflowContext);
            return Outcomes("Iterate");
        }

        private async Task<bool> FetchNextBatchAsync()
        {
            //Have already looped once and there is no PageSize parameter so all results would have come with first query.
            if (_currentPage > 0 && PageSize == 0)
            {
                return false;
            }
            if (UseQuery)
            {
                ContentItems = await ExecContentQueryAsync();
            }
            else
            {
                await ExecuteContentTypeQueryAsync();
            }
            _currentPage++;
            Index = 0;
            return ContentItems.Count > 0;
        }

        private async Task<List<ContentItem>> ExecContentQueryAsync()
        {
            var _queryManager = (IQueryManager)_serviceProvider.GetService(typeof(IQueryManager));
            var contentItems = new List<ContentItem>();
            dynamic query = await _queryManager.GetQueryAsync(Query);
            if (query == null)
            {
                throw new InvalidOperationException(S[$"Failed to retrieve the query {Query} (Have you changed, deleted the query or disabled the feature?)"]);
            }
            var queryParameters = !string.IsNullOrEmpty(Parameters) ?
                JsonSerializer.Deserialize<Dictionary<string, object>>(Parameters)
                : new Dictionary<string, object>();

            if (PageSize > 0)
            {
                EnsureTemplateHasSizeAndFrom(query);
                queryParameters["from"] = _currentPage * PageSize;
                queryParameters["size"] = PageSize;
            }
            try
            {
                IQueryResults results = await _queryManager.ExecuteQueryAsync(query, queryParameters);
                foreach (ContentItem item in results.Items)
                {
                    contentItems.Add(item);
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(S[$"Failed to run the query {Query}, failed with message: {e.Message}."]);
            }
            return contentItems;
        }

        private void EnsureTemplateHasSizeAndFrom(dynamic query)
        {
            if (!((string)query.Template).Contains("from"))
            {
                throw new InvalidOperationException(S[@"Your query has no 'from' parameter, please update your query if you want to use take. I.e. 'from': { from | default: 0 } "]);
            }
            if (!((string)query.Template).Contains("size"))
            {
                throw new InvalidOperationException(S[@"Your query has no 'size' parameter, please update your query if you want to use take. I.e. 'take': { take | default: 10 }"]);
            }
        }

        private async Task ExecuteContentTypeQueryAsync()
        {
            ContentItems = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == ContentType)
                .Where(w => (w.Published || w.Published == PublishedOnly) && (w.Latest || w.Latest == !PublishedOnly))
                .Skip(_currentPage * PageSize)
                .Take(PageSize)
                .ListAsync() as List<ContentItem>;
        }
        private void ProcessContentItem(WorkflowExecutionContext workflowContext)
        {
            var contentItem = ContentItems[Index];
            Current = contentItem;
            workflowContext.CorrelationId = contentItem.ContentItemId;
            workflowContext.Properties[ContentEventConstants.ContentItemInputKey] = contentItem;
            workflowContext.LastResult = Current;
            Index++;
        }

        /// <summary>
        /// How many to take each db call.
        /// </summary>
        public int PageSize
        {
            get => GetProperty(() => 10);
            set => SetProperty(value);
        }
        /// <summary>
        /// The current number of iterations executed.
        /// </summary>
        public int Index
        {
            get => GetProperty(() => 0);
            set => SetProperty(value);
        }

        /// <summary>
        /// The current iteration value.
        /// </summary>
        public object Current
        {
            get => GetProperty<object>();
            set => SetProperty(value);
        }

        /// <summary>
        /// The collection of contentItems.
        /// </summary>
        public List<ContentItem> ContentItems
        {
            get => GetProperty(() => new List<ContentItem>());
            set => SetProperty(value);
        }
        /// <summary>
        /// The name of the content type to select.
        /// </summary>
        public string ContentType
        {
            get => GetProperty(() => string.Empty);
            set => SetProperty(value);
        }
        /// <summary>
        /// Toggles between using a query (I.e. Lucene or raw YesSql query).
        /// </summary>
        public bool UseQuery
        {
            get => GetProperty<bool>(() => false);
            set => SetProperty(value);
        }
        /// <summary>
        /// The selected query source, if any.
        /// </summary>
        public string QuerySource
        {
            get => GetProperty(() => string.Empty);
            set => SetProperty(value);
        }
        /// <summary>
        /// The name of the query to run.
        /// </summary>
        public string Query
        {
            get => GetProperty(() => string.Empty);
            set => SetProperty(value);
        }
        /// <summary>
        /// Parameters to pass into the query.
        /// </summary>
        public string Parameters
        {
            get => GetProperty(() => string.Empty);
            set => SetProperty(value);
        }
        /// <summary>
        /// Only return published items.
        /// </summary>
        public bool PublishedOnly
        {
            get => GetProperty<bool>(() => false);
            set => SetProperty(value);
        }
    }

}
