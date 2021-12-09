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

namespace DatabaseTest.LocalDatabaseTest
{
    public static class Helpers
    {
        public static Task<RestfulFirebaseApp> Empty()
        {
            return RestfulFirebase.Test.Helpers.AppGenerator()();
        }

        public static async Task<RestfulFirebaseApp> Hier()
        {
            var generator = RestfulFirebase.Test.Helpers.AppGenerator();
            var app = await generator();

            app.LocalDatabase.SetValue("test", "0", "1", "1.1");
            app.LocalDatabase.SetValue("test", "0", "1", "1.2");

            app.LocalDatabase.SetValue("test", "0", "2", "2.1", "2.1.1");
            app.LocalDatabase.SetValue("test", "0", "2", "2.1", "2.1.2");

            app.LocalDatabase.SetValue("test", "0", "3", "3.1", "3.1.1", "3.1.1.1");
            app.LocalDatabase.SetValue("test", "0", "3", "3.1", "3.1.1", "3.1.1.2");

            return app;
        }
    }

    public class ContainsTest
    {
        [Fact]
        public async void Normal()
        {
            var app = await Helpers.Hier();
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

            app.Dispose();
        }

        [Fact]
        public async void Throws()
        {
            var app = await Helpers.Hier();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Contains(new string[] { "path", "" }));

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

            db.Delete("0", "1", "1.1");
            Assert.Null(db.GetValue("0", "1", "1.1"));
            string[][] test1 = db.GetRecursiveChildren("0");
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
            Assert.Null(db.GetValue("0", "2"));
            string[][] test2 = db.GetRecursiveChildren("0");
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
            Assert.Null(db.GetValue("0", "3", "3.1"));
            string[][] test3 = db.GetRecursiveChildren("0");
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

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.Delete(new string[] { "path", "" }));

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

