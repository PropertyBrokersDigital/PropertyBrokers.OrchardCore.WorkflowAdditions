﻿using OrchardCore.Workflows.Display;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.ContentManagement.Metadata;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Queries;
using System.Collections.Generic;
using YesSql;
using OrchardCore.Environment.Shell.Descriptor.Models;
using System;


namespace PropertyBrokers.OrchardCore.WorkflowAdditions.ContentForEach
{


    public class ContentForEachTaskDisplayDriver: ActivityDisplayDriver<ContentForEachTask, ContentForEachTaskViewModel>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IServiceProvider _serviceProvider;
        public ContentForEachTaskDisplayDriver(IContentDefinitionManager contentDefinitionManager, ISession session, ShellDescriptor shellDescriptor, IServiceProvider serviceProvider)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _shellDescriptor = shellDescriptor;
            _serviceProvider = serviceProvider;
        }
        protected override async void EditActivity(ContentForEachTask activity, ContentForEachTaskViewModel model)
        {
            model.QueriesEnabled = _shellDescriptor.Features.Any(feature => feature.Id == "OrchardCore.Queries");
            if (model.QueriesEnabled)
            {
                var _queryManager = (IQueryManager)_serviceProvider.GetService(typeof(IQueryManager));
                model.UseQuery = activity.UseQuery;
                model.QuerySource = activity.QuerySource;
                model.Query = activity.Query;
                model.Parameters = activity.Parameters;
                var queries = await _queryManager.ListQueriesAsync();

                model.QuerySources = queries.Select(x => x.Source).Distinct()
                    .Select(x => new SelectListItem { Text = x, Value = x })
                    .ToList();

                model.QueriesBySource = queries.GroupBy(x => x.Source)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(q => new SelectListItem { Text = q.Name, Value = q.Name }).ToList());

            }
            else
            {
                model.UseQuery = false;
            }

            model.AvailableContentTypes = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
                .Select(x => new SelectListItem { Text = x.DisplayName, Value = x.Name })
                .ToList();
            model.ContentType = activity.ContentType;
            model.PageSize = activity.PageSize;
            model.PublishedOnly = activity.PublishedOnly;
        }

        protected override void UpdateActivity(ContentForEachTaskViewModel model, ContentForEachTask activity)
        {
            activity.UseQuery = model.UseQuery;
            activity.ContentType = model.ContentType;
            activity.Query = model.Query;
            activity.QuerySource = model.QuerySource;
            activity.Parameters = model.Parameters ?? string.Empty;
            activity.PageSize = model.PageSize;
            activity.PublishedOnly = model.PublishedOnly;
        }
    }

}
