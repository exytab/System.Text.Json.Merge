using System.Text.Json.Nodes;

namespace System.Text.Json.Merge;

internal static class JsonValueExtensions
{
    internal static void MergeItem(this JsonValue @this, object content, JsonMergeSettings? settings)
    {
        JsonNode? value = content.ToJsonNode();

        if (value != null && value.GetValueKind() != JsonValueKind.Null)
        {
            switch (@this.Parent)
            {
                case JsonObject jsonObject:
                    jsonObject[@this.GetPropertyName()] = value.DeepClone();
                    return;
                case JsonArray jsonArray:
                    jsonArray[@this.GetElementIndex()] = value.DeepClone();
                    return;
            }
        }
    }

    internal static int GetHashCodeOfValue(this JsonValue @this)
    {
        if (@this.TryGetValue<IComparable>(out var comparable))
        {
            return comparable.GetHashCode();
        }

        return @this.GetHashCode();
    }
}
