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
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using ObservableHelpers.Utilities;
using ObservableHelpers;

namespace DatabaseTest.RealtimeModuleTest
{
    public static class Helpers
    {
        public static async Task<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>> AuthenticatedTestApp(string testName, string factName)
        {
            var generator = await RestfulFirebase.Test.Helpers.AuthenticatedAppGenerator();

            return new Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>(
                async subNode =>
                {
                    RestfulFirebaseApp app = await generator();

                    RealtimeWire wire;
                    subNode = subNode == null ? new string[0] : subNode;
                    if (subNode.Length == 0)
                    {
                        wire = app.Database
                            .Child("users")
                            .Child(app.Auth.Session.LocalId)
                            .Child(nameof(RealtimeModuleTest))
                            .Child(testName)
                            .Child(factName)
                            .AsRealtimeWire();
                    }
                    else
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (var subPath in subNode)
                        {
                            if (string.IsNullOrEmpty(subPath))
                            {
                                builder.Append("/");
                            }
                            else
                            {
                                builder.Append(subPath);
                                if (!subPath.EndsWith("/"))
                                {
                                    builder.Append("/");
                                }
                            }
                        }
                        string additionalPath = builder.ToString();
                        additionalPath = additionalPath.Substring(0, additionalPath.Length - 1);
                        wire = app.Database
                            .Child("users")
                            .Child(app.Auth.Session.LocalId)
                            .Child(nameof(RealtimeModuleTest))
                            .Child(testName)
                            .Child(factName)
                            .Child(additionalPath)
                            .AsRealtimeWire();
                    }
                    wire.Error += (s, e) =>
                    {
                        Task.Run(delegate
                        {
                            Assert.True(false, e.Exception.Message);
                        });
                    };
                    var dataChanges = new List<DataChangesEventArgs>();
                    wire.DataChanges += (s, e) =>
                    {
                        dataChanges.Add(e);
                    };

                    return (app, wire, dataChanges);
                });
        }

        public static async Task CleanTest(
            string testName,
            string factName,
            Func<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>, Task> test)
        {
            var appGenerator = await AuthenticatedTestApp(testName, factName);

            var app1 = await appGenerator(null);
            app1.wire.Start();
            app1.wire.SetNull();
            Assert.True(await app1.wire.WaitForSynced(true));
            app1.app.Dispose();

            await test(appGenerator);

            var app2 = await appGenerator(null);
            app2.wire.Start();
            app2.wire.SetNull();
            Assert.True(await app2.wire.WaitForSynced(true));
            app2.app.Dispose();
        }

