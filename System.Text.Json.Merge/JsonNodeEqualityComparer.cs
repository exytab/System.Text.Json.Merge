﻿using System.Text.Json.Nodes;

namespace System.Text.Json.Merge;

/// <summary>
/// Compares tokens to determine whether they are equal.
/// </summary>
internal class JsonNodeEqualityComparer : IEqualityComparer<JsonNode>
{
    /// <summary>
    /// Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first object of type <see cref="JsonNode"/> to compare.</param>
    /// <param name="y">The second object of type <see cref="JsonNode"/> to compare.</param>
    /// <returns>
    /// <c>true</c> if the specified objects are equal; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(JsonNode? x, JsonNode? y)
    {
        return JsonNode.DeepEquals(x, y);
    }

    /// <summary>
    /// Returns a hash code for the specified object.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object"/> for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    /// <exception cref="System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is <c>null</c>.</exception>
    public int GetHashCode(JsonNode obj)
    {
        //if (obj == null)
        //{
        //    return 0;
        //}

        //if (obj is JsonValue value)
        //{
        //    return value.GetHashCodeOfValue();
        //}

        //return obj.GetHashCode();

#warning This is a temporary fix for the issue where the hash code of a JsonNode is not consistent with its value.
        return 0;
    }
}
