using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Scripting;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.TaxonomyTerm
{
    /// <summary>
    /// Exposes taxonomy term lookups to workflow JavaScript expressions:
    /// <c>getTaxonomyTerm(taxonomyId, termId)</c> returns the term
    /// <see cref="ContentItem"/> (read <c>.DisplayText</c>), and
    /// <c>getInheritedTaxonomyTerms(taxonomyId, termId)</c> returns the term plus its
    /// ancestors ordered <c>[term, parent, ..., root]</c> (array length = depth).
    ///
    /// Terms live embedded in their parent taxonomy's single content item, which for
    /// large taxonomies (e.g. Region/City/Suburb) is an expensive blob to load and
    /// deserialize. So the flattened term index is built once per taxonomy and cached
    /// in <see cref="IMemoryCache"/> - a bulk email send resolves every term via an
    /// in-memory dictionary lookup instead of reloading the taxonomy per call.
    /// </summary>
    public class TaxonomyGlobalMethodProvider : IGlobalMethodProvider
    {
        private const string CacheKeyPrefix = "WorkflowAdditions:TaxonomyTermIndex:";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

        private readonly GlobalMethod _getTaxonomyTerm;
        private readonly GlobalMethod _getInheritedTaxonomyTerms;

        public TaxonomyGlobalMethodProvider()
        {
            _getTaxonomyTerm = new GlobalMethod
            {
                Name = "getTaxonomyTerm",
                Method = serviceProvider => (Func<string, string, object>)((taxonomyId, termId) =>
                {
                    var index = GetIndex(serviceProvider, taxonomyId);
                    if (index != null && termId != null && index.TryGetValue(termId, out var chain))
                    {
                        return chain[0];
                    }

                    return null;
                }),
            };

            // Returns the term plus its ancestors, ordered [term, parent, ..., root].
            // The array length is the term's depth: 1 = region, 2 = town/city, 3 = suburb.
            _getInheritedTaxonomyTerms = new GlobalMethod
            {
                Name = "getInheritedTaxonomyTerms",
                Method = serviceProvider => (Func<string, string, object>)((taxonomyId, termId) =>
                {
                    var index = GetIndex(serviceProvider, taxonomyId);
                    if (index != null && termId != null && index.TryGetValue(termId, out var chain))
                    {
                        return chain;
                    }

                    return Array.Empty<ContentItem>();
                }),
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _getTaxonomyTerm, _getInheritedTaxonomyTerms };
        }

        // Returns (and caches) the flattened termId -> ancestry-chain index for a
        // taxonomy. The chain arrays share the same stripped ContentItem instances,
        // so the index stays memory-light regardless of tree depth.
        private static Dictionary<string, ContentItem[]> GetIndex(IServiceProvider serviceProvider, string taxonomyId)
        {
            if (string.IsNullOrEmpty(taxonomyId))
            {
                return null;
            }

            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

            return memoryCache.GetOrCreate(CacheKeyPrefix + taxonomyId, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                var contentManager = serviceProvider.GetRequiredService<IContentManager>();
                var taxonomy = contentManager.GetAsync(taxonomyId).GetAwaiter().GetResult();

                var index = new Dictionary<string, ContentItem[]>();

                // ContentElement.Content is a JsonDynamicObject, so Terms is a
                // JsonDynamicArray wrapper - an `is JsonArray` test fails against it.
                // Cast (like OC's own TaxonomyOrchardHelperExtensions) to unwrap it.
                var terms = (JsonArray)(taxonomy?.Content.TaxonomyPart?.Terms);

                if (terms != null)
                {
                    BuildIndex(terms, Array.Empty<ContentItem>(), index);
                }

                return index;
            });
        }

        // Walks the term tree once, mapping each term id to its ancestry chain
        // [self, parent, ..., root]. Each term is cloned without its child Terms so the
        // cached ContentItem carries only the term's own fields (DisplayText etc.).
        private static void BuildIndex(JsonArray termsArray, ContentItem[] ancestors, Dictionary<string, ContentItem[]> index)
        {
            foreach (var term in termsArray.Cast<JsonObject>())
            {
                var id = term["ContentItemId"]?.ToString();
                if (id == null)
                {
                    continue;
                }

                var chain = new ContentItem[ancestors.Length + 1];
                chain[0] = StripChildren(term);
                Array.Copy(ancestors, 0, chain, 1, ancestors.Length);
                index[id] = chain;

                if (term["Terms"] is JsonArray children)
                {
                    BuildIndex(children, chain, index);
                }
            }
        }

        // Copies a term node's own properties (excluding the nested Terms subtree) into
        // a fresh ContentItem, so cached chains don't retain the whole taxonomy per node.
        private static ContentItem StripChildren(JsonObject term)
        {
            var stripped = new JsonObject();

            foreach (var property in term)
            {
                if (property.Key == "Terms")
                {
                    continue;
                }

                stripped[property.Key] = property.Value?.DeepClone();
            }

            return stripped.ToObject<ContentItem>();
        }
    }
}