        public static Task CleanTest(
            string testName,
            string factName,
            Action<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>> test)
        {
            return CleanTest(testName, factName, t => Task.Run(delegate { test(t); }));
        }
    }

    public class AppTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(AppTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                Assert.Equal(test1.app, test1.wire.App);

                test1.app.Dispose();
            });
        }
    }

    public class ChildTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(ChildTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                var child = test1.wire.Child("0", "1");

                Assert.True(test1.wire.SetValue("test1", "0", "1", "11"));
                Assert.True(test1.wire.SetValue("test2", "0", "1", "12"));

                await Task.Delay(1000);

                var data = test1.wire.GetRecursiveData();
                Assert.Collection(data,
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test1", i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(3, i.path.Length);
                        Assert.Equal("0", i.path[0]);
                        Assert.Equal("1", i.path[1]);
                        Assert.Equal("11", i.path[2]);
                    },
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test2", i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(3, i.path.Length);
                        Assert.Equal("0", i.path[0]);
                        Assert.Equal("1", i.path[1]);
                        Assert.Equal("12", i.path[2]);
                    });
                var childData = child.GetRecursiveData();
                Assert.Collection(childData,
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test1", i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("11", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test2", i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("12", i.path[0]);
                    });

                test1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(ChildTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(StringNullOrEmptyException), () => test1.wire.Child());
                Assert.Throws(typeof(StringNullOrEmptyException), () => test1.wire.Child(null));
                Assert.Throws(typeof(StringNullOrEmptyException), () => test1.wire.Child(new string[0]));

                Assert.Throws(typeof(StringNullOrEmptyException), () => test1.wire.Child("path", null));
                Assert.Throws(typeof(StringNullOrEmptyException), () => test1.wire.Child("path", ""));
                Assert.Throws(typeof(StringNullOrEmptyException), () => test1.wire.Child(new string?[] { "path", null }));
                Assert.Throws(typeof(StringNullOrEmptyException), () => test1.wire.Child(new string[] { "path", "" }));

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.Child(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.Child(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.Child(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.Child(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.Child(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class CloneTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(CloneTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                var clone = test1.wire.Clone();
                var cloneDataChanges = new List<DataChangesEventArgs>();
                clone.DataChanges += (s, e) =>
                {
                    cloneDataChanges.Add(e);
                };

                Assert.True(test1.wire.SetValue("test1", "0", "1", "11"));
                Assert.True(test1.wire.SetValue("test2", "0", "1", "12"));

                await Task.Delay(1000);

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(3, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                        Assert.Equal("11", i.Path[2]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(3, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                        Assert.Equal("12", i.Path[2]);
                    });
                Assert.Collection(cloneDataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(3, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                        Assert.Equal("11", i.Path[2]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(3, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                        Assert.Equal("12", i.Path[2]);
                    });

                var data = test1.wire.GetRecursiveData();
                Assert.Collection(data,
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test1", i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(3, i.path.Length);
                        Assert.Equal("0", i.path[0]);
                        Assert.Equal("1", i.path[1]);
                        Assert.Equal("11", i.path[2]);
                    },
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test2", i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(3, i.path.Length);
                        Assert.Equal("0", i.path[0]);
                        Assert.Equal("1", i.path[1]);
                        Assert.Equal("12", i.path[2]);
                    });
                var cloneData = clone.GetRecursiveData();
                Assert.Collection(cloneData,
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test1", i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(3, i.path.Length);
                        Assert.Equal("0", i.path[0]);
                        Assert.Equal("1", i.path[1]);
                        Assert.Equal("11", i.path[2]);
                    },
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test2", i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(3, i.path.Length);
                        Assert.Equal("0", i.path[0]);
                        Assert.Equal("1", i.path[1]);
                        Assert.Equal("12", i.path[2]);
                    });

                test1.app.Dispose();
            });
        }
    }

    public class DataChangesTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(DataChangesTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;
                var dataChanges = new List<DataChangesEventArgs>();
                test1.wire.DataChanges += (s, e) =>
                {
                    dataChanges.Add(e);
                };

                Assert.True(test1.wire.SetValue("test1", "0", "1", "11"));
                Assert.True(test1.wire.SetValue("test2", "0", "1", "12"));

                await Task.Delay(1000);

                Assert.Collection(dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(3, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                        Assert.Equal("11", i.Path[2]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(3, i.Path.Length);
                        Assert.Equal("0", i.Path[0]);
                        Assert.Equal("1", i.Path[1]);
                        Assert.Equal("12", i.Path[2]);
                    });

                dataChanges.Clear();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await test1.wire.WaitForSynced(true));

                Assert.Equal(2, dataChanges.Count);

                test1.app.Dispose();
            });
        }
    }

    public class DisposeTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(DisposeTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                test1.wire.Dispose();

                Assert.False(test1.wire.SetValue("test1", "prop1"));
                Assert.False(test1.wire.SetValue("test2", "prop2"));
                Assert.False(test1.wire.SetValue("test3", "prop3"));

                await Task.Delay(1000);

                Assert.False(test1.wire.Started);
                Assert.Empty(test1.dataChanges);

                test1.app.Dispose();
            });
        }
    }

    public class ErrorTest
    {
        [Fact]
        public async void Normal()
        {
            var generator = await RestfulFirebase.Test.Helpers.AuthenticatedAppGenerator();
            var app1 = await generator();
            var wire1 = app1.Database
                .Child("unauthorized")
                .AsRealtimeWire();
            var wire1Errors = new List<WireExceptionEventArgs>();
            wire1.Error += (s, e) =>
            {
                wire1Errors.Add(e);
            };
            wire1.Start();
            wire1.SetValue("test");
            Assert.False(await wire1.WaitForSynced(true));
            Assert.True(wire1Errors.Count > 0);
            Assert.Equal(typeof(DatabaseUnauthorizedException), wire1Errors[0].Exception.GetType());

            var app2 = await generator();
            var wire2 = app2.Database
                .Child("users")
                .Child(app2.Auth.Session.LocalId)
                .Child(nameof(RealtimeModuleTest))
                .Child(nameof(ErrorTest))
                .Child(nameof(Normal))
                .AsRealtimeWire();
            var wire2Errors = new List<WireExceptionEventArgs>();
            wire2.Error += (s, e) =>
            {
                wire2Errors.Add(e);
            };
            app2.Config.OfflineMode = true;
            wire2.Start();
            wire2.SetValue("test");
            Assert.False(await wire2.WaitForSynced(true));
            await Task.Delay(1000);
            Assert.True(wire2Errors.Count > 0);
            Assert.Equal(typeof(OfflineModeException), wire2Errors[0].Exception.GetType());

            app1.Dispose();
            app2.Dispose();
        }
    }

    public class GetChildrenTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(GetChildrenTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                var children1 = test1.wire.GetChildren();
                Assert.Collection(children1,
                    i =>
                    {
                        Assert.Equal("prop1", i.key);
                        Assert.Equal(LocalDataType.Value, i.type);
                    },
                    i =>
                    {
                        Assert.Equal("prop2", i.key);
                        Assert.Equal(LocalDataType.Value, i.type);
                    },
                    i =>
                    {
                        Assert.Equal("prop3", i.key);
                        Assert.Equal(LocalDataType.Value, i.type);
                    },
                    i =>
                    {
                        Assert.Equal("prop4", i.key);
                        Assert.Equal(LocalDataType.Path, i.type);
                    });
                var children2 = test1.wire.GetChildren("prop4");
                Assert.Collection(children2,
                    i =>
                    {
                        Assert.Equal("subProp1", i.key);
                        Assert.Equal(LocalDataType.Value, i.type);
                    },
                    i =>
                    {
                        Assert.Equal("subProp2", i.key);
                        Assert.Equal(LocalDataType.Value, i.type);
                    },
                    i =>
                    {
                        Assert.Equal("subProp3", i.key);
                        Assert.Equal(LocalDataType.Value, i.type);
                    });

                test1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetChildrenTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetChildren(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetChildren(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetChildren(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetChildren(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetChildren(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class GetDataCountTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(GetDataCountTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.Equal((0, 0), test1.wire.GetDataCount());
                Assert.Equal((0, 0), test1.wire.GetDataCount("prop4"));

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                Assert.Equal((6, 0), test1.wire.GetDataCount());
                Assert.Equal((3, 0), test1.wire.GetDataCount("prop4"));

                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await test1.wire.WaitForSynced(true));

                Assert.Equal((6, 6), test1.wire.GetDataCount());
                Assert.Equal((3, 3), test1.wire.GetDataCount("prop4"));

                test1.app.Dispose();

                var test2 = await generator(null);
                test2.wire.Start();
                Assert.True(await test2.wire.WaitForSynced(true));

                Assert.Equal((6, 6), test2.wire.GetDataCount());
                Assert.Equal((3, 3), test2.wire.GetDataCount("prop4"));

                test2.app.Dispose();

                var test3 = await generator(new string[] { "prop4" });
                test3.wire.Start();
                Assert.True(await test3.wire.WaitForSynced(true));

                Assert.Equal((3, 3), test3.wire.GetDataCount());

                test3.app.Dispose();

                var test4 = await generator(new string[] { "prop1" });
                test4.wire.Start();
                Assert.True(await test4.wire.WaitForSynced(true));

                Assert.Equal((1, 1), test4.wire.GetDataCount());

                test4.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetDataCountTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetDataCount(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetDataCount(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetDataCount(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetDataCount(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetDataCount(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class GetDataTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(GetDataTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                Assert.Equal((null, "test1", "test1", LocalDataChangesType.Create), test1.wire.GetData("prop1"));
                Assert.Equal((null, "test2", "test2", LocalDataChangesType.Create), test1.wire.GetData("prop2"));
                Assert.Equal((null, "test3", "test3", LocalDataChangesType.Create), test1.wire.GetData("prop3"));
                Assert.Equal((null, "test4", "test4", LocalDataChangesType.Create), test1.wire.GetData("prop4", "subProp1"));
                Assert.Equal((null, "test5", "test5", LocalDataChangesType.Create), test1.wire.GetData("prop4", "subProp2"));
                Assert.Equal((null, "test6", "test6", LocalDataChangesType.Create), test1.wire.GetData("prop4", "subProp3"));

                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await test1.wire.WaitForSynced(true));

                Assert.Equal(("test1", null, "test1", LocalDataChangesType.Synced), test1.wire.GetData("prop1"));
                Assert.Equal(("test2", null, "test2", LocalDataChangesType.Synced), test1.wire.GetData("prop2"));
                Assert.Equal(("test3", null, "test3", LocalDataChangesType.Synced), test1.wire.GetData("prop3"));
                Assert.Equal(("test4", null, "test4", LocalDataChangesType.Synced), test1.wire.GetData("prop4", "subProp1"));
                Assert.Equal(("test5", null, "test5", LocalDataChangesType.Synced), test1.wire.GetData("prop4", "subProp2"));
                Assert.Equal(("test6", null, "test6", LocalDataChangesType.Synced), test1.wire.GetData("prop4", "subProp3"));

                test1.app.Dispose();

                var test2 = await generator(null);
                test2.wire.Start();
                Assert.True(await test2.wire.WaitForSynced(true));

                Assert.Equal(("test1", null, "test1", LocalDataChangesType.Synced), test2.wire.GetData("prop1"));
                Assert.Equal(("test2", null, "test2", LocalDataChangesType.Synced), test2.wire.GetData("prop2"));
                Assert.Equal(("test3", null, "test3", LocalDataChangesType.Synced), test2.wire.GetData("prop3"));
                Assert.Equal(("test4", null, "test4", LocalDataChangesType.Synced), test2.wire.GetData("prop4", "subProp1"));
                Assert.Equal(("test5", null, "test5", LocalDataChangesType.Synced), test2.wire.GetData("prop4", "subProp2"));
                Assert.Equal(("test6", null, "test6", LocalDataChangesType.Synced), test2.wire.GetData("prop4", "subProp3"));

                test2.app.Dispose();

                var test3 = await generator(new string[] { "prop1" });
                test3.wire.Start();
                Assert.True(await test3.wire.WaitForSynced(true));

                Assert.Equal(("test1", null, "test1", LocalDataChangesType.Synced), test3.wire.GetData());

                test3.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetDataTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetData(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetData(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetData(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetData(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetData(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class GetRecursiveChildrenTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(GetRecursiveChildrenTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                var children1 = test1.wire.GetRecursiveChildren();
                Assert.Collection(children1,
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("prop1", i[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("prop2", i[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("prop3", i[0]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Length);
                        Assert.Equal("prop4", i[0]);
                        Assert.Equal("subProp1", i[1]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Length);
                        Assert.Equal("prop4", i[0]);
                        Assert.Equal("subProp2", i[1]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Length);
                        Assert.Equal("prop4", i[0]);
                        Assert.Equal("subProp3", i[1]);
                    });
                var children2 = test1.wire.GetRecursiveChildren("prop4");
                Assert.Collection(children2,
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("subProp1", i[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("subProp2", i[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Length);
                        Assert.Equal("subProp3", i[0]);
                    });

                test1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetRecursiveChildrenTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetRecursiveChildren(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetRecursiveChildren(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetRecursiveChildren(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetRecursiveChildren(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetRecursiveChildren(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class GetRecursiveDataTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(GetRecursiveDataTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                var children1a = test1.wire.GetRecursiveData();
                Assert.Collection(children1a,
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test1", i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test2", i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test3", i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test4", i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test5", i.local);
                        Assert.Equal("test5", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test6", i.local);
                        Assert.Equal("test6", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp3", i.path[1]);
                    });
                var children1b = test1.wire.GetRecursiveData("prop4");
                Assert.Collection(children1b,
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test4", i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("subProp1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test5", i.local);
                        Assert.Equal("test5", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("subProp2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Null(i.sync);
                        Assert.Equal("test6", i.local);
                        Assert.Equal("test6", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("subProp3", i.path[0]);
                    });

                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await test1.wire.WaitForSynced(true));

                var children1c = test1.wire.GetRecursiveData();
                Assert.Collection(children1c,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test5", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test5", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test6", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test6", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp3", i.path[1]);
                    });
                var children1d = test1.wire.GetRecursiveData("prop4");
                Assert.Collection(children1d,
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("subProp1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test5", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test5", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("subProp2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test6", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test6", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("subProp3", i.path[0]);
                    });

                test1.app.Dispose();

                var test2 = await generator(null);
                test2.wire.Start();
                Assert.True(await test2.wire.WaitForSynced(true));

                var children2a = test2.wire.GetRecursiveData();
                Assert.Collection(children2a,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test5", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test5", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test6", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test6", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp3", i.path[1]);
                    });
                var children2b = test2.wire.GetRecursiveData("prop4");
                Assert.Collection(children2b,
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("subProp1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test5", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test5", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("subProp2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test6", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test6", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("subProp3", i.path[0]);
                    });

                test2.app.Dispose();

                var test3 = await generator(new string[] { "prop1" });
                test3.wire.Start();
                Assert.True(await test3.wire.WaitForSynced(true));

                var children3 = test3.wire.GetRecursiveData();
                Assert.Collection(children3,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Empty(i.path);
                    });

                test3.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetRecursiveDataTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetRecursiveData(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetRecursiveData(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetRecursiveData(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetRecursiveData(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetRecursiveData(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class GetValueTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(GetValueTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                Assert.Equal("test1", test1.wire.GetValue("prop1"));
                Assert.Equal("test2", test1.wire.GetValue("prop2"));
                Assert.Equal("test3", test1.wire.GetValue("prop3"));
                Assert.Equal("test4", test1.wire.GetValue("prop4", "subProp1"));
                Assert.Equal("test5", test1.wire.GetValue("prop4", "subProp2"));
                Assert.Equal("test6", test1.wire.GetValue("prop4", "subProp3"));

                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await test1.wire.WaitForSynced(true));

                Assert.Equal("test1", test1.wire.GetValue("prop1"));
                Assert.Equal("test2", test1.wire.GetValue("prop2"));
                Assert.Equal("test3", test1.wire.GetValue("prop3"));
                Assert.Equal("test4", test1.wire.GetValue("prop4", "subProp1"));
                Assert.Equal("test5", test1.wire.GetValue("prop4", "subProp2"));
                Assert.Equal("test6", test1.wire.GetValue("prop4", "subProp3"));

                test1.app.Dispose();

                var test2 = await generator(null);
                test2.wire.Start();
                Assert.True(await test2.wire.WaitForSynced(true));

                Assert.Equal("test1", test2.wire.GetValue("prop1"));
                Assert.Equal("test2", test2.wire.GetValue("prop2"));
                Assert.Equal("test3", test2.wire.GetValue("prop3"));
                Assert.Equal("test4", test2.wire.GetValue("prop4", "subProp1"));
                Assert.Equal("test5", test2.wire.GetValue("prop4", "subProp2"));
                Assert.Equal("test6", test2.wire.GetValue("prop4", "subProp3"));

                test2.app.Dispose();

                var test3 = await generator(new string[] { "prop1" });
                test3.wire.Start();
                Assert.True(await test3.wire.WaitForSynced(true));

                Assert.Equal("test1", test3.wire.GetValue());

                test3.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetValueTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetValue(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetValue(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetValue(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetValue(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.GetValue(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class HasChildrenTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(HasChildrenTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.False(test1.wire.HasChildren());
                Assert.False(test1.wire.HasChildren("prop4"));

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                Assert.True(test1.wire.HasChildren());
                Assert.True(test1.wire.HasChildren("prop4"));

                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await test1.wire.WaitForSynced(true));

                Assert.True(test1.wire.HasChildren());
                Assert.True(test1.wire.HasChildren("prop4"));

                test1.app.Dispose();

                var test2 = await generator(null);
                test2.wire.Start();
                Assert.True(await test2.wire.WaitForSynced(true));

                Assert.True(test2.wire.HasChildren());
                Assert.True(test2.wire.HasChildren("prop4"));

                test2.app.Dispose();

                var test3 = await generator(new string[] { "prop4" });
                test3.wire.Start();
                Assert.True(await test3.wire.WaitForSynced(true));

                Assert.True(test3.wire.HasChildren());

                test3.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(HasChildrenTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.HasChildren(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.HasChildren(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.HasChildren(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.HasChildren(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.HasChildren(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class HasFirstStreamTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(HasFirstStreamTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.False(test1.wire.HasFirstStream);

                Assert.True(await test1.wire.WaitForSynced(true));

                Assert.True(test1.wire.HasFirstStream);

                test1.app.Dispose();
            });
        }
    }

    public class IsLocallyAvailableTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(IsLocallyAvailableTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.False(test1.wire.IsLocallyAvailable());
                Assert.False(test1.wire.IsLocallyAvailable("prop1"));
                Assert.False(test1.wire.IsLocallyAvailable("prop4"));

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                Assert.True(test1.wire.IsLocallyAvailable());
                Assert.True(test1.wire.IsLocallyAvailable("prop1"));
                Assert.True(test1.wire.IsLocallyAvailable("prop4"));

                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await test1.wire.WaitForSynced(true));

                Assert.True(test1.wire.IsLocallyAvailable());
                Assert.True(test1.wire.IsLocallyAvailable("prop1"));
                Assert.True(test1.wire.IsLocallyAvailable("prop4"));

                test1.app.Dispose();

                var test2 = await generator(null);
                test2.wire.Start();

                Assert.False(test2.wire.IsLocallyAvailable());
                Assert.False(test2.wire.IsLocallyAvailable("prop1"));
                Assert.False(test2.wire.IsLocallyAvailable("prop4"));

                Assert.True(await test2.wire.WaitForSynced(true));

                Assert.True(test2.wire.IsLocallyAvailable());
                Assert.True(test2.wire.IsLocallyAvailable("prop1"));
                Assert.True(test2.wire.IsLocallyAvailable("prop4"));

                test2.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(IsLocallyAvailableTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsLocallyAvailable(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsLocallyAvailable(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsLocallyAvailable(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsLocallyAvailable(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsLocallyAvailable(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class IsNullTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(IsNullTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(test1.wire.IsNull());
                Assert.True(test1.wire.IsNull("prop1"));
                Assert.True(test1.wire.IsNull("prop2"));
                Assert.True(test1.wire.IsNull("prop3"));
                Assert.True(test1.wire.IsNull("prop4"));
                Assert.True(test1.wire.IsNull("prop4", "subProp1"));
                Assert.True(test1.wire.IsNull("prop4", "subProp2"));
                Assert.True(test1.wire.IsNull("prop4", "subProp3"));

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                Assert.False(test1.wire.IsNull());
                Assert.False(test1.wire.IsNull("prop1"));
                Assert.False(test1.wire.IsNull("prop2"));
                Assert.False(test1.wire.IsNull("prop3"));
                Assert.False(test1.wire.IsNull("prop4"));
                Assert.False(test1.wire.IsNull("prop4", "subProp1"));
                Assert.False(test1.wire.IsNull("prop4", "subProp2"));
                Assert.False(test1.wire.IsNull("prop4", "subProp3"));

                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await test1.wire.WaitForSynced(true));

                Assert.False(test1.wire.IsNull());
                Assert.False(test1.wire.IsNull("prop1"));
                Assert.False(test1.wire.IsNull("prop2"));
                Assert.False(test1.wire.IsNull("prop3"));
                Assert.False(test1.wire.IsNull("prop4"));
                Assert.False(test1.wire.IsNull("prop4", "subProp1"));
                Assert.False(test1.wire.IsNull("prop4", "subProp2"));
                Assert.False(test1.wire.IsNull("prop4", "subProp3"));

                test1.app.Dispose();

                var test2 = await generator(null);
                test2.wire.Start();

                Assert.True(test2.wire.IsNull());
                Assert.True(test2.wire.IsNull("prop1"));
                Assert.True(test2.wire.IsNull("prop2"));
                Assert.True(test2.wire.IsNull("prop3"));
                Assert.True(test2.wire.IsNull("prop4"));
                Assert.True(test2.wire.IsNull("prop4", "subProp1"));
                Assert.True(test2.wire.IsNull("prop4", "subProp2"));
                Assert.True(test2.wire.IsNull("prop4", "subProp3"));

                Assert.True(await test2.wire.WaitForSynced(true));

                Assert.False(test2.wire.IsNull());
                Assert.False(test2.wire.IsNull("prop1"));
                Assert.False(test2.wire.IsNull("prop2"));
                Assert.False(test2.wire.IsNull("prop3"));
                Assert.False(test2.wire.IsNull("prop4"));
                Assert.False(test2.wire.IsNull("prop4", "subProp1"));
                Assert.False(test2.wire.IsNull("prop4", "subProp2"));
                Assert.False(test2.wire.IsNull("prop4", "subProp3"));

                test2.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(IsNullTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsNull(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsNull(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsNull(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsNull(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsNull(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class IsSyncedTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(IsSyncedTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                Assert.False(test1.wire.IsSynced());

                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await test1.wire.WaitForSynced(true));

                Assert.True(test1.wire.IsSynced());

                test1.app.Dispose();

                var test2 = await generator(null);
                test2.wire.Start();

                Assert.False(test2.wire.IsSynced());

                Assert.True(await test2.wire.WaitForSynced(true));

                Assert.True(test2.wire.IsSynced());

                test2.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(IsSyncedTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsSynced(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsSynced(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsSynced(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsSynced(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.IsSynced(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class LocalDatabaseTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(LocalDatabaseTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.Equal(test1.app.Config.LocalDatabase, test1.wire.LocalDatabase);

                var localDatabase = new SampleLocalDatabase();

                var wire2 = test1.app.Database
                    .Child("users")
                    .Child(test1.app.Auth.Session.LocalId)
                    .Child(nameof(RealtimeModuleTest))
                    .Child(nameof(LocalDatabaseTest))
                    .Child(nameof(Normal))
                    .AsRealtimeWire(localDatabase);
                wire2.Start();

                Assert.Equal(localDatabase, wire2.LocalDatabase);

                wire2.SetValue("test1", "prop1");

                await Task.Delay(1000);

                var wire3 = test1.app.Database
                    .Child("users")
                    .Child(test1.app.Auth.Session.LocalId)
                    .Child(nameof(RealtimeModuleTest))
                    .Child(nameof(LocalDatabaseTest))
                    .Child(nameof(Normal))
                    .AsRealtimeWire(localDatabase);
                wire3.Start();

                Assert.Null(test1.wire.GetValue("prop1"));
                Assert.Equal("test1", wire2.GetValue("prop1"));
                Assert.Equal("test1", wire3.GetValue("prop1"));

                test1.app.Dispose();
            });
        }
    }

    public class MaxConcurrentWriteTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(MaxConcurrentWriteTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                Assert.False(test1.wire.IsSynced());

                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await test1.wire.WaitForSynced(true));

                Assert.True(test1.wire.IsSynced());

                test1.app.Dispose();

                var test2 = await generator(null);
                test2.wire.Start();

                Assert.False(test2.wire.IsSynced());

                Assert.True(await test2.wire.WaitForSynced(true));

                Assert.True(test2.wire.IsSynced());

                test2.app.Dispose();
            });
        }
    }

    public class ParentTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(ParentTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                test1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                var child1 = test1.wire.Child("child1");
                var child1DataChanges = new List<DataChangesEventArgs>();
                child1.DataChanges += (s, e) =>
                {
                    child1DataChanges.Add(e);
                };
                child1.Error += (s, e) =>
                {
                    Assert.True(false, e.Exception.Message);
                };

                var child2 = child1.Child("child2");
                var child2DataChanges = new List<DataChangesEventArgs>();
                child2.DataChanges += (s, e) =>
                {
                    child2DataChanges.Add(e);
                };
                child2.Error += (s, e) =>
                {
                    Assert.True(false, e.Exception.Message);
                };

                Assert.Null(test1.wire.Parent);
                Assert.Equal(test1.wire, child1.Parent);
                Assert.Equal(child1, child2.Parent);

                Assert.Empty(test1.dataChanges);
                Assert.Empty(child1DataChanges);
                Assert.Empty(child2DataChanges);

                Assert.True(child2.SetValue("test"));

                await Task.Delay(1000);

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("child1", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("child1", i.Path[0]);
                        Assert.Equal("child2", i.Path[1]);
                    });
                Assert.Collection(child1DataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("child2", i.Path[0]);
                    });
                Assert.Collection(child2DataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });

                test1.wire.SetNull();
                await Task.Delay(1000);
                test1.dataChanges.Clear();
                child1DataChanges.Clear();
                child2DataChanges.Clear();

                Assert.True(child1.SetValue("test"));

                await Task.Delay(1000);

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("child1", i.Path[0]);
                    });
                Assert.Collection(child1DataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                Assert.Empty(child2DataChanges);

                test1.app.Dispose();
            });
        }
    }

    public class PutModelAsyncTest
    {
        private class PutModelAsyncErrorTest : ObservableObject, IRealtimeModel
        {
            public RealtimeInstance? RealtimeInstance { get; }

            public bool HasAttachedRealtime { get; }

            event EventHandler<RealtimeInstanceEventArgs> IRealtimeModel.RealtimeAttached
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }

            event EventHandler<RealtimeInstanceEventArgs> IRealtimeModel.RealtimeDetached
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }

            event EventHandler<WireExceptionEventArgs> IRealtimeModel.WireError
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }
        }

        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(PutModelAsyncTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();


                test1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(PutModelAsyncTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.ThrowsAsync(typeof(DatabaseInvalidModel), () => test1.wire.PutModelAsync(new PutModelAsyncErrorTest(), new string[] { "path" }));

                await Assert.ThrowsAsync(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.PutModelAsync(new FirebaseProperty(), new string[] { "path", "1.1" }));
                await Assert.ThrowsAsync(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.PutModelAsync(new FirebaseProperty(), new string[] { "path", "1#1" }));
                await Assert.ThrowsAsync(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.PutModelAsync(new FirebaseProperty(), new string[] { "path", "1$1" }));
                await Assert.ThrowsAsync(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.PutModelAsync(new FirebaseProperty(), new string[] { "path", "1[1" }));
                await Assert.ThrowsAsync(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.PutModelAsync(new FirebaseProperty(), new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class PutModelTest
    {
        private class PutModelErrorTest : ObservableObject, IRealtimeModel
        {
            public RealtimeInstance? RealtimeInstance { get; }

            public bool HasAttachedRealtime { get; }

            event EventHandler<RealtimeInstanceEventArgs> IRealtimeModel.RealtimeAttached
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }

            event EventHandler<RealtimeInstanceEventArgs> IRealtimeModel.RealtimeDetached
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }

            event EventHandler<WireExceptionEventArgs> IRealtimeModel.WireError
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }
        }

        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(PutModelTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();


                test1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(PutModelTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseInvalidModel), () => test1.wire.PutModel(new PutModelErrorTest(), new string[] { "path" }));

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.PutModel(new FirebaseProperty(), new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.PutModel(new FirebaseProperty(), new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.PutModel(new FirebaseProperty(), new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.PutModel(new FirebaseProperty(), new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.PutModel(new FirebaseProperty(), new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class QueryTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(QueryTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                var test2 = await generator(null);
                var test1WireClone = test1.wire.Clone();
                var test2WireClone = test2.wire.Clone();

                Assert.Equal(test1.wire.Query.GetAbsoluteUrl(), test2.wire.Query.GetAbsoluteUrl());
                Assert.Equal(test1WireClone.Query.GetAbsoluteUrl(), test2WireClone.Query.GetAbsoluteUrl());
                Assert.Equal(test1.wire.Query.GetAbsoluteUrl(), test2WireClone.Query.GetAbsoluteUrl());
                Assert.Equal(test1WireClone.Query.GetAbsoluteUrl(), test2.wire.Query.GetAbsoluteUrl());

                test1.app.Dispose();
                test2.app.Dispose();
            });
        }
    }

    public class RootTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(RootTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                var test1WireClone = test1.wire.Clone();
                var test1WireChild1 = test1WireClone.Child("child");
                var test1WireChild2 = test1WireChild1.Child("child");

                Assert.Equal(null, test1.wire.Root);
                Assert.Equal(null, test1WireClone.Root);
                Assert.Equal(test1WireChild1.Root, test1WireClone);
                Assert.Equal(test1WireChild2.Root, test1WireClone);
                Assert.Equal(test1WireChild2.Root, test1WireChild1.Root);

                test1.app.Dispose();
            });
        }
    }

    public class SetNullTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(SetNullTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                Assert.True(await test1.wire.WaitForSynced(true));
                test1.dataChanges.Clear();

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.True(test1.wire.SetValue("test3", "prop3"));
                Assert.True(test1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(test1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(test1.wire.SetValue("test6", "prop4", "subProp3"));

                Assert.True(await test1.wire.WaitForSynced(true));
                test1.dataChanges.Clear();

                Assert.True(test1.wire.SetNull("prop1"));
                Assert.False(test1.wire.SetNull("prop1"));

                var phase1 = test1.wire.GetRecursiveData();
                Assert.Collection(phase1,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal(null, i.value);
                        Assert.Equal(LocalDataChangesType.Delete, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test5", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test5", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test6", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test6", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp3", i.path[1]);
                    });

                Assert.True(await test1.wire.WaitForSynced(true));

                var phase2 = test1.wire.GetRecursiveData();
                Assert.Collection(phase2,
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test5", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test5", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test6", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test6", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp3", i.path[1]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                test1.dataChanges.Clear();

                Assert.True(test1.wire.SetNull("prop4"));
                Assert.False(test1.wire.SetNull("prop4"));

                var phase3 = test1.wire.GetRecursiveData();
                Assert.Collection(phase3,
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(null, i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal(null, i.value);
                        Assert.Equal(LocalDataChangesType.Delete, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                    });

                Assert.True(await test1.wire.WaitForSynced(true));

                var phase4 = test1.wire.GetRecursiveData();
                Assert.Collection(phase4,
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                        Assert.Equal("subProp1", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                        Assert.Equal("subProp2", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                        Assert.Equal("subProp3", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                test1.dataChanges.Clear();

                Assert.True(test1.wire.SetNull());
                Assert.False(test1.wire.SetNull());

                var phase5 = test1.wire.GetRecursiveData();
                Assert.Collection(phase5,
                    i =>
                    {
                        Assert.Equal(null, i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal(null, i.value);
                        Assert.Equal(LocalDataChangesType.Delete, i.changesType);
                        Assert.Equal(0, i.path.Length);
                    });

                Assert.True(await test1.wire.WaitForSynced(true));

                var phase6 = test1.wire.GetRecursiveData();
                Assert.Empty(phase6);

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                test1.dataChanges.Clear();

                test1.app.Dispose();
            });
        }

        [Fact]
        public async void CrossSync()
        {
            await Helpers.CleanTest(nameof(SetNullTest), nameof(CrossSync), async generator =>
            {
                var origin1 = await generator(null);
                origin1.wire.Start();

                Assert.True(origin1.wire.SetValue("test1", "prop1"));
                Assert.True(origin1.wire.SetValue("test2", "prop2"));
                Assert.True(origin1.wire.SetValue("test3", "prop3"));
                Assert.True(origin1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(origin1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(origin1.wire.SetValue("test6", "prop4", "subProp3"));

                Assert.True(await origin1.wire.WaitForSynced(true));

                var test1 = await generator(null);
                test1.wire.Start();
                Assert.True(await test1.wire.WaitForSynced(true));
                test1.dataChanges.Clear();

                Assert.True(origin1.wire.SetNull("prop1"));
                Assert.False(origin1.wire.SetNull("prop1"));

                Assert.True(await origin1.wire.WaitForSynced(true));
                await Task.Delay(5000);

                var phase1 = test1.wire.GetRecursiveData();
                Assert.Collection(phase1,
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test5", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test5", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test6", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test6", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp3", i.path[1]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                test1.dataChanges.Clear();

                test1.wire.Stop();

                Assert.True(origin1.wire.SetNull("prop4"));
                Assert.False(origin1.wire.SetNull("prop4"));

                Assert.True(await origin1.wire.WaitForSynced(true));

                Assert.True(test1.wire.SetValue("test4Mod", "prop4", "subProp1"));
                Assert.False(test1.wire.SetValue("test4Mod", "prop4", "subProp1"));

                var phase2 = test1.wire.GetRecursiveData();
                Assert.Collection(phase2,
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal("test4Mod", i.local);
                        Assert.Equal("test4Mod", i.value);
                        Assert.Equal(LocalDataChangesType.Update, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test5", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test5", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test6", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test6", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop4", i.path[0]);
                        Assert.Equal("subProp3", i.path[1]);
                    });

                test1.wire.Start();
                Assert.True(await test1.wire.WaitForSynced(true));

                var phase3 = test1.wire.GetRecursiveData();
                Assert.Collection(phase3,
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                        Assert.Equal("subProp1", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                        Assert.Equal("subProp1", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                        Assert.Equal("subProp2", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                        Assert.Equal("subProp3", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop4", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                test1.dataChanges.Clear();

                test1.wire.Stop();

                Assert.True(origin1.wire.SetValue("test2Mod", "prop2"));
                Assert.False(origin1.wire.SetValue("test2Mod", "prop2"));

                Assert.True(await origin1.wire.WaitForSynced(true));

                Assert.True(test1.wire.SetNull("prop2"));
                Assert.False(test1.wire.SetNull("prop2"));

                var phase5 = test1.wire.GetRecursiveData();
                Assert.Collection(phase5,
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal(null, i.value);
                        Assert.Equal(LocalDataChangesType.Delete, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    });

                test1.wire.Start();
                Assert.True(await test1.wire.WaitForSynced(true));

                var phase6 = test1.wire.GetRecursiveData();
                Assert.Collection(phase6,
                    i =>
                    {
                        Assert.Equal("test2Mod", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2Mod", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    });
                test1.dataChanges.Clear();

                Assert.True(origin1.wire.SetNull());
                Assert.False(origin1.wire.SetNull());

                Assert.True(await origin1.wire.WaitForSynced(true));
                await Task.Delay(5000);

                var phase7 = test1.wire.GetRecursiveData();
                Assert.Empty(phase7);

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                test1.dataChanges.Clear();

                origin1.app.Dispose();
                test1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(SetNullTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SetNull(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SetNull(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SetNull(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SetNull(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SetNull(new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class SetValueTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(SetValueTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();
                Assert.True(await test1.wire.WaitForSynced(true));
                test1.dataChanges.Clear();

                Assert.True(test1.wire.SetValue("test1", "prop1"));
                Assert.False(test1.wire.SetValue("test1", "prop1"));

                var phase1 = test1.wire.GetRecursiveData();
                Assert.Collection(phase1,
                    i =>
                    {
                        Assert.Equal(null, i.sync);
                        Assert.Equal("test1", i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    });

                Assert.True(await test1.wire.WaitForSynced(true));

                var phase2 = test1.wire.GetRecursiveData();
                Assert.Collection(phase2,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    });
                test1.dataChanges.Clear();

                Assert.True(test1.wire.SetValue("test2", "prop2"));
                Assert.False(test1.wire.SetValue("test2", "prop2"));

                var phase3 = test1.wire.GetRecursiveData();
                Assert.Collection(phase3,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(null, i.sync);
                        Assert.Equal("test2", i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    });

                Assert.True(await test1.wire.WaitForSynced(true));

                var phase4 = test1.wire.GetRecursiveData();
                Assert.Collection(phase4,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    });
                test1.dataChanges.Clear();

                Assert.True(test1.wire.SetValue("test3", "prop3", "subProp1"));
                Assert.False(test1.wire.SetValue("test3", "prop3", "subProp1"));

                var phase5 = test1.wire.GetRecursiveData();
                Assert.Collection(phase5,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(null, i.sync);
                        Assert.Equal("test3", i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    });

                Assert.True(await test1.wire.WaitForSynced(true));

                var phase6 = test1.wire.GetRecursiveData();
                Assert.Collection(phase6,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                        Assert.Equal("subProp1", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                        Assert.Equal("subProp1", i.Path[1]);
                    });
                test1.dataChanges.Clear();

                Assert.True(test1.wire.SetValue("test4", "prop3", "subProp2"));
                Assert.False(test1.wire.SetValue("test4", "prop3", "subProp2"));

                var phase7 = test1.wire.GetRecursiveData();
                Assert.Collection(phase7,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(null, i.sync);
                        Assert.Equal("test4", i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Create, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    });

                Assert.True(await test1.wire.WaitForSynced(true));

                var phase8 = test1.wire.GetRecursiveData();
                Assert.Collection(phase8,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                        Assert.Equal("subProp2", i.Path[1]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                        Assert.Equal("subProp2", i.Path[1]);
                    });
                test1.dataChanges.Clear();

                Assert.True(test1.wire.SetValue(null, "prop1"));
                Assert.False(test1.wire.SetValue(null, "prop1"));

                var phase9 = test1.wire.GetRecursiveData();
                Assert.Collection(phase9,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal(null, i.value);
                        Assert.Equal(LocalDataChangesType.Delete, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    });

                Assert.True(await test1.wire.WaitForSynced(true));

                var phase10 = test1.wire.GetRecursiveData();
                Assert.Collection(phase10,
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                test1.dataChanges.Clear();

                Assert.True(test1.wire.SetValue("test2Mod", "prop2"));
                Assert.False(test1.wire.SetValue("test2Mod", "prop2"));

                var phase11 = test1.wire.GetRecursiveData();
                Assert.Collection(phase11,
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal("test2Mod", i.local);
                        Assert.Equal("test2Mod", i.value);
                        Assert.Equal(LocalDataChangesType.Update, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    });

                Assert.True(await test1.wire.WaitForSynced(true));

                var phase12 = test1.wire.GetRecursiveData();
                Assert.Collection(phase12,
                    i =>
                    {
                        Assert.Equal("test2Mod", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2Mod", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    });
                test1.dataChanges.Clear();

                test1.app.Dispose();
            });
        }

        [Fact]
        public async void CrossSync()
        {
            await Helpers.CleanTest(nameof(SetValueTest), nameof(CrossSync), async generator =>
            {
                var origin1 = await generator(null);
                origin1.wire.Start();
                Assert.True(await origin1.wire.WaitForSynced(true));
                origin1.dataChanges.Clear();

                var test1 = await generator(null);
                test1.wire.Start();
                Assert.True(await test1.wire.WaitForSynced(true));
                test1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue("test1", "prop1"));
                Assert.False(origin1.wire.SetValue("test1", "prop1"));

                Assert.True(await origin1.wire.WaitForSynced(true));
                await Task.Delay(5000);

                var phase1 = test1.wire.GetRecursiveData();
                Assert.Collection(phase1,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    });
                test1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue("test2", "prop2"));
                Assert.False(origin1.wire.SetValue("test2", "prop2"));

                Assert.True(await origin1.wire.WaitForSynced(true));
                await Task.Delay(5000);

                var phase2 = test1.wire.GetRecursiveData();
                Assert.Collection(phase2,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    });
                test1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue("test3", "prop3", "subProp1"));
                Assert.False(origin1.wire.SetValue("test3", "prop3", "subProp1"));

                Assert.True(await origin1.wire.WaitForSynced(true));
                await Task.Delay(5000);

                var phase3 = test1.wire.GetRecursiveData();
                Assert.Collection(phase3,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                        Assert.Equal("subProp1", i.Path[1]);
                    });
                test1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue("test4", "prop3", "subProp2"));
                Assert.False(origin1.wire.SetValue("test4", "prop3", "subProp2"));

                Assert.True(await origin1.wire.WaitForSynced(true));
                await Task.Delay(5000);

                var phase4 = test1.wire.GetRecursiveData();
                Assert.Collection(phase4,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop1", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(2, i.Path.Length);
                        Assert.Equal("prop3", i.Path[0]);
                        Assert.Equal("subProp2", i.Path[1]);
                    });
                test1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue(null, "prop1"));
                Assert.False(origin1.wire.SetValue(null, "prop1"));

                Assert.True(await origin1.wire.WaitForSynced(true));
                await Task.Delay(5000);

                var phase5 = test1.wire.GetRecursiveData();
                Assert.Collection(phase5,
                    i =>
                    {
                        Assert.Equal("test2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                test1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue("test2Mod", "prop2"));
                Assert.False(origin1.wire.SetValue("test2Mod", "prop2"));

                Assert.True(await origin1.wire.WaitForSynced(true));
                await Task.Delay(5000);

                var phase6 = test1.wire.GetRecursiveData();
                Assert.Collection(phase6,
                    i =>
                    {
                        Assert.Equal("test2Mod", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2Mod", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    });
                test1.dataChanges.Clear();

                test1.wire.Stop();

                Assert.True(origin1.wire.SetValue("test2Mod2", "prop2"));
                Assert.False(origin1.wire.SetValue("test2Mod2", "prop2"));

                Assert.True(await origin1.wire.WaitForSynced(true));

                Assert.True(test1.wire.SetValue("conflict", "prop2"));
                Assert.False(test1.wire.SetValue("conflict", "prop2"));

                var phase7 = test1.wire.GetRecursiveData();
                Assert.Collection(phase7,
                    i =>
                    {
                        Assert.Equal("test2Mod", i.sync);
                        Assert.Equal("conflict", i.local);
                        Assert.Equal("conflict", i.value);
                        Assert.Equal(LocalDataChangesType.Update, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    });

                test1.wire.Start();
                Assert.True(await test1.wire.WaitForSynced(true));

                var phase8 = test1.wire.GetRecursiveData();
                Assert.Collection(phase8,
                    i =>
                    {
                        Assert.Equal("test2Mod2", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test2Mod2", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(1, i.path.Length);
                        Assert.Equal("prop2", i.path[0]);
                    },
                    i =>
                    {
                        Assert.Equal("test3", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test3", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp1", i.path[1]);
                    },
                    i =>
                    {
                        Assert.Equal("test4", i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal("test4", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Equal(2, i.path.Length);
                        Assert.Equal("prop3", i.path[0]);
                        Assert.Equal("subProp2", i.path[1]);
                    });

                Assert.Collection(test1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    });
                test1.dataChanges.Clear();

                origin1.app.Dispose();
                test1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(SetValueTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SetValue("test", new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SetValue("test", new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SetValue("test", new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SetValue("test", new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SetValue("test", new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class StartTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(StartTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);

                Assert.False(test1.wire.Started);

                test1.wire.Start();

                Assert.True(test1.wire.Started);

                test1.app.Dispose();
            });
        }
    }

    public class StartedTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(StartedTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);

                Assert.False(test1.wire.Started);

                test1.wire.Start();

                Assert.True(test1.wire.Started);

                test1.wire.Stop();

                Assert.False(test1.wire.Started);

                test1.wire.Start();

                Assert.True(test1.wire.Started);

                test1.app.Dispose();

                Assert.False(test1.wire.Started);
            });
        }
    }

    public class StopTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(StopTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.True(test1.wire.Started);

                test1.wire.Stop();

                Assert.False(test1.wire.Started);

                test1.app.Dispose();
            });
        }
    }

    public class SubModelAsyncTest
    {
        private class SubModelAsyncErrorTest : ObservableObject, IRealtimeModel
        {
            public RealtimeInstance? RealtimeInstance { get; }

            public bool HasAttachedRealtime { get; }

            event EventHandler<RealtimeInstanceEventArgs> IRealtimeModel.RealtimeAttached
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }

            event EventHandler<RealtimeInstanceEventArgs> IRealtimeModel.RealtimeDetached
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }

            event EventHandler<WireExceptionEventArgs> IRealtimeModel.WireError
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }
        }

        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(SubModelAsyncTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();


                test1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(SubModelAsyncTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.ThrowsAsync(typeof(DatabaseInvalidModel), () => test1.wire.SubModelAsync(new SubModelAsyncErrorTest(), new string[] { "path" }));

                await Assert.ThrowsAsync(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SubModelAsync(new FirebaseProperty(), new string[] { "path", "1.1" }));
                await Assert.ThrowsAsync(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SubModelAsync(new FirebaseProperty(), new string[] { "path", "1#1" }));
                await Assert.ThrowsAsync(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SubModelAsync(new FirebaseProperty(), new string[] { "path", "1$1" }));
                await Assert.ThrowsAsync(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SubModelAsync(new FirebaseProperty(), new string[] { "path", "1[1" }));
                await Assert.ThrowsAsync(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SubModelAsync(new FirebaseProperty(), new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class SubModelTest
    {
        private class SubModelErrorTest : ObservableObject, IRealtimeModel
        {
            public RealtimeInstance? RealtimeInstance { get; }

            public bool HasAttachedRealtime { get; }

            event EventHandler<RealtimeInstanceEventArgs> IRealtimeModel.RealtimeAttached
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }

            event EventHandler<RealtimeInstanceEventArgs> IRealtimeModel.RealtimeDetached
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }

            event EventHandler<WireExceptionEventArgs> IRealtimeModel.WireError
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }
        }

        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(SubModelTest), nameof(Normal), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();


                test1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(SubModelTest), nameof(Throws), async generator =>
            {
                var test1 = await generator(null);
                test1.wire.Start();

                Assert.Throws(typeof(DatabaseInvalidModel), () => test1.wire.SubModel(new SubModelErrorTest(), new string[] { "path" }));

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SubModel(new FirebaseProperty(), new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SubModel(new FirebaseProperty(), new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SubModel(new FirebaseProperty(), new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SubModel(new FirebaseProperty(), new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => test1.wire.SubModel(new FirebaseProperty(), new string[] { "path", "1]1" }));

                test1.app.Dispose();
            });
        }
    }

    public class WaitForSynced
    {
        [Fact]
        public async void Normal()
        {
            var generator = await RestfulFirebase.Test.Helpers.AuthenticatedAppGenerator();
            var app1 = await generator();
            var wire1 = app1.Database
                .Child("users")
                .Child(app1.Auth.Session.LocalId)
                .Child(nameof(RealtimeModuleTest))
                .Child(nameof(WaitForSynced))
                .Child(nameof(Normal))
                .AsRealtimeWire();
            var wire1Errors = new List<WireExceptionEventArgs>();
            wire1.Error += (s, e) =>
            {
                wire1Errors.Add(e);
            };

            wire1.Start();
            Assert.True(await wire1.WaitForSynced(true));

            app1.Dispose();

            var app2 = await generator();
            var wire2 = app2.Database
                .Child("unauthorized")
                .AsRealtimeWire();
            var wire2Errors = new List<WireExceptionEventArgs>();
            wire2.Error += (s, e) =>
            {
                wire2Errors.Add(e);
            };

            wire2.Start();
            Assert.False(await wire2.WaitForSynced(true));
        }
    }
}
