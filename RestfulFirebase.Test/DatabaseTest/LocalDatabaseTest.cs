using RestfulFirebase;
using RestfulFirebase.Database.Models;
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

namespace LocalDatabaseTest
{
    public class SampleLocalDatabase : ILocalDatabase
    {
        public ConcurrentDictionary<string, string> db { get; } = new ConcurrentDictionary<string, string>();

        public bool ContainsKey(string key)
        {
            return db.ContainsKey(key);
        }

        public string? Get(string key)
        {
            if (!db.TryGetValue(key, out string? value))
            {
                return null;
            }
            return value;
        }

        public void Set(string key, string value)
        {
            db.AddOrUpdate(key, value, delegate { return value; });
        }

        public void Delete(string key)
        {
            db.TryRemove(key, out _);
        }

        public void Clear()
        {
            db.Clear();
        }
    }

    public class Helpers
    {
        public static RestfulFirebaseApp EmptySample()
        {
            FirebaseConfig config = Config.YourConfig();
            config.LocalDatabase = new SampleLocalDatabase();
            var app = new RestfulFirebaseApp(config);
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            return app;
        }

        public static RestfulFirebaseApp HierSample()
        {
            FirebaseConfig config = Config.YourConfig();
            config.LocalDatabase = new SampleLocalDatabase();
            var app = new RestfulFirebaseApp(config);
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            db.SetValue("test", "0", "1", "1.1");
            db.SetValue("test", "0", "1", "1.2");

            db.SetValue("test", "0", "2", "2.1", "2.1.1");
            db.SetValue("test", "0", "2", "2.1", "2.1.2");

            db.SetValue("test", "0", "3", "3.1", "3.1.1", "3.1.1.1");
            db.SetValue("test", "0", "3", "3.1", "3.1.1", "3.1.1.2");

            return app;
        }

        public static (string[] path, string key)[] GetChildren(LocalDatabaseApp db, params string[] path)
        {
            return db.GetChildren(path);
        }

        public static (string key, LocalDataType type)[] GetRelativeTypedChildren(LocalDatabaseApp db, params string[] path)
        {
            return db.GetRelativeTypedChildren(path);
        }

        public static (string[] path, LocalDataType type)[] GetTypedChildren(LocalDatabaseApp db, params string[] path)
        {
            return db.GetTypedChildren(path);
        }

        public static string[][] GetRecursiveChildren(LocalDatabaseApp db, params string[] path)
        {
            return db.GetRecursiveChildren(path);
        }

        public static string[][] GetRecursiveRelativeChildren(LocalDatabaseApp db, params string[] path)
        {
            return db.GetRecursiveRelativeChildren(path);
        }

        public static string GetValue(LocalDatabaseApp db, params string[] path)
        {
            return db.GetValue(path);
        }
    }

    public class ContainsTest
    {
        [Fact]
        public void Normal()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.True(db.Contains("0", "1", "1.1"));
            Assert.True(db.Contains("0", "1", "1.2"));

            Assert.True(db.Contains("0", "2", "2.1", "2.1.1"));
            Assert.True(db.Contains("0", "2", "2.1", "2.1.2"));

            Assert.True(db.Contains("0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.True(db.Contains("0", "3", "3.1", "3.1.1", "3.1.1.2"));

            Assert.False(db.Contains("0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.False(db.Contains("0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.False(db.Contains("0", "4"));
            Assert.False(db.Contains("0", "4"));
        }

        [Fact]
        public void Throws()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains(new string[] { "path", "" }));
        }
    }

    public class DeleteTest
    {
        [Fact]
        public void Normal()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            db.Delete("0", "1", "1.1");
            Assert.Null(Helpers.GetValue(db, "0", "1", "1.1"));
            string[][] test1 = Helpers.GetRecursiveChildren(db, "0");
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

            db.Delete("0", "2");
            Assert.Null(Helpers.GetValue(db, "0", "2"));
            string[][] test2 = Helpers.GetRecursiveChildren(db, "0");
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

