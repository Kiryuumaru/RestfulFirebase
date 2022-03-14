namespace DatabaseTest.LocalDatabaseTest;

using RestfulFirebase;
using RestfulFirebase.RealtimeDatabase.Models;
using RestfulFirebase.Test.Utilities;
using RestfulFirebase.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using RestfulFirebase.Local;
using System.Collections.Concurrent;
using RestfulFirebase.Exceptions;
using SerializerHelpers;
using SerializerHelpers.Exceptions;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

public class SampleModel
{
    public string? Value1 { get; set; }

    public string? Value2 { get; set; }
}

public class SampleModelSerializer : ISerializer<SampleModel>
{
    public SampleModel? Deserialize(string? data, SampleModel? defaultValue = null)
    {
        var des = StringSerializer.Deserialize(data);
        if (des == null || des.Length != 2)
        {
            return null;
        }
        return new SampleModel()
        {
            Value1 = des[0],
            Value2 = des[1]
        };
    }

    public string? Serialize(SampleModel? value, string? defaultValue = null)
    {
        return StringSerializer.Serialize(value?.Value1, value?.Value2);
    }
}

public static class Helpers
{
    public static Task<RestfulFirebaseApp> Empty()
    {
        return RestfulFirebase.Test.Helpers.AppGenerator().generator();
    }

    public static async Task<RestfulFirebaseApp> Hier()
    {
        var (generator, dispose) = RestfulFirebase.Test.Helpers.AppGenerator();
        var app = await generator();

        app.LocalDatabase.SetValue(app.Config.CachedLocalDatabase, "test", "0", "1", "1.1");
        app.LocalDatabase.SetValue(app.Config.CachedLocalDatabase, "test", "0", "1", "1.2");

        app.LocalDatabase.SetValue(app.Config.CachedLocalDatabase, "test", "0", "2", "2.1", "2.1.1");
        app.LocalDatabase.SetValue(app.Config.CachedLocalDatabase, "test", "0", "2", "2.1", "2.1.2");

        app.LocalDatabase.SetValue(app.Config.CachedLocalDatabase, "test", "0", "3", "3.1", "3.1.1", "3.1.1.1");
        app.LocalDatabase.SetValue(app.Config.CachedLocalDatabase, "test", "0", "3", "3.1", "3.1.1", "3.1.1.2");

        Serializer.Register(new SampleModelSerializer());

        app.LocalDatabase.SetValue("0", new SampleModel()
        {
            Value1 = "val1",
            Value2 = "val2"
        }, false);
        app.LocalDatabase.SetValue("0", new SampleModel()
        {
            Value1 = "persVal1",
            Value2 = "persVal2"
        }, true);

        return app;
    }
}

