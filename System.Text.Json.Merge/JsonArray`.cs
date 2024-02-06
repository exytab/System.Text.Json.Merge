using System.Collections;
using System.Text.Json.Nodes;

namespace System.Text.Json.Merge;

internal static class JsonArrayExtensions
{
    internal static void MergeItem(this JsonArray @this, object content, JsonMergeSettings? settings)
    {
        IEnumerable? a = (JsonNodeExtensions.IsMultiContent(content) || content is JsonArray)
            ? (IEnumerable)content
            : null;
        if (a == null)
        {
            return;
        }

        JsonNodeExtensions.MergeEnumerableContent(@this, a, settings);
    }
}
