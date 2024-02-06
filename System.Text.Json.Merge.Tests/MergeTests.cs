using System.Text.Json.Nodes;

namespace System.Text.Json.Merge.Tests;

public class MergeTests
{
    [Fact]
    public void MergeInvalidObject()
    {
        var a = new JsonObject();

        var exp = Assert.Throws<ArgumentException>(
            () => a.Merge(new Version())
        );
        Assert.Equal(@"Could not determine JSON object type for type System.Version. (Parameter 'content')", exp.Message);
    }

    [Fact]
    public void MergeArraySelf()
    {
        var a = new JsonArray { "1", "2" };
        a.Merge(a, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
        Node.Equal(new JsonArray { "1", "2" }, a);
    }

    [Fact]
    public void MergeObjectSelf()
    {
        var a = new JsonObject
        {
            ["1"] = 1,
            ["2"] = 2
        };
        a.Merge(a, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
        Node.Equal(new JsonObject
        {
            ["1"] = 1,
            ["2"] = 2
        }, a);
    }

    [Fact]
    public void MergeArrayIntoArray_Replace()
    {
        var a = new JsonArray { "1", "2" };
        a.Merge(new string[] { "3", "4" }, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
        Node.Equal(new JsonArray { "3", "4" }, a);
    }

    [Fact]
    public void MergeArrayIntoArray_Concat()
    {
        var a = new JsonArray { "1", "2" };
        a.Merge(new string[] { "3", "4" }, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Concat });
        Node.Equal(new JsonArray { "1", "2", "3", "4" }, a);
    }

    [Fact]
    public void MergeArrayIntoArray_Union()
    {
        var a = new JsonArray { "1", "2" };
        a.Merge(new string[] { "2", "3", "4" }, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
        Node.Equal(new JsonArray { "1", "2", "3", "4" }, a);
    }

    [Fact]
    public void MergeArrayIntoArray_Merge()
    {
        var a = new JsonArray { "1", "2" };
        a.Merge(new string[] { "2" }, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Merge });
        Node.Equal(new JsonArray { "2", "2" }, a);
    }

    [Fact]
    public void MergeNullString()
    {
        var a = new JsonObject { ["a"] = 1 };
        var b = new JsonObject { ["a"] = false ? "2" : null };
        a.Merge(b);

        Assert.Equal(1, (int)a["a"]);
    }

    [Fact]
    public void MergeObjectProperty()
    {
        var left = new JsonObject
        {
            ["Property1"] = 1,
        };
        var right = new JsonObject
        {
            ["Property2"] = 2,
        };

        left.Merge(right);

        string json = left.ToString();

        Assert.Equal(@"{
  ""Property1"": 1,
  ""Property2"": 2
}", json);
    }

    [Fact]
    public void MergeChildObject()
    {
        var left = new JsonObject
        {
            ["Property1"] = new JsonObject { ["SubProperty1"] = 1 }
        };
        var right = new JsonObject
        {
            ["Property1"] = new JsonObject { ["SubProperty2"] = 2 }
        };

        left.Merge(right);

        string json = left.ToString();

        Assert.Equal(@"{
  ""Property1"": {
    ""SubProperty1"": 1,
    ""SubProperty2"": 2
  }
}", json);
    }

    [Fact]
    public void MergeMismatchedTypesRoot()
    {
        var left = new JsonObject
        {
            ["Property1"] = new JsonObject { ["SubProperty1"] = 1 }
        };
        var right = new JsonArray
        {
            new JsonObject { ["Property1"] = 1 },
            new JsonObject { ["Property1"] = 1 }
        };

        left.Merge(right);

        string json = left.ToString();

        Assert.Equal(@"{
  ""Property1"": {
    ""SubProperty1"": 1
  }
}", json);
    }

    [Fact]
    public void MergeMultipleObjects()
    {
        var left = new JsonObject
        {
            ["Property1"] = new JsonObject { ["SubProperty1"] = 1 }
        };
        var right = new JsonObject
        {
            ["Property1"] = new JsonObject { ["SubProperty2"] = 2 },
            ["Property2"] = 2
        };

        left.Merge(right);

        string json = left.ToString();

        Assert.Equal(@"{
  ""Property1"": {
    ""SubProperty1"": 1,
    ""SubProperty2"": 2
  },
  ""Property2"": 2
}", json);
    }

    [Fact]
    public void MergeArray()
    {
        var left = new JsonObject
        {
            ["Array1"] = new JsonArray
            {
                new JsonObject
                {
                    ["Property1"] = new JsonObject
                    {
                        ["Property1"] = 1,
                        ["Property2"] = 2,
                        ["Property3"] = 3,
                        ["Property4"] = 4,
                        ["Property5"] = null
                    }
                },
                new JsonObject { },
                3,
                null,
                5,
                null
            }
        };
        var right = new JsonObject
        {
            ["Array1"] = new JsonArray
            {
                new JsonObject
                {
                    ["Property1"] = new JsonObject
                    {
                        ["Property1"] = null,
                        ["Property2"] = 3,
                        ["Property3"] = new JsonObject
                        {
                        },
                        ["Property5"] = null
                    }
                },
                null,
                null,
                4,
                5.1,
                null,
                new JsonObject
                {
                    ["Property1"] = 1
                }
            }
        };

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Merge
        });

        string json = left.ToString();

        Assert.Equal(@"{
  ""Array1"": [
    {
      ""Property1"": {
        ""Property1"": 1,
        ""Property2"": 3,
        ""Property3"": {},
        ""Property4"": 4,
        ""Property5"": null
      }
    },
    {},
    3,
    4,
    5.1,
    null,
    {
      ""Property1"": 1
    }
  ]
}", json);
    }