public class ContainsTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var db = app.LocalDatabase;

        Assert.True(db.Contains(app.Config.CachedLocalDatabase, "0", "1", "1.1"));
        Assert.True(db.Contains(app.Config.CachedLocalDatabase, "0", "1", "1.2"));

        Assert.True(db.Contains(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.1"));
        Assert.True(db.Contains(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.2"));

        Assert.True(db.Contains(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.True(db.Contains(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.2"));

        Assert.False(db.Contains(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
        Assert.False(db.Contains(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
        Assert.False(db.Contains(app.Config.CachedLocalDatabase, "0", "4"));
        Assert.False(db.Contains(app.Config.CachedLocalDatabase, "0", "4"));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.Contains(app.Config.CachedLocalDatabase));
        Assert.Throws<StringNullOrEmptyException>(() => db.Contains(app.Config.CachedLocalDatabase, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.Contains(app.Config.CachedLocalDatabase, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.Contains(app.Config.CachedLocalDatabase, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.Contains(app.Config.CachedLocalDatabase, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.Contains(app.Config.CachedLocalDatabase, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.Contains(app.Config.CachedLocalDatabase, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class ContainsKeyTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var db = app.LocalDatabase;

        Assert.True(db.ContainsKey("0", true));
        Assert.True(db.ContainsKey("0", false));

        Assert.False(db.ContainsKey("1", true));
        Assert.False(db.ContainsKey("1", false));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<ArgumentException>(() => db.ContainsKey(null, false));
        
        app.Dispose();
    }
}

public class DeleteTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        db.Delete(app.Config.CachedLocalDatabase, "0", "1", "1.1");
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "1", "1.1"));
        string[][] test1 = db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test1,
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.2", i[2]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.1", i[3]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.2", i[3]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.1", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.2", i[4]);
            });

        db.Delete(app.Config.CachedLocalDatabase, "0", "2");
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "2"));
        string[][] test2 = db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test2,
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.2", i[2]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.1", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.2", i[4]);
            });

        db.Delete(app.Config.CachedLocalDatabase, "0", "3", "3.1");
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1"));
        string[][] test3 = db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test3,
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.2", i[2]);
            });

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.Delete(app.Config.CachedLocalDatabase));
        Assert.Throws<StringNullOrEmptyException>(() => db.Delete(app.Config.CachedLocalDatabase, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.Delete(app.Config.CachedLocalDatabase, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.Delete(app.Config.CachedLocalDatabase, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.Delete(app.Config.CachedLocalDatabase, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.Delete(app.Config.CachedLocalDatabase, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.Delete(app.Config.CachedLocalDatabase, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class GetChildrenTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        (string[] path, string key)[] test1 = db.GetChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test1,
            i =>
            {
                Assert.Equal("1", i.key);
                Assert.Equal(2, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("1", i.path[1]);
            },
            i =>
            {
                Assert.Equal("2", i.key);
                Assert.Equal(2, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("2", i.path[1]);
            },
            i =>
            {
                Assert.Equal("3", i.key);
                Assert.Equal(2, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("3", i.path[1]);
            });

        (string[] path, string key)[] test2 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "1");
        Assert.Collection(test2,
            i =>
            {
                Assert.Equal("1.1", i.key);
                Assert.Equal(3, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("1", i.path[1]);
                Assert.Equal("1.1", i.path[2]);
            },
            i =>
            {
                Assert.Equal("1.2", i.key);
                Assert.Equal(3, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("1", i.path[1]);
                Assert.Equal("1.2", i.path[2]);
            });

        (string[] path, string key)[] test3 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "2");
        Assert.Collection(test3,
            i =>
            {
                Assert.Equal("2.1", i.key);
                Assert.Equal(3, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("2", i.path[1]);
                Assert.Equal("2.1", i.path[2]);
            });

        (string[] path, string key)[] test4 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "2", "2.1");
        Assert.Collection(test4,
            i =>
            {
                Assert.Equal("2.1.1", i.key);
                Assert.Equal(4, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("2", i.path[1]);
                Assert.Equal("2.1", i.path[2]);
                Assert.Equal("2.1.1", i.path[3]);
            },
            i =>
            {
                Assert.Equal("2.1.2", i.key);
                Assert.Equal(4, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("2", i.path[1]);
                Assert.Equal("2.1", i.path[2]);
                Assert.Equal("2.1.2", i.path[3]);
            });

        (string[] path, string key)[] test5 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "3");
        Assert.Collection(test5,
            i =>
            {
                Assert.Equal("3.1", i.key);
                Assert.Equal(3, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("3", i.path[1]);
                Assert.Equal("3.1", i.path[2]);
            });

        (string[] path, string key)[] test6 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "3", "3.1");
        Assert.Collection(test6,
            i =>
            {
                Assert.Equal("3.1.1", i.key);
                Assert.Equal(4, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("3", i.path[1]);
                Assert.Equal("3.1", i.path[2]);
                Assert.Equal("3.1.1", i.path[3]);
            });

        (string[] path, string key)[] test7 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1");
        Assert.Collection(test7,
            i =>
            {
                Assert.Equal("3.1.1.1", i.key);
                Assert.Equal(5, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("3", i.path[1]);
                Assert.Equal("3.1", i.path[2]);
                Assert.Equal("3.1.1", i.path[3]);
                Assert.Equal("3.1.1.1", i.path[4]);
            },
            i =>
            {
                Assert.Equal("3.1.1.2", i.key);
                Assert.Equal(5, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("3", i.path[1]);
                Assert.Equal("3.1", i.path[2]);
                Assert.Equal("3.1.1", i.path[3]);
                Assert.Equal("3.1.1.2", i.path[4]);
            });

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.GetChildren(app.Config.CachedLocalDatabase));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetChildren(app.Config.CachedLocalDatabase, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetChildren(app.Config.CachedLocalDatabase, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.GetChildren(app.Config.CachedLocalDatabase, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetChildren(app.Config.CachedLocalDatabase, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetChildren(app.Config.CachedLocalDatabase, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetChildren(app.Config.CachedLocalDatabase, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class GetDataTypeTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var db = app.LocalDatabase;

        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "0", "1", "1.1"));
        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "0", "1", "1.2"));
        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.1"));
        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.2"));
        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.2"));

        Assert.Equal(LocalDataType.Path, db.GetDataType(app.Config.CachedLocalDatabase, "0", "1"));
        Assert.Equal(LocalDataType.Path, db.GetDataType(app.Config.CachedLocalDatabase, "0", "1"));
        Assert.Equal(LocalDataType.Path, db.GetDataType(app.Config.CachedLocalDatabase, "0", "2", "2.1"));
        Assert.Equal(LocalDataType.Path, db.GetDataType(app.Config.CachedLocalDatabase, "0", "2", "2.1"));
        Assert.Equal(LocalDataType.Path, db.GetDataType(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1"));
        Assert.Equal(LocalDataType.Path, db.GetDataType(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1"));

        Assert.Equal(LocalDataType.Path, db.GetDataType(app.Config.CachedLocalDatabase, "0"));
        Assert.Equal(LocalDataType.Path, db.GetDataType(app.Config.CachedLocalDatabase, "0", "2"));
        Assert.Equal(LocalDataType.Path, db.GetDataType(app.Config.CachedLocalDatabase, "0", "3", "3.1"));

        Assert.Equal(LocalDataType.Path, db.GetDataType(app.Config.CachedLocalDatabase, "0", "3"));

        Assert.Equal(LocalDataType.Path, db.GetDataType(app.Config.CachedLocalDatabase, "0"));

        // Null values
        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "0", "1", "1.3"));
        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "0", "1", "1.4"));
        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.1", "2.1.1.1"));
        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.2", "2.1.2.1"));
        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "0", "4"));
        Assert.Equal(LocalDataType.Value, db.GetDataType(app.Config.CachedLocalDatabase, "1"));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.GetDataType(app.Config.CachedLocalDatabase));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetDataType(app.Config.CachedLocalDatabase, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetDataType(app.Config.CachedLocalDatabase, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.GetDataType(app.Config.CachedLocalDatabase, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetDataType(app.Config.CachedLocalDatabase, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetDataType(app.Config.CachedLocalDatabase, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetDataType(app.Config.CachedLocalDatabase, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class GetRecursiveChildrenTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        string[][] test1 = db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test1,
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.1", i[2]);
            },
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.2", i[2]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.1", i[3]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.2", i[3]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.1", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.2", i[4]);
            });

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveChildren(app.Config.CachedLocalDatabase));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveChildren(app.Config.CachedLocalDatabase, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveChildren(app.Config.CachedLocalDatabase, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveChildren(app.Config.CachedLocalDatabase, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveChildren(app.Config.CachedLocalDatabase, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class GetRecursiveRelativeChildrenTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        string[][] test1 = db.GetRecursiveRelativeChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test1,
            i =>
            {
                Assert.Equal(2, i.Length);
                Assert.Equal("1", i[0]);
                Assert.Equal("1.1", i[1]);
            },
            i =>
            {
                Assert.Equal(2, i.Length);
                Assert.Equal("1", i[0]);
                Assert.Equal("1.2", i[1]);
            },
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("2", i[0]);
                Assert.Equal("2.1", i[1]);
                Assert.Equal("2.1.1", i[2]);
            },
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("2", i[0]);
                Assert.Equal("2.1", i[1]);
                Assert.Equal("2.1.2", i[2]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("3", i[0]);
                Assert.Equal("3.1", i[1]);
                Assert.Equal("3.1.1", i[2]);
                Assert.Equal("3.1.1.1", i[3]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("3", i[0]);
                Assert.Equal("3.1", i[1]);
                Assert.Equal("3.1.1", i[2]);
                Assert.Equal("3.1.1.2", i[3]);
            });

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveRelativeChildren(app.Config.CachedLocalDatabase));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveRelativeChildren(app.Config.CachedLocalDatabase, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveRelativeChildren(app.Config.CachedLocalDatabase, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveRelativeChildren(app.Config.CachedLocalDatabase, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveRelativeChildren(app.Config.CachedLocalDatabase, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveRelativeChildren(app.Config.CachedLocalDatabase, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRecursiveRelativeChildren(app.Config.CachedLocalDatabase, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class GetRelativeTypedChildrenTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        (string key, LocalDataType type)[] test1 = db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test1,
            i =>
            {
                Assert.Equal("1", i.key);
                Assert.Equal(LocalDataType.Path, i.type);
            },
            i =>
            {
                Assert.Equal("2", i.key);
                Assert.Equal(LocalDataType.Path, i.type);
            },
            i =>
            {
                Assert.Equal("3", i.key);
                Assert.Equal(LocalDataType.Path, i.type);
            });

        (string key, LocalDataType type)[] test2 = db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, "0", "1");
        Assert.Collection(test2,
            i =>
            {
                Assert.Equal("1.1", i.key);
                Assert.Equal(LocalDataType.Value, i.type);
            },
            i =>
            {
                Assert.Equal("1.2", i.key);
                Assert.Equal(LocalDataType.Value, i.type);
            });

        (string key, LocalDataType type)[] test3 = db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, "0", "2");
        Assert.Collection(test3,
            i =>
            {
                Assert.Equal("2.1", i.key);
                Assert.Equal(LocalDataType.Path, i.type);
            });

        (string key, LocalDataType type)[] test4 = db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, "0", "2", "2.1");
        Assert.Collection(test4,
            i =>
            {
                Assert.Equal("2.1.1", i.key);
                Assert.Equal(LocalDataType.Value, i.type);
            },
            i =>
            {
                Assert.Equal("2.1.2", i.key);
                Assert.Equal(LocalDataType.Value, i.type);
            });

        (string key, LocalDataType type)[] test5 = db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, "0", "3");
        Assert.Collection(test5,
            i =>
            {
                Assert.Equal("3.1", i.key);
                Assert.Equal(LocalDataType.Path, i.type);
            });

        (string key, LocalDataType type)[] test6 = db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, "0", "3", "3.1");
        Assert.Collection(test6,
            i =>
            {
                Assert.Equal("3.1.1", i.key);
                Assert.Equal(LocalDataType.Path, i.type);
            });

        (string key, LocalDataType type)[] test7 = db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1");
        Assert.Collection(test7,
            i =>
            {
                Assert.Equal("3.1.1.1", i.key);
                Assert.Equal(LocalDataType.Value, i.type);
            },
            i =>
            {
                Assert.Equal("3.1.1.2", i.key);
                Assert.Equal(LocalDataType.Value, i.type);
            });

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetRelativeTypedChildren(app.Config.CachedLocalDatabase, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class GetTypedChildrenTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        (string[] path, string key)[] test1 = db.GetChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test1,
            i =>
            {
                Assert.Equal("1", i.key);
                Assert.Equal(2, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("1", i.path[1]);
            },
            i =>
            {
                Assert.Equal("2", i.key);
                Assert.Equal(2, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("2", i.path[1]);
            },
            i =>
            {
                Assert.Equal("3", i.key);
                Assert.Equal(2, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("3", i.path[1]);
            });

        (string[] path, string key)[] test2 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "1");
        Assert.Collection(test2,
            i =>
            {
                Assert.Equal("1.1", i.key);
                Assert.Equal(3, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("1", i.path[1]);
                Assert.Equal("1.1", i.path[2]);
            },
            i =>
            {
                Assert.Equal("1.2", i.key);
                Assert.Equal(3, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("1", i.path[1]);
                Assert.Equal("1.2", i.path[2]);
            });

        (string[] path, string key)[] test3 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "2");
        Assert.Collection(test3,
            i =>
            {
                Assert.Equal("2.1", i.key);
                Assert.Equal(3, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("2", i.path[1]);
                Assert.Equal("2.1", i.path[2]);
            });

        (string[] path, string key)[] test4 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "2", "2.1");
        Assert.Collection(test4,
            i =>
            {
                Assert.Equal("2.1.1", i.key);
                Assert.Equal(4, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("2", i.path[1]);
                Assert.Equal("2.1", i.path[2]);
                Assert.Equal("2.1.1", i.path[3]);
            },
            i =>
            {
                Assert.Equal("2.1.2", i.key);
                Assert.Equal(4, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("2", i.path[1]);
                Assert.Equal("2.1", i.path[2]);
                Assert.Equal("2.1.2", i.path[3]);
            });

        (string[] path, string key)[] test5 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "3");
        Assert.Collection(test5,
            i =>
            {
                Assert.Equal("3.1", i.key);
                Assert.Equal(3, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("3", i.path[1]);
                Assert.Equal("3.1", i.path[2]);
            });

        (string[] path, string key)[] test6 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "3", "3.1");
        Assert.Collection(test6,
            i =>
            {
                Assert.Equal("3.1.1", i.key);
                Assert.Equal(4, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("3", i.path[1]);
                Assert.Equal("3.1", i.path[2]);
                Assert.Equal("3.1.1", i.path[3]);
            });

        (string[] path, string key)[] test7 = db.GetChildren(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1");
        Assert.Collection(test7,
            i =>
            {
                Assert.Equal("3.1.1.1", i.key);
                Assert.Equal(5, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("3", i.path[1]);
                Assert.Equal("3.1", i.path[2]);
                Assert.Equal("3.1.1", i.path[3]);
                Assert.Equal("3.1.1.1", i.path[4]);
            },
            i =>
            {
                Assert.Equal("3.1.1.2", i.key);
                Assert.Equal(5, i.path.Length);
                Assert.Equal("0", i.path[0]);
                Assert.Equal("3", i.path[1]);
                Assert.Equal("3.1", i.path[2]);
                Assert.Equal("3.1.1", i.path[3]);
                Assert.Equal("3.1.1.2", i.path[4]);
            });

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.GetTypedChildren(app.Config.CachedLocalDatabase));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetTypedChildren(app.Config.CachedLocalDatabase, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetTypedChildren(app.Config.CachedLocalDatabase, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.GetTypedChildren(app.Config.CachedLocalDatabase, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetTypedChildren(app.Config.CachedLocalDatabase, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetTypedChildren(app.Config.CachedLocalDatabase, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetTypedChildren(app.Config.CachedLocalDatabase, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class GetValueTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var db = app.LocalDatabase;

        Assert.Equal("test", db.GetValue(app.Config.CachedLocalDatabase, "0", "1", "1.1"));
        Assert.Equal("test", db.GetValue(app.Config.CachedLocalDatabase, "0", "1", "1.2"));
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "1"));

        Assert.Equal("test", db.GetValue(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.1"));
        Assert.Equal("test", db.GetValue(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.2"));
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "2"));
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "2", "2.1"));

        Assert.Equal("test", db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.Equal("test", db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "3"));
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1"));
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1"));

        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "4"));
        Assert.Null(db.GetValue(app.Config.CachedLocalDatabase, "0", "4"));

        var val1 = db.GetValue<SampleModel>("0", false);
        Assert.Equal("val1", val1?.Value1);
        Assert.Equal("val2", val1?.Value2);

        var val2 = db.GetValue<SampleModel>("0", true);
        Assert.Equal("persVal1", val2?.Value1);
        Assert.Equal("persVal2", val2?.Value2);

        var val3 = db.GetValue<SampleModel>("none", false);
        Assert.Null(val3);

        var val4 = db.GetValue<SampleModel>("none", true);
        Assert.Null(val4);

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.GetValue(app.Config.CachedLocalDatabase));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetValue(app.Config.CachedLocalDatabase, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetValue(app.Config.CachedLocalDatabase, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.GetValue(app.Config.CachedLocalDatabase, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetValue(app.Config.CachedLocalDatabase, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetValue(app.Config.CachedLocalDatabase, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.GetValue(app.Config.CachedLocalDatabase, new string[] { "path", "" }));

        Assert.Throws<SerializerNotSupportedException>(() => db.GetValue<GetValueTest>("1", true));
        Assert.Throws<ArgumentException>(() => db.GetValue<string>(null, true));

        app.Dispose();
    }
}

public class RemoveValueTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var db = app.LocalDatabase;

        Assert.True(db.ContainsKey("0", true));
        db.RemoveValue("0", true);
        Assert.False(db.ContainsKey("0", true));

        Assert.True(db.ContainsKey("0", false));
        db.RemoveValue("0", false);
        Assert.False(db.ContainsKey("0", false));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<ArgumentException>(() => db.RemoveValue(null, false));

        app.Dispose();
    }
}

public class SetValueTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Empty();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        db.SetValue(app.Config.CachedLocalDatabase, "testValue01", "0", "1", "1.1");
        db.SetValue(app.Config.CachedLocalDatabase, "testValue02", "0", "1", "1.2");
        Assert.Equal("testValue01", db.GetValue(app.Config.CachedLocalDatabase, "0", "1", "1.1"));
        Assert.Equal("testValue02", db.GetValue(app.Config.CachedLocalDatabase, "0", "1", "1.2"));
        string[][] test1 = db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test1,
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.1", i[2]);
            },
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.2", i[2]);
            });

        db.SetValue(app.Config.CachedLocalDatabase, "testValue03", "0", "2", "2.1", "2.1.1");
        db.SetValue(app.Config.CachedLocalDatabase, "testValue04", "0", "2", "2.1", "2.1.2");
        Assert.Equal("testValue03", db.GetValue(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.1"));
        Assert.Equal("testValue04", db.GetValue(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.2"));
        string[][] test2 = db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test2,
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.1", i[2]);
            },
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.2", i[2]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.1", i[3]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.2", i[3]);
            });

        db.SetValue(app.Config.CachedLocalDatabase, "testValue05", "0", "3", "3.1", "3.1.1", "3.1.1.1");
        db.SetValue(app.Config.CachedLocalDatabase, "testValue06", "0", "3", "3.1", "3.1.1", "3.1.1.2");
        Assert.Equal("testValue05", db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.Equal("testValue06", db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
        string[][] test3 = db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test3,
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.1", i[2]);
            },
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.2", i[2]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.1", i[3]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.2", i[3]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.1", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.2", i[4]);
            });

        Assert.False(db.ContainsKey("sample1", true));
        db.SetValue("sample1", new SampleModel()
        {
            Value1 = "v1",
            Value2 = "v2"
        }, true);
        Assert.True(db.ContainsKey("sample1", true));

        Assert.False(db.ContainsKey("sample2", false));
        db.SetValue("sample2", new SampleModel()
        {
            Value1 = "vv1",
            Value2 = "vv2"
        }, false);
        Assert.True(db.ContainsKey("sample2", false));

        app.Dispose();
    }

    [Fact]
    public async void WithExistingData()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        db.SetValue(app.Config.CachedLocalDatabase, "testValue01", "0", "1", "1.1");
        db.SetValue(app.Config.CachedLocalDatabase, "testValue02", "0", "1", "1.2");
        Assert.Equal("testValue01", db.GetValue(app.Config.CachedLocalDatabase, "0", "1", "1.1"));
        Assert.Equal("testValue02", db.GetValue(app.Config.CachedLocalDatabase, "0", "1", "1.2"));
        string[][] test1 = db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test1,
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.1", i[2]);
            },
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.2", i[2]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.1", i[3]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.2", i[3]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.1", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.2", i[4]);
            });

        db.SetValue(app.Config.CachedLocalDatabase, "testValue03", "0", "2", "2.1", "2.1.1");
        db.SetValue(app.Config.CachedLocalDatabase, "testValue04", "0", "2", "2.1", "2.1.2");
        Assert.Equal("testValue03", db.GetValue(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.1"));
        Assert.Equal("testValue04", db.GetValue(app.Config.CachedLocalDatabase, "0", "2", "2.1", "2.1.2"));
        string[][] test2 = db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test2,
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.1", i[2]);
            },
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.2", i[2]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.1", i[3]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.2", i[3]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.1", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.2", i[4]);
            });

        db.SetValue(app.Config.CachedLocalDatabase, "testValue05", "0", "3", "3.1", "3.1.1", "3.1.1.1");
        db.SetValue(app.Config.CachedLocalDatabase, "testValue06", "0", "3", "3.1", "3.1.1", "3.1.1.2");
        Assert.Equal("testValue05", db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.Equal("testValue06", db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
        string[][] test3 = db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test3,
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.1", i[2]);
            },
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.2", i[2]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.1", i[3]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.2", i[3]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.1", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.2", i[4]);
            });


        db.SetValue(app.Config.CachedLocalDatabase, "testValue07", "0", "3", "3.1", "3.1.1", "3.1.1.3");
        db.SetValue(app.Config.CachedLocalDatabase, "testValue08", "0", "3", "3.1", "3.1.1", "3.1.1.4");
        db.SetValue(app.Config.CachedLocalDatabase, "testValue09", "0", "3", "3.1", "3.1.2", "3.1.2.1");
        db.SetValue(app.Config.CachedLocalDatabase, "testValue10", "0", "3", "3.1", "3.1.2", "3.1.2.2");
        db.SetValue(app.Config.CachedLocalDatabase, "testValue11", "0", "4", "4.1", "4.1.1", "4.1.1.1");
        db.SetValue(app.Config.CachedLocalDatabase, "testValue12", "0", "4", "4.1", "4.1.1", "4.1.1.2");
        Assert.Equal("testValue07", db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
        Assert.Equal("testValue08", db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
        Assert.Equal("testValue09", db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.2", "3.1.2.1"));
        Assert.Equal("testValue10", db.GetValue(app.Config.CachedLocalDatabase, "0", "3", "3.1", "3.1.2", "3.1.2.2"));
        Assert.Equal("testValue11", db.GetValue(app.Config.CachedLocalDatabase, "0", "4", "4.1", "4.1.1", "4.1.1.1"));
        Assert.Equal("testValue12", db.GetValue(app.Config.CachedLocalDatabase, "0", "4", "4.1", "4.1.1", "4.1.1.2"));
        string[][] test4 = db.GetRecursiveChildren(app.Config.CachedLocalDatabase, "0");
        Assert.Collection(test4,
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.1", i[2]);
            },
            i =>
            {
                Assert.Equal(3, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("1", i[1]);
                Assert.Equal("1.2", i[2]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.1", i[3]);
            },
            i =>
            {
                Assert.Equal(4, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("2", i[1]);
                Assert.Equal("2.1", i[2]);
                Assert.Equal("2.1.2", i[3]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.1", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.2", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.3", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.1", i[3]);
                Assert.Equal("3.1.1.4", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.2", i[3]);
                Assert.Equal("3.1.2.1", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("3", i[1]);
                Assert.Equal("3.1", i[2]);
                Assert.Equal("3.1.2", i[3]);
                Assert.Equal("3.1.2.2", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("4", i[1]);
                Assert.Equal("4.1", i[2]);
                Assert.Equal("4.1.1", i[3]);
                Assert.Equal("4.1.1.1", i[4]);
            },
            i =>
            {
                Assert.Equal(5, i.Length);
                Assert.Equal("0", i[0]);
                Assert.Equal("4", i[1]);
                Assert.Equal("4.1", i[2]);
                Assert.Equal("4.1.1", i[3]);
                Assert.Equal("4.1.1.2", i[4]);
            });

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.SetValue(app.Config.CachedLocalDatabase, "test"));
        Assert.Throws<StringNullOrEmptyException>(() => db.SetValue(app.Config.CachedLocalDatabase, "test", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.SetValue(app.Config.CachedLocalDatabase, "test", Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.SetValue(app.Config.CachedLocalDatabase, "test", "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.SetValue(app.Config.CachedLocalDatabase, "test", "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.SetValue(app.Config.CachedLocalDatabase, "test", new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.SetValue(app.Config.CachedLocalDatabase, "test", new string[] { "path", "" }));

        Assert.Throws<SerializerNotSupportedException>(() => db.SetValue<SetValueTest>("1", null, true));
        Assert.Throws<ArgumentException>(() => db.SetValue<string>(null, null, true));

        app.Dispose();
    }
}

public class TryGetValueOrChildrenTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.1"));
        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.2"));
        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("1.1", i.key);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("1", i.path[1]);
                    Assert.Equal("1.1", i.path[2]);
                },
                i =>
                {
                    Assert.Equal("1.2", i.key);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("1", i.path[1]);
                    Assert.Equal("1.2", i.path[2]);
                });
        }, "0", "1"));

        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.1"));
        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.2"));
        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("2.1", i.key);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                });
        }, "0", "2"));
        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("2.1.1", i.key);
                    Assert.Equal(4, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                    Assert.Equal("2.1.1", i.path[3]);
                },
                i =>
                {
                    Assert.Equal("2.1.2", i.key);
                    Assert.Equal(4, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                    Assert.Equal("2.1.2", i.path[3]);
                });
        }, "0", "2", "2.1"));

        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("3.1", i.key);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                });
        }, "0", "3"));
        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("3.1.1", i.key);
                    Assert.Equal(4, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                });
        }, "0", "3", "3.1"));
        Assert.True(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("3.1.1.1", i.key);
                    Assert.Equal(5, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                    Assert.Equal("3.1.1.1", i.path[4]);
                },
                i =>
                {
                    Assert.Equal("3.1.1.2", i.key);
                    Assert.Equal(5, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                    Assert.Equal("3.1.1.2", i.path[4]);
                });
        }, "0", "3", "3.1", "3.1.1"));

        Assert.False(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
        Assert.False(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
        Assert.False(db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "4"));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class TryGetValueOrPathTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, delegate
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.1"));
        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, delegate
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.2"));
        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, delegate
        {
            Assert.True(true);
        }, "0", "1"));

        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, delegate
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.1"));
        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, delegate
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.2"));
        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, delegate
        {
            Assert.True(true);
        }, "0", "2"));
        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, delegate
        {
            Assert.True(true);
        }, "0", "2", "2.1"));

        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, delegate
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, delegate
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, delegate
        {
            Assert.True(true);
        }, "0", "3"));
        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, delegate
        {
            Assert.True(true);
        }, "0", "3", "3.1"));
        Assert.True(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, delegate
        {
            Assert.True(true);
        }, "0", "3", "3.1", "3.1.1"));

        Assert.False(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, delegate
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
        Assert.False(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, delegate
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
        Assert.False(db.TryGetValueOrPath(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, delegate
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "4"));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrPath(app.Config.CachedLocalDatabase, delegate { }, delegate { }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrPath(app.Config.CachedLocalDatabase, delegate { }, delegate { }, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrPath(app.Config.CachedLocalDatabase, delegate { }, delegate { }, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrPath(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrPath(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrPath(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrPath(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class TryGetValueOrRecursiveChildrenTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.1"));
        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.2"));
        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(3, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("1", i[1]);
                    Assert.Equal("1.1", i[2]);
                },
                i =>
                {
                    Assert.Equal(3, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("1", i[1]);
                    Assert.Equal("1.2", i[2]);
                });
        }, "0", "1"));

        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.1"));
        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.2"));
        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(4, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("2", i[1]);
                    Assert.Equal("2.1", i[2]);
                    Assert.Equal("2.1.1", i[3]);
                },
                i =>
                {
                    Assert.Equal(4, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("2", i[1]);
                    Assert.Equal("2.1", i[2]);
                    Assert.Equal("2.1.2", i[3]);
                });
        }, "0", "2"));
        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(4, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("2", i[1]);
                    Assert.Equal("2.1", i[2]);
                    Assert.Equal("2.1.1", i[3]);
                },
                i =>
                {
                    Assert.Equal(4, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("2", i[1]);
                    Assert.Equal("2.1", i[2]);
                    Assert.Equal("2.1.2", i[3]);
                });
        }, "0", "2", "2.1"));

        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(5, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("3", i[1]);
                    Assert.Equal("3.1", i[2]);
                    Assert.Equal("3.1.1", i[3]);
                    Assert.Equal("3.1.1.1", i[4]);
                },
                i =>
                {
                    Assert.Equal(5, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("3", i[1]);
                    Assert.Equal("3.1", i[2]);
                    Assert.Equal("3.1.1", i[3]);
                    Assert.Equal("3.1.1.2", i[4]);
                });
        }, "0", "3"));
        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(5, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("3", i[1]);
                    Assert.Equal("3.1", i[2]);
                    Assert.Equal("3.1.1", i[3]);
                    Assert.Equal("3.1.1.1", i[4]);
                },
                i =>
                {
                    Assert.Equal(5, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("3", i[1]);
                    Assert.Equal("3.1", i[2]);
                    Assert.Equal("3.1.1", i[3]);
                    Assert.Equal("3.1.1.2", i[4]);
                });
        }, "0", "3", "3.1"));
        Assert.True(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(5, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("3", i[1]);
                    Assert.Equal("3.1", i[2]);
                    Assert.Equal("3.1.1", i[3]);
                    Assert.Equal("3.1.1.1", i[4]);
                },
                i =>
                {
                    Assert.Equal(5, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("3", i[1]);
                    Assert.Equal("3.1", i[2]);
                    Assert.Equal("3.1.1", i[3]);
                    Assert.Equal("3.1.1.2", i[4]);
                });
        }, "0", "3", "3.1", "3.1.1"));

        Assert.False(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
        Assert.False(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
        Assert.False(db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "4"));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class TryGetValueOrRecursiveRelativeChildrenTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.1"));
        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.2"));
        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Single(i);
                    Assert.Equal("1.1", i[0]);
                },
                i =>
                {
                    Assert.Single(i);
                    Assert.Equal("1.2", i[0]);
                });
        }, "0", "1"));

        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.1"));
        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.2"));
        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(2, i.Length);
                    Assert.Equal("2.1", i[0]);
                    Assert.Equal("2.1.1", i[1]);
                },
                i =>
                {
                    Assert.Equal(2, i.Length);
                    Assert.Equal("2.1", i[0]);
                    Assert.Equal("2.1.2", i[1]);
                });
        }, "0", "2"));
        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Single(i);
                    Assert.Equal("2.1.1", i[0]);
                },
                i =>
                {
                    Assert.Single(i);
                    Assert.Equal("2.1.2", i[0]);
                });
        }, "0", "2", "2.1"));

        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(3, i.Length);
                    Assert.Equal("3.1", i[0]);
                    Assert.Equal("3.1.1", i[1]);
                    Assert.Equal("3.1.1.1", i[2]);
                },
                i =>
                {
                    Assert.Equal(3, i.Length);
                    Assert.Equal("3.1", i[0]);
                    Assert.Equal("3.1.1", i[1]);
                    Assert.Equal("3.1.1.2", i[2]);
                });
        }, "0", "3"));
        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(2, i.Length);
                    Assert.Equal("3.1.1", i[0]);
                    Assert.Equal("3.1.1.1", i[1]);
                },
                i =>
                {
                    Assert.Equal(2, i.Length);
                    Assert.Equal("3.1.1", i[0]);
                    Assert.Equal("3.1.1.2", i[1]);
                });
        }, "0", "3", "3.1"));
        Assert.True(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Single(i);
                    Assert.Equal("3.1.1.1", i[0]);
                },
                i =>
                {
                    Assert.Single(i);
                    Assert.Equal("3.1.1.2", i[0]);
                });
        }, "0", "3", "3.1", "3.1.1"));

        Assert.False(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
        Assert.False(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
        Assert.False(db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "4"));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class TryGetValueOrRecursiveValueTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.1"));
        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.2"));
        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("1", i.path[1]);
                    Assert.Equal("1.1", i.path[2]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("1", i.path[1]);
                    Assert.Equal("1.2", i.path[2]);
                });
        }, "0", "1"));

        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.1"));
        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.2"));
        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(4, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                    Assert.Equal("2.1.1", i.path[3]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(4, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                    Assert.Equal("2.1.2", i.path[3]);
                });
        }, "0", "2"));
        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(4, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                    Assert.Equal("2.1.1", i.path[3]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(4, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                    Assert.Equal("2.1.2", i.path[3]);
                });
        }, "0", "2", "2.1"));

        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(5, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                    Assert.Equal("3.1.1.1", i.path[4]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(5, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                    Assert.Equal("3.1.1.2", i.path[4]);
                });
        }, "0", "3"));
        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(5, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                    Assert.Equal("3.1.1.1", i.path[4]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(5, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                    Assert.Equal("3.1.1.2", i.path[4]);
                });
        }, "0", "3", "3.1"));
        Assert.True(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(5, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                    Assert.Equal("3.1.1.1", i.path[4]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(5, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                    Assert.Equal("3.1.1.2", i.path[4]);
                });
        }, "0", "3", "3.1", "3.1.1"));

        Assert.False(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
        Assert.False(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
        Assert.False(db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "4"));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class TryGetValueOrRecursiveRelativeValueTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.1"));
        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.2"));
        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Single(i.path);
                    Assert.Equal("1.1", i.path[0]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Single(i.path);
                    Assert.Equal("1.2", i.path[0]);
                });
        }, "0", "1"));

        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.1"));
        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.2"));
        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(2, i.path.Length);
                    Assert.Equal("2.1", i.path[0]);
                    Assert.Equal("2.1.1", i.path[1]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(2, i.path.Length);
                    Assert.Equal("2.1", i.path[0]);
                    Assert.Equal("2.1.2", i.path[1]);
                });
        }, "0", "2"));
        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Single(i.path);
                    Assert.Equal("2.1.1", i.path[0]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Single(i.path);
                    Assert.Equal("2.1.2", i.path[0]);
                });
        }, "0", "2", "2.1"));

        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("3.1", i.path[0]);
                    Assert.Equal("3.1.1", i.path[1]);
                    Assert.Equal("3.1.1.1", i.path[2]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("3.1", i.path[0]);
                    Assert.Equal("3.1.1", i.path[1]);
                    Assert.Equal("3.1.1.2", i.path[2]);
                });
        }, "0", "3"));
        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(2, i.path.Length);
                    Assert.Equal("3.1.1", i.path[0]);
                    Assert.Equal("3.1.1.1", i.path[1]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Equal(2, i.path.Length);
                    Assert.Equal("3.1.1", i.path[0]);
                    Assert.Equal("3.1.1.2", i.path[1]);
                });
        }, "0", "3", "3.1"));
        Assert.True(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Single(i.path);
                    Assert.Equal("3.1.1.1", i.path[0]);
                },
                i =>
                {
                    Assert.Equal("test", i.value);
                    Assert.Single(i.path);
                    Assert.Equal("3.1.1.2", i.path[0]);
                });
        }, "0", "3", "3.1", "3.1.1"));

        Assert.False(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
        Assert.False(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
        Assert.False(db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "4"));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRecursiveRelativeValues(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class TryGetValueOrRelativeTypedChildrenTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.1"));
        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.2"));
        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("1.1", i.key);
                    Assert.Equal(LocalDataType.Value, i.type);
                },
                i =>
                {
                    Assert.Equal("1.2", i.key);
                    Assert.Equal(LocalDataType.Value, i.type);
                });
        }, "0", "1"));

        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.1"));
        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.2"));
        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("2.1", i.key);
                    Assert.Equal(LocalDataType.Path, i.type);
                });
        }, "0", "2"));
        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("2.1.1", i.key);
                    Assert.Equal(LocalDataType.Value, i.type);
                },
                i =>
                {
                    Assert.Equal("2.1.2", i.key);
                    Assert.Equal(LocalDataType.Value, i.type);
                });
        }, "0", "2", "2.1"));

        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("3.1", i.key);
                    Assert.Equal(LocalDataType.Path, i.type);
                });
        }, "0", "3"));
        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("3.1.1", i.key);
                    Assert.Equal(LocalDataType.Path, i.type);
                });
        }, "0", "3", "3.1"));
        Assert.True(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal("3.1.1.1", i.key);
                    Assert.Equal(LocalDataType.Value, i.type);
                },
                i =>
                {
                    Assert.Equal("3.1.1.2", i.key);
                    Assert.Equal(LocalDataType.Value, i.type);
                });
        }, "0", "3", "3.1", "3.1.1"));

        Assert.False(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
        Assert.False(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
        Assert.False(db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "4"));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrRelativeTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", "" }));

        app.Dispose();
    }
}