            app.Dispose();
        }

        [Fact]
        public async void Throws()
        {
            var app = await Helpers.Hier();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetChildren(new string[] { "path", "" }));

            app.Dispose();
        }
    }

    public class GetDataTypeTest
    {
        [Fact]
        public async void Normal()
        {
            var app = await Helpers.Hier();
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

            app.Dispose();
        }

        [Fact]
        public async void Throws()
        {
            var app = await Helpers.Hier();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetDataType(new string[] { "path", "" }));

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

            app.Dispose();
        }

        [Fact]
        public async void Throws()
        {
            var app = await Helpers.Hier();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveChildren(new string[] { "path", "" }));

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

            app.Dispose();
        }

        [Fact]
        public async void Throws()
        {
            var app = await Helpers.Hier();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRecursiveRelativeChildren(new string[] { "path", "" }));

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

            app.Dispose();
        }

        [Fact]
        public async void Throws()
        {
            var app = await Helpers.Hier();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetRelativeTypedChildren(new string[] { "path", "" }));

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

            app.Dispose();
        }

        [Fact]
        public async void Throws()
        {
            var app = await Helpers.Hier();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetTypedChildren(new string[] { "path", "" }));

            app.Dispose();
        }
    }

    public class GetValueTest
    {
        [Fact]
        public async void Normal()
        {
            var app = await Helpers.Hier();
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

            app.Dispose();
        }

        [Fact]
        public async void Throws()
        {
            var app = await Helpers.Hier();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue());
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue(null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue(new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue("path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue("path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue(new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.GetValue(new string[] { "path", "" }));

            app.Dispose();
        }
    }

    public class SetTest
    {
        [Fact]
        public async void Normal()
        {
            var app = await Helpers.Empty();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            db.SetValue("testValue01", "0", "1", "1.1");
            db.SetValue("testValue02", "0", "1", "1.2");
            Assert.Equal("testValue01", db.GetValue("0", "1", "1.1"));
            Assert.Equal("testValue02", db.GetValue("0", "1", "1.2"));
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
                });

            db.SetValue("testValue03", "0", "2", "2.1", "2.1.1");
            db.SetValue("testValue04", "0", "2", "2.1", "2.1.2");
            Assert.Equal("testValue03", db.GetValue("0", "2", "2.1", "2.1.1"));
            Assert.Equal("testValue04", db.GetValue("0", "2", "2.1", "2.1.2"));
            string[][] test2 = db.GetRecursiveChildren("0");
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
            Assert.Equal("testValue05", db.GetValue("0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.Equal("testValue06", db.GetValue("0", "3", "3.1", "3.1.1", "3.1.1.2"));
            string[][] test3 = db.GetRecursiveChildren("0");
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

            app.Dispose();
        }

        [Fact]
        public async void WithExistingData()
        {
            var app = await Helpers.Hier();
            var dbConfig = app.Config.LocalDatabase as SampleLocalDatabase;
            var db = app.LocalDatabase;

            db.SetValue("testValue01", "0", "1", "1.1");
            db.SetValue("testValue02", "0", "1", "1.2");
            Assert.Equal("testValue01", db.GetValue("0", "1", "1.1"));
            Assert.Equal("testValue02", db.GetValue("0", "1", "1.2"));
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

            db.SetValue("testValue03", "0", "2", "2.1", "2.1.1");
            db.SetValue("testValue04", "0", "2", "2.1", "2.1.2");
            Assert.Equal("testValue03", db.GetValue("0", "2", "2.1", "2.1.1"));
            Assert.Equal("testValue04", db.GetValue("0", "2", "2.1", "2.1.2"));
            string[][] test2 = db.GetRecursiveChildren("0");
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
            Assert.Equal("testValue05", db.GetValue("0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.Equal("testValue06", db.GetValue("0", "3", "3.1", "3.1.1", "3.1.1.2"));
            string[][] test3 = db.GetRecursiveChildren("0");
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
            Assert.Equal("testValue07", db.GetValue("0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.Equal("testValue08", db.GetValue("0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.Equal("testValue09", db.GetValue("0", "3", "3.1", "3.1.2", "3.1.2.1"));
            Assert.Equal("testValue10", db.GetValue("0", "3", "3.1", "3.1.2", "3.1.2.2"));
            Assert.Equal("testValue11", db.GetValue("0", "4", "4.1", "4.1.1", "4.1.1.1"));
            Assert.Equal("testValue12", db.GetValue("0", "4", "4.1", "4.1.1", "4.1.1.2"));
            string[][] test4 = db.GetRecursiveChildren("0");
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

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test"));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", "path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", "path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.SetValue("test", new string[] { "path", "" }));

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

            Assert.True(db.TryGetValueOrChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.1"));
            Assert.True(db.TryGetValueOrChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.2"));
            Assert.True(db.TryGetValueOrChildren(value =>
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

            Assert.True(db.TryGetValueOrChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.1"));
            Assert.True(db.TryGetValueOrChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.2"));
            Assert.True(db.TryGetValueOrChildren(value =>
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
            Assert.True(db.TryGetValueOrChildren(value =>
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

            Assert.True(db.TryGetValueOrChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.True(db.TryGetValueOrChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
            Assert.True(db.TryGetValueOrChildren(value =>
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
            Assert.True(db.TryGetValueOrChildren(value =>
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
            Assert.True(db.TryGetValueOrChildren(value =>
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

            Assert.False(db.TryGetValueOrChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.False(db.TryGetValueOrChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.False(db.TryGetValueOrChildren(value =>
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

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrChildren(delegate { }, delegate { }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrChildren(delegate { }, delegate { }, null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrChildren(delegate { }, delegate { }, new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrChildren(delegate { }, delegate { }, "path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrChildren(delegate { }, delegate { }, "path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrChildren(delegate { }, delegate { }, new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrChildren(delegate { }, delegate { }, new string[] { "path", "" }));

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

            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.Equal("test", value);
            }, delegate
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.1"));
            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.Equal("test", value);
            }, delegate
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.2"));
            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.True(false, "Path contains value.");
            }, delegate
            {
                Assert.True(true);
            }, "0", "1"));

            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.Equal("test", value);
            }, delegate
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.1"));
            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.Equal("test", value);
            }, delegate
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.2"));
            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.True(false, "Path contains value.");
            }, delegate
            {
                Assert.True(true);
            }, "0", "2"));
            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.True(false, "Path contains value.");
            }, delegate
            {
                Assert.True(true);
            }, "0", "2", "2.1"));

            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.Equal("test", value);
            }, delegate
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.Equal("test", value);
            }, delegate
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.True(false, "Path contains value.");
            }, delegate
            {
                Assert.True(true);
            }, "0", "3"));
            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.True(false, "Path contains value.");
            }, delegate
            {
                Assert.True(true);
            }, "0", "3", "3.1"));
            Assert.True(db.TryGetValueOrPath(value =>
            {
                Assert.True(false, "Path contains value.");
            }, delegate
            {
                Assert.True(true);
            }, "0", "3", "3.1", "3.1.1"));

            Assert.False(db.TryGetValueOrPath(value =>
            {
                Assert.True(false, "Path contains value.");
            }, delegate
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.False(db.TryGetValueOrPath(value =>
            {
                Assert.True(false, "Path contains value.");
            }, delegate
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.False(db.TryGetValueOrPath(value =>
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

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrPath(delegate { }, delegate { }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrPath(delegate { }, delegate { }, null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrPath(delegate { }, delegate { }, new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrPath(delegate { }, delegate { }, "path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrPath(delegate { }, delegate { }, "path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrPath(delegate { }, delegate { }, new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrPath(delegate { }, delegate { }, new string[] { "path", "" }));

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

            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.1"));
            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.2"));
            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
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

            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.1"));
            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.2"));
            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
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
            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
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

            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
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
            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
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
            Assert.True(db.TryGetValueOrRecursiveChildren(value =>
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

            Assert.False(db.TryGetValueOrRecursiveChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.False(db.TryGetValueOrRecursiveChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.False(db.TryGetValueOrRecursiveChildren(value =>
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

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveChildren(delegate { }, delegate { }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveChildren(delegate { }, delegate { }, null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveChildren(delegate { }, delegate { }, new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveChildren(delegate { }, delegate { }, "path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveChildren(delegate { }, delegate { }, "path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveChildren(delegate { }, delegate { }, new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveChildren(delegate { }, delegate { }, new string[] { "path", "" }));

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

            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.1"));
            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.2"));
            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.Collection(children,
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("1.1", i[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("1.2", i[0]);
                    });
            }, "0", "1"));

            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.1"));
            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.2"));
            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
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
            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.Collection(children,
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("2.1.1", i[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("2.1.2", i[0]);
                    });
            }, "0", "2", "2.1"));

            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
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
            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
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
            Assert.True(db.TryGetValueOrRecursiveRelativeChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.Collection(children,
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("3.1.1.1", i[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("3.1.1.2", i[0]);
                    });
            }, "0", "3", "3.1", "3.1.1"));

            Assert.False(db.TryGetValueOrRecursiveRelativeChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.False(db.TryGetValueOrRecursiveRelativeChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.False(db.TryGetValueOrRecursiveRelativeChildren(value =>
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

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeChildren(delegate { }, delegate { }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeChildren(delegate { }, delegate { }, null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeChildren(delegate { }, delegate { }, new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeChildren(delegate { }, delegate { }, "path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeChildren(delegate { }, delegate { }, "path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeChildren(delegate { }, delegate { }, new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeChildren(delegate { }, delegate { }, new string[] { "path", "" }));

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

            Assert.True(db.TryGetValueOrRecursiveValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.1"));
            Assert.True(db.TryGetValueOrRecursiveValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.2"));
            Assert.True(db.TryGetValueOrRecursiveValues(value =>
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

            Assert.True(db.TryGetValueOrRecursiveValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.1"));
            Assert.True(db.TryGetValueOrRecursiveValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.2"));
            Assert.True(db.TryGetValueOrRecursiveValues(value =>
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
            Assert.True(db.TryGetValueOrRecursiveValues(value =>
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

            Assert.True(db.TryGetValueOrRecursiveValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.True(db.TryGetValueOrRecursiveValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
            Assert.True(db.TryGetValueOrRecursiveValues(value =>
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
            Assert.True(db.TryGetValueOrRecursiveValues(value =>
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
            Assert.True(db.TryGetValueOrRecursiveValues(value =>
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

            Assert.False(db.TryGetValueOrRecursiveValues(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.False(db.TryGetValueOrRecursiveValues(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.False(db.TryGetValueOrRecursiveValues(value =>
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

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveValues(delegate { }, delegate { }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveValues(delegate { }, delegate { }, null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveValues(delegate { }, delegate { }, new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveValues(delegate { }, delegate { }, "path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveValues(delegate { }, delegate { }, "path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveValues(delegate { }, delegate { }, new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveValues(delegate { }, delegate { }, new string[] { "path", "" }));

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

            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.1"));
            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.2"));
            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.Collection(children,
                    i =>
                    {
                        Assert.Equal("test", i.value);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("1.1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test", i.value);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("1.2", i.path[0]);
                    });
            }, "0", "1"));

            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.1"));
            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.2"));
            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
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
            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.Collection(children,
                    i =>
                    {
                        Assert.Equal("test", i.value);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("2.1.1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test", i.value);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("2.1.2", i.path[0]);
                    });
            }, "0", "2", "2.1"));

            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
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
            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
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
            Assert.True(db.TryGetValueOrRecursiveRelativeValues(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.Collection(children,
                    i =>
                    {
                        Assert.Equal("test", i.value);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("3.1.1.1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test", i.value);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("3.1.1.2", i.path[0]);
                    });
            }, "0", "3", "3.1", "3.1.1"));

            Assert.False(db.TryGetValueOrRecursiveRelativeValues(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.False(db.TryGetValueOrRecursiveRelativeValues(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.False(db.TryGetValueOrRecursiveRelativeValues(value =>
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

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeValues(delegate { }, delegate { }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeValues(delegate { }, delegate { }, null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeValues(delegate { }, delegate { }, new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeValues(delegate { }, delegate { }, "path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeValues(delegate { }, delegate { }, "path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeValues(delegate { }, delegate { }, new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRecursiveRelativeValues(delegate { }, delegate { }, new string[] { "path", "" }));

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

            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.1"));
            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.2"));
            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
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

            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.1"));
            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.2"));
            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
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
            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
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

            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
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
            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
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
            Assert.True(db.TryGetValueOrRelativeTypedChildren(value =>
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

            Assert.False(db.TryGetValueOrRelativeTypedChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.False(db.TryGetValueOrRelativeTypedChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.False(db.TryGetValueOrRelativeTypedChildren(value =>
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

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRelativeTypedChildren(delegate { }, delegate { }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRelativeTypedChildren(delegate { }, delegate { }, null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRelativeTypedChildren(delegate { }, delegate { }, new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRelativeTypedChildren(delegate { }, delegate { }, "path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRelativeTypedChildren(delegate { }, delegate { }, "path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRelativeTypedChildren(delegate { }, delegate { }, new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrRelativeTypedChildren(delegate { }, delegate { }, new string[] { "path", "" }));

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

            Assert.True(db.TryGetValueOrTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.1"));
            Assert.True(db.TryGetValueOrTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "1", "1.2"));
            Assert.True(db.TryGetValueOrTypedChildren(value =>
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

            Assert.True(db.TryGetValueOrTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.1"));
            Assert.True(db.TryGetValueOrTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "2", "2.1", "2.1.2"));
            Assert.True(db.TryGetValueOrTypedChildren(value =>
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
            Assert.True(db.TryGetValueOrTypedChildren(value =>
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

            Assert.True(db.TryGetValueOrTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.1"));
            Assert.True(db.TryGetValueOrTypedChildren(value =>
            {
                Assert.Equal("test", value);
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.2"));
            Assert.True(db.TryGetValueOrTypedChildren(value =>
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
            Assert.True(db.TryGetValueOrTypedChildren(value =>
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
            Assert.True(db.TryGetValueOrTypedChildren(value =>
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

            Assert.False(db.TryGetValueOrTypedChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.3"));
            Assert.False(db.TryGetValueOrTypedChildren(value =>
            {
                Assert.True(false, "Path contains value.");
            }, children =>
            {
                Assert.True(false, "Path contains another path.");
            }, "0", "3", "3.1", "3.1.1", "3.1.1.4"));
            Assert.False(db.TryGetValueOrTypedChildren(value =>
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

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrTypedChildren(delegate { }, delegate { }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrTypedChildren(delegate { }, delegate { }, null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrTypedChildren(delegate { }, delegate { }, new string[0]));

            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrTypedChildren(delegate { }, delegate { }, "path", null));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrTypedChildren(delegate { }, delegate { }, "path", ""));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrTypedChildren(delegate { }, delegate { }, new string?[] { "path", null }));
            Assert.Throws(typeof(StringNullOrEmptyException), () => db.TryGetValueOrTypedChildren(delegate { }, delegate { }, new string[] { "path", "" }));

            app.Dispose();
        }
    }
}
