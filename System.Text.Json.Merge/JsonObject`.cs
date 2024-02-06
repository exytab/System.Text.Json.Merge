using System.Text.Json.Nodes;

namespace System.Text.Json.Merge;

internal static class JsonObjectExtensions
{
    internal static void MergeItem(this JsonObject @this, object content, JsonMergeSettings? settings)
    {
        if (!(content is JsonObject o))
        {
            return;
        }

        var propertyNames = ((IDictionary<string, JsonNode?>)o).Keys;
        for (int i = 0; i < propertyNames.Count; i++)
        {
            string propertyName = propertyNames.ElementAt(i);
            JsonNode? existingProperty = Property(@this, propertyName, settings?.PropertyNameComparison ?? StringComparison.Ordinal);

            if (existingProperty == null)
            {
                Add(@this, propertyName, o[propertyName]);
            }
            else if (((IDictionary<string, JsonNode?>)o).ContainsKey(propertyName))
            {
                if (!(existingProperty is JsonNode existingContainer)
                    || (o[propertyName] == null || existingContainer.GetValueKind() != o[propertyName].GetValueKind()))
                {
                    if (!IsNull(o[propertyName]) || settings?.MergeNullValueHandling == MergeNullValueHandling.Merge)
                    {
                        @this[existingProperty.GetPropertyName()] = o[propertyName]?.DeepClone();
                    }
                }
                else if (o[propertyName] != null)
                {
                    existingContainer.Merge(o[propertyName], settings);
                }
            }
        }
    }

    /// <summary>
    /// Gets the <see cref="JsonNode"/> with the specified name.
    /// The exact name will be searched for first and if no matching property is found then
    /// the <see cref="StringComparison"/> will be used to match a property.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns>A <see cref="JsonNode"/> matched with the specified name or <c>null</c>.</returns>
    internal static JsonNode? Property(this JsonObject @this, string name, StringComparison comparison)
    {
        if (name == null)
        {
            return null;
        }

        if (@this.TryGetPropertyValue(name, out var property))
        {
            return property;
        }

        // test above already uses this comparison so no need to repeat
        if (comparison != StringComparison.Ordinal)
        {
            foreach (KeyValuePair<string, JsonNode?> p in @this)
            {
                if (string.Equals(p.Key, name, comparison))
                {
                    return p.Value;
                }
            }
        }

        return null;
    }

    private static bool IsNull(JsonNode? token)
    {
        if (token == null)
        {
            return true;
        }

        if (token.GetValueKind() == JsonValueKind.Null)
        {
            return true;
        }

        return false;
    }

    internal static void Add(this JsonObject @this, string propertyName, JsonNode? value)
    {
        ((JsonNode)@this).Add(
            new KeyValuePair<string, JsonNode?>(propertyName, value)
        );
    }
}
