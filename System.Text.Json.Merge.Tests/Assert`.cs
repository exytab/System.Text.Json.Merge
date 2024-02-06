using System.Text.Json.Nodes;

namespace System.Text.Json.Merge.Tests;

public static class Node
{
    public static void Equal(JsonNode a, JsonNode b)
    {
        Assert.True(JsonNode.DeepEquals(a, b));
    }
}
