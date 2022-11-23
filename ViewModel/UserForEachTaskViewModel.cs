using Microsoft.AspNetCore.Http;
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
using OrchardCore.ContentManagement.Workflows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Contents.Workflows.Activities;
using System.ComponentModel.DataAnnotations;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.UserForEach
{

    public class UserForEachTaskViewModel
    {
        [Required]
        public string Name { get; set; }
    }

}
