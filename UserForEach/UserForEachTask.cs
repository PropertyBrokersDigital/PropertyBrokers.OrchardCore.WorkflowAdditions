using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Users.Indexes;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.ContentManagement.Records;
using YesSql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Users.Models;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.UserForEach
{
    public class UserForEachTask : TaskActivity
    {
        readonly IStringLocalizer S;
        private readonly ISession _session;
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        private readonly IContentManager _contentManager;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private List<User> _users;

        public UserForEachTask(IWorkflowExpressionEvaluator expressionEvaluator, IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<RetrieveContentTask> localizer, ISession session)
        {
            S = localizer;
            _session = session;
            _scriptEvaluator = scriptEvaluator;
            _expressionEvaluator = expressionEvaluator;
            _contentManager = contentManager;
        }


        public override string Name => nameof(UserForEachTask);

        public override LocalizedString DisplayText => S["User For Each Task"];

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
            if (_users == null)
            {
                _users = (await _session
                .Query<User, UserIndex>(index => index.IsEnabled == true)
                .ListAsync()).ToList();
            }                
            if (_users.Count() == 0)
            {
                throw new InvalidOperationException($"The '{nameof(UserForEachTask)}' failed to find any users");
            }

            if (Index < _users.Count())
            {
                var user = _users[Index];
                var current = Current = user;
                workflowContext.CorrelationId = user.UserId;
                workflowContext.Properties["ContentItem"] = user;
                workflowContext.LastResult = current;
                Index++;
                return Outcomes("Iterate"); 
            }
            else
            {
                Index = 0;
                return Outcomes("Done");
            }
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
    }

}