            db.Delete("0", "3", "3.1");
            Assert.Null(Helpers.GetValue(db, "0", "3", "3.1"));
            string[][] test3 = Helpers.GetRecursiveChildren(db, "0");
            Assert.Collection(test3,
                i =>
                {
                    Assert.Equal(3, i.Length);
                    Assert.Equal("0", i[0]);
                    Assert.Equal("1", i[1]);
                    Assert.Equal("1.2", i[2]);
                });
        }

        [Fact]
        public void Throws()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete(new string[] { "path", "" }));
        }
    }

    public class GetChildrenTest
    {
        [Fact]
        public void Normal()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            (string[] path, string key)[] test1 = db.GetChildren("0");
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

            (string[] path, string key)[] test2 = db.GetChildren("0", "1");
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

            (string[] path, string key)[] test3 = db.GetChildren("0", "2");
            Assert.Collection(test3,
                i =>
                {
                    Assert.Equal("2.1", i.key);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                });

            (string[] path, string key)[] test4 = db.GetChildren("0", "2", "2.1");
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

            (string[] path, string key)[] test5 = db.GetChildren("0", "3");
            Assert.Collection(test5,
                i =>
                {
                    Assert.Equal("3.1", i.key);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                });

            (string[] path, string key)[] test6 = db.GetChildren("0", "3", "3.1");
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

            (string[] path, string key)[] test7 = db.GetChildren("0", "3", "3.1", "3.1.1");
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
        }

        [Fact]
        public void Throws()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren(new string[] { "path", "" }));
        }
    }

    public class GetRelativeTypedChildrenTest
    {
        [Fact]
        public void Normal()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            (string key, LocalDataType type)[] test1 = db.GetRelativeTypedChildren("0");
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

            (string key, LocalDataType type)[] test2 = db.GetRelativeTypedChildren("0", "1");
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

            (string key, LocalDataType type)[] test3 = db.GetRelativeTypedChildren("0", "2");
            Assert.Collection(test3,
                i =>
                {
                    Assert.Equal("2.1", i.key);
                    Assert.Equal(LocalDataType.Path, i.type);
                });

            (string key, LocalDataType type)[] test4 = db.GetRelativeTypedChildren("0", "2", "2.1");
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

            (string key, LocalDataType type)[] test5 = db.GetRelativeTypedChildren("0", "3");
            Assert.Collection(test5,
                i =>
                {
                    Assert.Equal("3.1", i.key);
                    Assert.Equal(LocalDataType.Path, i.type);
                });

            (string key, LocalDataType type)[] test6 = db.GetRelativeTypedChildren("0", "3", "3.1");
            Assert.Collection(test6,
                i =>
                {
                    Assert.Equal("3.1.1", i.key);
                    Assert.Equal(LocalDataType.Path, i.type);
                });

            (string key, LocalDataType type)[] test7 = db.GetRelativeTypedChildren("0", "3", "3.1", "3.1.1");
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
        }

        [Fact]
        public void Throws()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren(new string[] { "path", "" }));
        }
    }

    public class GetTypedChildrenTest
    {
        [Fact]
        public void Normal()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            (string[] path, string key)[] test1 = db.GetChildren("0");
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

            (string[] path, string key)[] test2 = db.GetChildren("0", "1");
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

            (string[] path, string key)[] test3 = db.GetChildren("0", "2");
            Assert.Collection(test3,
                i =>
                {
                    Assert.Equal("2.1", i.key);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("2", i.path[1]);
                    Assert.Equal("2.1", i.path[2]);
                });

            (string[] path, string key)[] test4 = db.GetChildren("0", "2", "2.1");
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

            (string[] path, string key)[] test5 = db.GetChildren("0", "3");
            Assert.Collection(test5,
                i =>
                {
                    Assert.Equal("3.1", i.key);
                    Assert.Equal(3, i.path.Length);
                    Assert.Equal("0", i.path[0]);
                    Assert.Equal("3", i.path[1]);
                    Assert.Equal("3.1", i.path[2]);
                });

            (string[] path, string key)[] test6 = db.GetChildren("0", "3", "3.1");
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

            (string[] path, string key)[] test7 = db.GetChildren("0", "3", "3.1", "3.1.1");
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
        }

        [Fact]
        public void Throws()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren(new string[] { "path", "" }));
        }
    }

    public class GetDataTypeTest
    {
        [Fact]
        public void Normal()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Equal(LocalDataType.Value, db.GetDataType("0", "1", "1.1"));
            Assert.Equal(LocalDataType.Value, db.GetDataType("0", "1", "1.2"));
            Assert.Equal(LocalDataType.Value, db.GetDataType("0", "2", "2.1", "2.1.1"));
            Assert.Equal(LocalDataType.Value, db.GetDataType("0", "2", "2.1", "2.1.2"));
            Assert.Equal(LocalDataType.Value, db.GetDataType("0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.Equal(LocalDataType.Value, db.GetDataType("0", "3", "3.1", "3.1.1", "3.1.1.2"));

            Assert.Equal(LocalDataType.Path, db.GetDataType("0", "1"));
            Assert.Equal(LocalDataType.Path, db.GetDataType("0", "1"));
            Assert.Equal(LocalDataType.Path, db.GetDataType("0", "2", "2.1"));
            Assert.Equal(LocalDataType.Path, db.GetDataType("0", "2", "2.1"));
            Assert.Equal(LocalDataType.Path, db.GetDataType("0", "3", "3.1", "3.1.1"));
            Assert.Equal(LocalDataType.Path, db.GetDataType("0", "3", "3.1", "3.1.1"));

            Assert.Equal(LocalDataType.Path, db.GetDataType("0"));
            Assert.Equal(LocalDataType.Path, db.GetDataType("0", "2"));
            Assert.Equal(LocalDataType.Path, db.GetDataType("0", "3", "3.1"));

            Assert.Equal(LocalDataType.Path, db.GetDataType("0", "3"));

            Assert.Equal(LocalDataType.Path, db.GetDataType("0"));

            // Null values
            Assert.Equal(LocalDataType.Value, db.GetDataType("0", "1", "1.3"));
            Assert.Equal(LocalDataType.Value, db.GetDataType("0", "1", "1.4"));
            Assert.Equal(LocalDataType.Value, db.GetDataType("0", "2", "2.1", "2.1.1", "2.1.1.1"));
            Assert.Equal(LocalDataType.Value, db.GetDataType("0", "2", "2.1", "2.1.2", "2.1.2.1"));
            Assert.Equal(LocalDataType.Value, db.GetDataType("0", "4"));
            Assert.Equal(LocalDataType.Value, db.GetDataType("1"));
        }

        [Fact]
        public void Throws()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType(new string[] { "path", "" }));
        }
    }

    public class GetRecursiveChildrenTest
    {
        [Fact]
        public void Normal()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            string[][] test1 = db.GetRecursiveChildren("0");
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
        }

        [Fact]
        public void Throws()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren(new string[] { "path", "" }));
        }
    }
    
    public class GetRecursiveRelativeChildrenTest
    {
        [Fact]
        public void Normal()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            string[][] test1 = db.GetRecursiveRelativeChildren("0");
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
        }

        [Fact]
        public void Throws()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren(new string[] { "path", "" }));
        }
    }

    public class GetValueTest
    {
        [Fact]
        public void Normal()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Equal("test", db.GetValue("0", "1", "1.1"));
            Assert.Equal("test", db.GetValue("0", "1", "1.2"));
            Assert.Null(db.GetValue("0", "1"));

            Assert.Equal("test", db.GetValue("0", "2", "2.1", "2.1.1"));
            Assert.Equal("test", db.GetValue("0", "2", "2.1", "2.1.2"));
            Assert.Null(db.GetValue("0", "2"));
            Assert.Null(db.GetValue("0", "2", "2.1"));

            Assert.Equal("test", db.GetValue("0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.Equal("test", db.GetValue("0", "3", "3.1", "3.1.1", "3.1.1.2"));
            Assert.Null(db.GetValue("0", "3"));
            Assert.Null(db.GetValue("0", "3", "3.1"));
            Assert.Null(db.GetValue("0", "3", "3.1", "3.1.1"));

            Assert.Null(db.GetValue("0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.Null(db.GetValue("0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.Null(db.GetValue("0", "4"));
            Assert.Null(db.GetValue("0", "4"));
        }

        [Fact]
        public void Throws()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue(new string[] { "path", "" }));
        }
    }

    public class SetTest
    {
        [Fact]
        public void Normal()
        {
            var app = Helpers.EmptySample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            db.SetValue("testValue01", "0", "1", "1.1");
            db.SetValue("testValue02", "0", "1", "1.2");
            Assert.Equal("testValue01", Helpers.GetValue(db, "0", "1", "1.1"));
            Assert.Equal("testValue02", Helpers.GetValue(db, "0", "1", "1.2"));
            string[][] test1 = Helpers.GetRecursiveChildren(db, "0");
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

            db.SetValue("testValue03", "0", "2", "2.1", "2.1.1");
            db.SetValue("testValue04", "0", "2", "2.1", "2.1.2");
            Assert.Equal("testValue03", Helpers.GetValue(db, "0", "2", "2.1", "2.1.1"));
            Assert.Equal("testValue04", Helpers.GetValue(db, "0", "2", "2.1", "2.1.2"));
            string[][] test2 = Helpers.GetRecursiveChildren(db, "0");
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

            db.SetValue("testValue05", "0", "3", "3.1", "3.1.1", "3.1.1.1");
            db.SetValue("testValue06", "0", "3", "3.1", "3.1.1", "3.1.1.2");
            Assert.Equal("testValue05", Helpers.GetValue(db, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.Equal("testValue06", Helpers.GetValue(db, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
            string[][] test3 = Helpers.GetRecursiveChildren(db, "0");
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
        }

        [Fact]
        public void WithExistingData()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            db.SetValue("testValue01", "0", "1", "1.1");
            db.SetValue("testValue02", "0", "1", "1.2");
            Assert.Equal("testValue01", Helpers.GetValue(db, "0", "1", "1.1"));
            Assert.Equal("testValue02", Helpers.GetValue(db, "0", "1", "1.2"));
            string[][] test1 = Helpers.GetRecursiveChildren(db, "0");
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

            db.SetValue("testValue03", "0", "2", "2.1", "2.1.1");
            db.SetValue("testValue04", "0", "2", "2.1", "2.1.2");
            Assert.Equal("testValue03", Helpers.GetValue(db, "0", "2", "2.1", "2.1.1"));
            Assert.Equal("testValue04", Helpers.GetValue(db, "0", "2", "2.1", "2.1.2"));
            string[][] test2 = Helpers.GetRecursiveChildren(db, "0");
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

            db.SetValue("testValue05", "0", "3", "3.1", "3.1.1", "3.1.1.1");
            db.SetValue("testValue06", "0", "3", "3.1", "3.1.1", "3.1.1.2");
            Assert.Equal("testValue05", Helpers.GetValue(db, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.Equal("testValue06", Helpers.GetValue(db, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
            string[][] test3 = Helpers.GetRecursiveChildren(db, "0");
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


            db.SetValue("testValue07", "0", "3", "3.1", "3.1.1", "3.1.1.3");
            db.SetValue("testValue08", "0", "3", "3.1", "3.1.1", "3.1.1.4");
            db.SetValue("testValue09", "0", "3", "3.1", "3.1.2", "3.1.2.1");
            db.SetValue("testValue10", "0", "3", "3.1", "3.1.2", "3.1.2.2");
            db.SetValue("testValue11", "0", "4", "4.1", "4.1.1", "4.1.1.1");
            db.SetValue("testValue12", "0", "4", "4.1", "4.1.1", "4.1.1.2");
            Assert.Equal("testValue07", Helpers.GetValue(db, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.Equal("testValue08", Helpers.GetValue(db, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.Equal("testValue09", Helpers.GetValue(db, "0", "3", "3.1", "3.1.2", "3.1.2.1"));
            Assert.Equal("testValue10", Helpers.GetValue(db, "0", "3", "3.1", "3.1.2", "3.1.2.2"));
            Assert.Equal("testValue11", Helpers.GetValue(db, "0", "4", "4.1", "4.1.1", "4.1.1.1"));
            Assert.Equal("testValue12", Helpers.GetValue(db, "0", "4", "4.1", "4.1.1", "4.1.1.2"));
            string[][] test4 = Helpers.GetRecursiveChildren(db, "0");
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
        }

        [Fact]
        public void Throws()
        {
            var app = Helpers.HierSample();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test"));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", "path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", "path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", new string[] { "path", "" }));
        }
    }
}
