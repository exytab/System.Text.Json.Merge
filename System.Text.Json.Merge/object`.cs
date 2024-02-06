using System.Text.Json.Nodes;

namespace System.Text.Json.Merge;

internal static class ObjectExtensions
{
    internal static JsonNode? ToJsonNode(this object? @this)
    {
        if (@this == null)
            return null;

        JsonNode? value = @this as JsonNode;
        if (value != null)
            return value;

        return @this switch
        {
            bool b => JsonValue.Create(b),
            byte b => JsonValue.Create(b),
            char c => JsonValue.Create(c),
            DateTime d => JsonValue.Create(d),
            DateTimeOffset d => JsonValue.Create(d),
            decimal d => JsonValue.Create(d),
            double d => JsonValue.Create(d),
            Guid g => JsonValue.Create(g),
            float f => JsonValue.Create(f),
            int i => JsonValue.Create(i),
            long l => JsonValue.Create(l),
            sbyte b => JsonValue.Create(b),
            short s => JsonValue.Create(s),
            JsonElement e => JsonValue.Create(e),
            uint i => JsonValue.Create(i),
            ulong l => JsonValue.Create(l),
            ushort s => JsonValue.Create(s),
            string s => JsonValue.Create(s),
            _ => null
        };
    }

    internal static bool IsJsonValue(this object? @this)
    {
        if (@this == null)
            return false;

        return @this switch
        {
            bool _ => true,
            byte _ => true,
            char _ => true,
            DateTime _ => true,
            DateTimeOffset _ => true,
            decimal _ => true,
            double _ => true,
            Guid _ => true,
            float _ => true,
            int _ => true,
            long _ => true,
            sbyte _ => true,
            short _ => true,
            JsonElement _ => true,
            uint _ => true,
            ulong _ => true,
            ushort _ => true,
            string _ => true,
            _ => false
        };
    }
}