public class TryGetValueOrTypedChildrenTest
{
    [Fact]
    public async void Normal()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.1"));
        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "1", "1.2"));
        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(LocalDataType.Value, i.type);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("1", i.path[1]);
                    Assert.Equal("1.1", i.path[2]);
                },
                i =>
                {
                    Assert.Equal(LocalDataType.Value, i.type);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("1", i.path[1]);
                    Assert.Equal("1.2", i.path[2]);
                });
        }, "0", "1"));

        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.1"));
        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "2", "2.1", "2.1.2"));
        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(LocalDataType.Path, i.type);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                });
        }, "0", "2"));
        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(LocalDataType.Value, i.type);
                    Assert.Equal(4, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                    Assert.Equal("2.1.1", i.path[3]);
                },
                i =>
                {
                    Assert.Equal(LocalDataType.Value, i.type);
                    Assert.Equal(4, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                    Assert.Equal("2.1.2", i.path[3]);
                });
        }, "0", "2", "2.1"));

        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.Equal("test", value);
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(LocalDataType.Path, i.type);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                });
        }, "0", "3"));
        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(LocalDataType.Path, i.type);
                    Assert.Equal(4, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                });
        }, "0", "3", "3.1"));
        Assert.True(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.Collection(children,
                i =>
                {
                    Assert.Equal(LocalDataType.Value, i.type);
                    Assert.Equal(5, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                    Assert.Equal("3.1.1.1", i.path[4]);
                },
                i =>
                {
                    Assert.Equal(LocalDataType.Value, i.type);
                    Assert.Equal(5, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                    Assert.Equal("3.1.1", i.path[3]);
                    Assert.Equal("3.1.1.2", i.path[4]);
                });
        }, "0", "3", "3.1", "3.1.1"));

        Assert.False(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
        Assert.False(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
        Assert.False(db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, value =>
        {
            Assert.True(false, "Path contains value.");
        }, children =>
        {
            Assert.True(false, "Path contains another path.");
        }, "0", "4"));

        app.Dispose();
    }

    [Fact]
    public async void Throws()
    {
        var app = await Helpers.Hier();
        var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
        var db = app.LocalDatabase;

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, Array.Empty<string>()));

        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", null));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, "path", ""));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", null }));
        Assert.Throws<StringNullOrEmptyException>(() => db.TryGetValueOrTypedChildren(app.Config.CachedLocalDatabase, delegate { }, delegate { }, new string[] { "path", "" }));

        app.Dispose();
    }
}

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.