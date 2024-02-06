using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace System.Text.Json.Merge;

public static class JsonNodeExtensions
{
    public static void Merge(this JsonNode @this, object? content)
    {
        if (content == null)
        {
            return;
        }

        ValidateContent(content);
        MergeItem(@this, content, null);
    }

    public static void Merge(this JsonNode @this, object? content, JsonMergeSettings? settings)
    {
        if (content == null)
        {
            return;
        }

        ValidateContent(content);
        MergeItem(@this, content, settings);
    }

    private static void ValidateContent(object content)
    {
        if (content.GetType().IsSubclassOf(typeof(JsonNode)))
        {
            return;
        }
        if (IsMultiContent(content))
        {
            return;
        }
        if (IsJsonValue(content))
        {
            return;
        }

        throw new ArgumentException($"Could not determine JSON object type for type {content.GetType()}.", nameof(content));
    }

    internal static bool IsMultiContent([NotNullWhen(true)] object? content)
    {
        return (content is IEnumerable && !(content is string) && !(content is JsonNode) && !(content is byte[]));
    }

    internal static bool IsJsonValue(object content)
    {
        return content.IsJsonValue();
    }

    internal static void MergeItem(this JsonNode @this, object content, JsonMergeSettings? settings)
    {
        switch (@this)
        {
            case JsonObject o:
                o.MergeItem(content, settings);
                break;
            case JsonArray a:
                a.MergeItem(content, settings);
                break;
            case JsonValue v:
                v.MergeItem(content, settings);
                break;
        }
    }

    internal static void MergeEnumerableContent(JsonArray target, IEnumerable content, JsonMergeSettings? settings)
    {
        switch (settings?.MergeArrayHandling ?? MergeArrayHandling.Concat)
        {
            case MergeArrayHandling.Concat:
                foreach (object item in content)
                {
                    target.Add(CreateFromContent(item));
                }
                break;
            case MergeArrayHandling.Union:
#if HAVE_HASH_SET
                HashSet<JsonNode> items = new HashSet<JsonNode>(target, EqualityComparer);

                foreach (object item in content)
                {
                    JsonNode contentItem = CreateFromContent(item);

                    if (items.Add(contentItem))
                    {
                        target.Add(contentItem);
                    }
                }
#else
                Dictionary<JsonNode, bool> items = new Dictionary<JsonNode, bool>(EqualityComparer);
                foreach (JsonNode t in target)
                {
                    items[t] = true;
                }

                foreach (object item in content)
                {
                    JsonNode contentItem = CreateFromContent(item);

                    if (!items.ContainsKey(contentItem))
                    {
                        items[contentItem] = true;
                        target.Add(contentItem);
                    }
                }
#endif
                break;
            case MergeArrayHandling.Replace:
                if (target == content)
                {
                    break;
                }
                target.Clear();
                foreach (object item in content)
                {
                    target.Add(CreateFromContent(item));
                }
                break;
            case MergeArrayHandling.Merge:
                int i = 0;
                foreach (object targetItem in content)
                {
                    if (i < target.Count)
                    {
                        JsonNode? sourceItem = target[i];

                        if (sourceItem is JsonNode existingContainer)
                        {
                            existingContainer.Merge(targetItem, settings);
                        }
                        else
                        {
                            if (targetItem != null)
                            {
                                JsonNode contentValue = CreateFromContent(targetItem);
                                if (contentValue.GetValueKind() != JsonValueKind.Null)
                                {
                                    target[i] = contentValue;
                                }
                            }
                        }
                    }
                    else
                    {
                        target.Add(CreateFromContent(targetItem));
                    }

                    i++;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(settings), "Unexpected merge array handling when merging JSON.");
        }
    }

    internal static JsonNode CreateFromContent(object? content)
    {
        if (content is JsonNode token)
        {
            return token.DeepClone();
        }

        return JsonValue.Create(content);
    }

    internal static void Add(this JsonNode @this, object? content)
    {
        TryAddInternal(@this,
            @this is ICollection en ? en.Count : 0, // @this.ChildrenTokens().Count,
            content,
            false,
            copyAnnotations: true);
    }

    internal static bool TryAddInternal(this JsonNode @this, int index, object? content, bool skipParentCheck, bool copyAnnotations)
    {
        if (IsMultiContent(content))
        {
            IEnumerable enumerable = (IEnumerable)content;

            int multiIndex = index;
            foreach (object c in enumerable)
            {
                TryAddInternal(@this, multiIndex, c, skipParentCheck, copyAnnotations);
                multiIndex++;
            }

            return true;
        }
        else if (content is KeyValuePair<string, JsonNode?> contentAsKvp)
        {
            JsonNode item = CreateFromContent(contentAsKvp.Value);
            if (@this is JsonObject o)
            {
                if (o.ContainsKey(contentAsKvp.Key))
                {
                    o[contentAsKvp.Key] = item;
                }
                else
                {
                    o.Add(contentAsKvp.Key, item);
                }
            }
            else if (@this is JsonArray a)
            {
                a.Add(item);
            }
            else
            {
                throw new InvalidOperationException("The current node is not a JsonObject or a JsonArray.");
            }

            return true;
        }
        else
        {
            JsonNode item = CreateFromContent(content);
            if (@this is JsonObject o)
            {
                o.Add(item);
            }
            else if (@this is JsonArray a)
            {
                a.Insert(index, item);
            }
            else
            {
                throw new InvalidOperationException("The current node is not a JsonObject or a JsonArray.");
            }

            return true;
        }
    }
    internal static void CheckReentrancy()
    {
#if (HAVE_COMPONENT_MODEL || HAVE_INOTIFY_COLLECTION_CHANGED)
            if (_busy)
            {
                throw new InvalidOperationException("Cannot change {0} during a collection change event.".FormatWith(CultureInfo.InvariantCulture, GetType()));
            }
#endif
    }

    internal static JsonNode? EnsureParentToken(this JsonNode @this, JsonNode? item, bool skipParentCheck, bool copyAnnotations)
    {
        if (item == null)
        {
            return null;
        }

        if (skipParentCheck)
        {
            return item;
        }

        // to avoid a token having multiple parents or creating a recursive loop, create a copy if...
        // the item already has a parent
        // the item is being added to itself
        // the item is being added to the root parent of itself
        if (item.Parent != null || item == @this || (item.HasValues() && @this.Root == item))
        {
            item = item.DeepClone();
        }

        return item;
    }

    private static JsonNodeEqualityComparer? _equalityComparer;
    /// <summary>
    /// Gets a comparer that can compare two tokens for value equality.
    /// </summary>
    /// <value>A <see cref="JsonNodeEqualityComparer"/> that can compare two nodes for value equality.</value>
    internal static JsonNodeEqualityComparer EqualityComparer
    {
        get
        {
            if (_equalityComparer == null)
            {
                _equalityComparer = new JsonNodeEqualityComparer();
            }

            return _equalityComparer;
        }
    }

    internal static bool HasValues(this JsonNode @this)
    {
        return @this is JsonObject || @this is JsonArray;
    }

    internal static IList<JsonNode> ChildrenTokens(this JsonNode @this) => @this switch
    {
        JsonObject o => ((IDictionary<string, JsonNode?>)o).Values.ToList(),
        JsonArray a => a,
        _ => new List<JsonNode>(),
    };
}
