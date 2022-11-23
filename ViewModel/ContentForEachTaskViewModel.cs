using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.ContentForEach
{

    public class ContentForEachTaskViewModel
    {
        public bool QueriesEnabled { get; set; }
        public bool UseQuery { get; set; }
        public string ContentType { get; set; }
        public string Query { get; set; }
        public string Parameters { get; set; }
        public bool PublishedOnly { get; set; }
        public int Take { get; set; }

        [BindNever]
        public IList<SelectListItem> AvailableContentTypes { get; set; }

        [BindNever]
        public IList<SelectListItem> Queries { get; set; }
    }

}