    [Fact]
    public void ConcatArray()
    {
        var left = new JsonObject
        {
            ["Array1"] = new JsonArray
            {
                new JsonObject { ["Property1"] = 1 },
                new JsonObject { ["Property1"] = 1 }
            }
        };
        var right = new JsonObject
        {
            ["Array1"] = new JsonArray
            {
                new JsonObject { ["Property1"] = 1 },
                new JsonObject { ["Property2"] = 2 },
                new JsonObject { ["Property3"] = 3 }
            }
        };

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Concat
        });

        string json = left.ToString();

        Assert.Equal(@"{
  ""Array1"": [
    {
      ""Property1"": 1
    },
    {
      ""Property1"": 1
    },
    {
      ""Property1"": 1
    },
    {
      ""Property2"": 2
    },
    {
      ""Property3"": 3
    }
  ]
}", json);
    }

    [Fact]
    public void MergeMismatchingTypesInArray()
    {
        var left = new JsonArray
        {
            true,
            null,
            new JsonObject { ["Property1"] = 1 },
            new JsonArray { 1 },
            new JsonObject { ["Property1"] = 1 },
            1,
            new JsonArray { 1 }
        };
        var right = new JsonArray
        {
            1,
            5,
            new JsonArray { 1 },
            new JsonObject { ["Property1"] = 1 },
            true,
            new JsonObject { ["Property1"] = 1 },
            null
        };

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Merge
        });

        string json = left.ToString();

        Assert.Equal(@"[
  1,
  5,
  {
    ""Property1"": 1
  },
  [
    1
  ],
  {
    ""Property1"": 1
  },
  {
    ""Property1"": 1
  },
  [
    1
  ]
]", json);
    }

    [Fact]
    public void MergeMismatchingTypesInObject()
    {
        var left = new JsonObject
        {
            ["Property1"] = new JsonArray
            {
                1
            },
            ["Property2"] = new JsonArray
            {
                1
            },
            ["Property3"] = true,
            ["Property4"] = true
        };
        var right = new JsonObject
        {
            ["Property1"] = new JsonObject { ["Nested"] = true },
            ["Property2"] = true,
            ["Property3"] = new JsonArray
            {
                1
            },
            ["Property4"] = null
        };

        left.Merge(right);

        string json = left.ToString();

        Assert.Equal(@"{
  ""Property1"": {
    ""Nested"": true
  },
  ""Property2"": true,
  ""Property3"": [
    1
  ],
  ""Property4"": true
}", json);
    }

    [Fact]
    public void MergeArrayOverwrite_Nested()
    {
        var left = new JsonObject
        {
            ["Array1"] = new JsonArray
            {
                1,
                2,
                3
            }
        };
        var right = new JsonObject
        {
            ["Array1"] = new JsonArray
            {
                    4,
                    5
            }
        };

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace
        });

        string json = left.ToString();

        Assert.Equal(@"{
  ""Array1"": [
    4,
    5
  ]
}", json);
    }

    [Fact]
    public void MergeArrayOverwrite_Root()
    {
        var left = new JsonArray
        {
            1,
            2,
            3
        };
        var right = new JsonArray
        {
                4,
                5
        };

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace
        });

        string json = left.ToString();

        Assert.Equal(@"[
  4,
  5
]", json);
    }

    [Fact]
    public void UnionArrays()
    {
        var left = new JsonObject
        {
            ["Array1"] = new JsonArray
            {
                new JsonObject { ["Property1"] = 1 },
                new JsonObject { ["Property1"] = 1 }
            }
        };
        var right = new JsonObject
        {
            ["Array1"] = new JsonArray
            {
                new JsonObject { ["Property1"] = 1 },
                new JsonObject { ["Property2"] = 2 },
                new JsonObject { ["Property3"] = 3 }
            }
        };

        left.Merge(right, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union
        });

        string json = left.ToString();

        Assert.Equal(@"{
  ""Array1"": [
    {
      ""Property1"": 1
    },
    {
      ""Property1"": 1
    },
    {
      ""Property2"": 2
    },
    {
      ""Property3"": 3
    }
  ]
}", json);
    }
    [Fact]
    public void MergeDefaultContainers()
    {
        JsonObject o = new JsonObject();
        o.Merge(new JsonObject());
        Assert.Empty(o);

        JsonArray a = new JsonArray();
        a.Merge(new JsonArray());
        Assert.Empty(a);
    }

    [Fact]
    public void MergeNull()
    {
        JsonObject o = new JsonObject();
        o.Merge(null);
        Assert.Empty(o);

        JsonArray a = new JsonArray();
        a.Merge(null);
        Assert.Empty(a);
    }

    [Fact]
    public void MergeNullValueHandling_Array()
    {
        string originalJson = @"{
  ""Bar"": [
    ""a"",
    ""b"",
    ""c""
  ]
}";
        string newJson = @"{
  ""Bar"": null
}";

        JsonObject oldFoo = JsonObject.Parse(originalJson)!.AsObject();
        JsonObject newFoo = JsonObject.Parse(newJson)!.AsObject();

        oldFoo.Merge(newFoo, new JsonMergeSettings
        {
            MergeNullValueHandling = MergeNullValueHandling.Ignore
        });

        Assert.Equal(originalJson, oldFoo.ToString());

        oldFoo.Merge(newFoo, new JsonMergeSettings
        {
            MergeNullValueHandling = MergeNullValueHandling.Merge
        });

        Assert.Equal(newJson, newFoo.ToString());
    }

    [Fact]
    public void MergeNullValueHandling_Object()
    {
        string originalJson = @"{
  ""Bar"": {}
}";
        string newJson = @"{
  ""Bar"": null
}";

        JsonObject oldFoo = JsonObject.Parse(originalJson)!.AsObject();
        JsonObject newFoo = JsonObject.Parse(newJson)!.AsObject();

        oldFoo.Merge(newFoo, new JsonMergeSettings
        {
            MergeNullValueHandling = MergeNullValueHandling.Ignore
        });

        Assert.Equal(originalJson, oldFoo.ToString());

        oldFoo.Merge(newFoo, new JsonMergeSettings
        {
            MergeNullValueHandling = MergeNullValueHandling.Merge
        });

        Assert.Equal(newJson, newFoo.ToString());
    }

    [Fact]
    public void Merge_IgnorePropertyCase()
    {
        JsonObject o1 = JsonObject.Parse(@"{
                                          ""Id"": ""1"",
                                          ""Words"": [ ""User"" ]
                                        }")!.AsObject();
        JsonObject o2 = JsonObject.Parse(@"{
                                            ""Id"": ""1"",
                                            ""words"": [ ""Name"" ]
                                        }")!.AsObject();

        o1.Merge(o2, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Concat,
            MergeNullValueHandling = MergeNullValueHandling.Merge,
            PropertyNameComparison = StringComparison.OrdinalIgnoreCase
        });

        Assert.Null(o1["words"]);
        Assert.NotNull(o1["Words"]);

        JsonArray words = (JsonArray)o1["Words"];
        Assert.Equal("User", (string)words[0]);
        Assert.Equal("Name", (string)words[1]);
    }

    [Fact]
    public void MergeSettingsComparisonDefault()
    {
        JsonMergeSettings settings = new JsonMergeSettings();

        Assert.Equal(StringComparison.Ordinal, settings.PropertyNameComparison);
    }
}
