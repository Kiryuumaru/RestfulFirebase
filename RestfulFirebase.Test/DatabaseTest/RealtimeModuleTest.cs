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
        public static Task<(Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>> generator, Action dispose)> AuthenticatedTestApp(string testName, string factName)
        {
            return RestfulFirebase.Test.Helpers.AuthenticatedTestApp(nameof(RealtimeModuleTest), testName, factName);
        }

        public static Task CleanTest(
            string testName,
            string factName,
            Func<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>, Task> test)
        {
            return RestfulFirebase.Test.Helpers.CleanTest(nameof(RealtimeModuleTest), testName, factName, test);
        }

        public static Task CleanTest(
            string testName,
            string factName,
            Action<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>> test)
        {
            return RestfulFirebase.Test.Helpers.CleanTest(nameof(RealtimeModuleTest), testName, factName, test);
        }
    }

    public class AppTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(AppTest), nameof(Normal), async generator =>
            {
                var appInstance1 = await generator(null);
                Assert.Equal(appInstance1.app, appInstance1.wire.App);

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                var child = appInstance1.wire.Child("0", "1");

                Assert.True(appInstance1.wire.SetValue("test1", "0", "1", "11"));
                Assert.True(appInstance1.wire.SetValue("test2", "0", "1", "12"));

                await Task.Delay(1000);

                var data = appInstance1.wire.GetRecursiveData();
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

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(ChildTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(StringNullOrEmptyException), () => appInstance1.wire.Child());
                Assert.Throws(typeof(StringNullOrEmptyException), () => appInstance1.wire.Child(null));
                Assert.Throws(typeof(StringNullOrEmptyException), () => appInstance1.wire.Child(new string[0]));

                Assert.Throws(typeof(StringNullOrEmptyException), () => appInstance1.wire.Child("path", null));
                Assert.Throws(typeof(StringNullOrEmptyException), () => appInstance1.wire.Child("path", ""));
                Assert.Throws(typeof(StringNullOrEmptyException), () => appInstance1.wire.Child(new string?[] { "path", null }));
                Assert.Throws(typeof(StringNullOrEmptyException), () => appInstance1.wire.Child(new string[] { "path", "" }));

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.Child(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.Child(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.Child(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.Child(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.Child(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                var clone = appInstance1.wire.Clone();
                var cloneDataChanges = new List<DataChangesEventArgs>();
                clone.ImmediateDataChanges += (s, e) =>
                {
                    cloneDataChanges.Add(e);
                };

                Assert.True(appInstance1.wire.SetValue("test1", "0", "1", "11"));
                Assert.True(appInstance1.wire.SetValue("test2", "0", "1", "12"));

                await Task.Delay(1000);

                Assert.Collection(appInstance1.dataChanges,
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

                var data = appInstance1.wire.GetRecursiveData();
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

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;
                var dataChanges = new List<DataChangesEventArgs>();
                appInstance1.wire.ImmediateDataChanges += (s, e) =>
                {
                    dataChanges.Add(e);
                };

                Assert.True(appInstance1.wire.SetValue("test1", "0", "1", "11"));
                Assert.True(appInstance1.wire.SetValue("test2", "0", "1", "12"));

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
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                Assert.Equal(2, dataChanges.Count);

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                appInstance1.wire.Dispose();

                Assert.False(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.False(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.False(appInstance1.wire.SetValue("test3", "prop3"));

                await Task.Delay(1000);

                Assert.False(appInstance1.wire.Started);
                Assert.Empty(appInstance1.dataChanges);

                appInstance1.app.Dispose();
            });
        }
    }

    public class ErrorTest
    {
        [Fact]
        public async void Normal()
        {
            var instance = await RestfulFirebase.Test.Helpers.AuthenticatedAppGenerator();
            var app1 = await instance.generator();
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
            Assert.False(await Task.Run(async delegate
            {
                for (int i = 0; i < 10; i++)
                {
                    if (await wire1.WaitForSynced(TimeSpan.FromMinutes(1)))
                    {
                        return true;
                    }
                    if (wire1Errors.Any(i => i.Exception.GetType() == typeof(DatabaseUnauthorizedException)))
                    {
                        return false;
                    }
                }
                return false;
            }));
            Assert.True(wire1Errors.Count > 0);
            Assert.Contains(wire1Errors, i => i.Exception.GetType() == typeof(DatabaseUnauthorizedException));

            var app2 = await instance.generator();
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
            Assert.False(await Task.Run(async delegate
            {
                for (int i = 0; i < 10; i++)
                {
                    if (await wire1.WaitForSynced(TimeSpan.FromMinutes(1)))
                    {
                        return true;
                    }
                    if (wire2Errors.Any(i => i.Exception.GetType() == typeof(OfflineModeException)))
                    {
                        return false;
                    }
                }
                return false;
            }));
            await Task.Delay(1000);
            Assert.True(wire2Errors.Count > 0);
            Assert.Contains(wire2Errors, i => i.Exception.GetType() == typeof(OfflineModeException));

            instance.dispose();
        }
    }

    public class GetChildrenTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(GetChildrenTest), nameof(Normal), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                var children1 = appInstance1.wire.GetChildren();
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
                var children2 = appInstance1.wire.GetChildren("prop4");
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

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetChildrenTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetChildren(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetChildren(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetChildren(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetChildren(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetChildren(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.Equal((0, 0), appInstance1.wire.GetDataCount());
                Assert.Equal((0, 0), appInstance1.wire.GetDataCount("prop4"));

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                Assert.Equal((6, 0), appInstance1.wire.GetDataCount());
                Assert.Equal((3, 0), appInstance1.wire.GetDataCount("prop4"));

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                Assert.Equal((6, 6), appInstance1.wire.GetDataCount());
                Assert.Equal((3, 3), appInstance1.wire.GetDataCount("prop4"));

                appInstance1.app.Dispose();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance2.wire));

                Assert.Equal((6, 6), appInstance2.wire.GetDataCount());
                Assert.Equal((3, 3), appInstance2.wire.GetDataCount("prop4"));

                appInstance2.app.Dispose();

                var appInstance3 = await generator(new string[] { "prop4" });
                appInstance3.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance3.wire));

                Assert.Equal((3, 3), appInstance3.wire.GetDataCount());

                appInstance3.app.Dispose();

                var appInstance4 = await generator(new string[] { "prop1" });
                appInstance4.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance4.wire));

                Assert.Equal((1, 1), appInstance4.wire.GetDataCount());

                appInstance4.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetDataCountTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetDataCount(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetDataCount(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetDataCount(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetDataCount(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetDataCount(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                Assert.Equal((null, "test1", "test1", LocalDataChangesType.Create), appInstance1.wire.GetData("prop1"));
                Assert.Equal((null, "test2", "test2", LocalDataChangesType.Create), appInstance1.wire.GetData("prop2"));
                Assert.Equal((null, "test3", "test3", LocalDataChangesType.Create), appInstance1.wire.GetData("prop3"));
                Assert.Equal((null, "test4", "test4", LocalDataChangesType.Create), appInstance1.wire.GetData("prop4", "subProp1"));
                Assert.Equal((null, "test5", "test5", LocalDataChangesType.Create), appInstance1.wire.GetData("prop4", "subProp2"));
                Assert.Equal((null, "test6", "test6", LocalDataChangesType.Create), appInstance1.wire.GetData("prop4", "subProp3"));

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                Assert.Equal(("test1", null, "test1", LocalDataChangesType.Synced), appInstance1.wire.GetData("prop1"));
                Assert.Equal(("test2", null, "test2", LocalDataChangesType.Synced), appInstance1.wire.GetData("prop2"));
                Assert.Equal(("test3", null, "test3", LocalDataChangesType.Synced), appInstance1.wire.GetData("prop3"));
                Assert.Equal(("test4", null, "test4", LocalDataChangesType.Synced), appInstance1.wire.GetData("prop4", "subProp1"));
                Assert.Equal(("test5", null, "test5", LocalDataChangesType.Synced), appInstance1.wire.GetData("prop4", "subProp2"));
                Assert.Equal(("test6", null, "test6", LocalDataChangesType.Synced), appInstance1.wire.GetData("prop4", "subProp3"));

                appInstance1.app.Dispose();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance2.wire));

                Assert.Equal(("test1", null, "test1", LocalDataChangesType.Synced), appInstance2.wire.GetData("prop1"));
                Assert.Equal(("test2", null, "test2", LocalDataChangesType.Synced), appInstance2.wire.GetData("prop2"));
                Assert.Equal(("test3", null, "test3", LocalDataChangesType.Synced), appInstance2.wire.GetData("prop3"));
                Assert.Equal(("test4", null, "test4", LocalDataChangesType.Synced), appInstance2.wire.GetData("prop4", "subProp1"));
                Assert.Equal(("test5", null, "test5", LocalDataChangesType.Synced), appInstance2.wire.GetData("prop4", "subProp2"));
                Assert.Equal(("test6", null, "test6", LocalDataChangesType.Synced), appInstance2.wire.GetData("prop4", "subProp3"));

                appInstance2.app.Dispose();

                var appInstance3 = await generator(new string[] { "prop1" });
                appInstance3.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance3.wire));

                Assert.Equal(("test1", null, "test1", LocalDataChangesType.Synced), appInstance3.wire.GetData());

                appInstance3.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetDataTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetData(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetData(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetData(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetData(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetData(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                var children1 = appInstance1.wire.GetRecursiveChildren();
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
                var children2 = appInstance1.wire.GetRecursiveChildren("prop4");
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

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetRecursiveChildrenTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetRecursiveChildren(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetRecursiveChildren(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetRecursiveChildren(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetRecursiveChildren(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetRecursiveChildren(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                var children1a = appInstance1.wire.GetRecursiveData();
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
                var children1b = appInstance1.wire.GetRecursiveData("prop4");
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

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                await Task.Delay(5000);

                var children1c = appInstance1.wire.GetRecursiveData();
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
                var children1d = appInstance1.wire.GetRecursiveData("prop4");
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

                appInstance1.app.Dispose();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance2.wire));

                await Task.Delay(5000);

                var children2a = appInstance2.wire.GetRecursiveData();
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
                var children2b = appInstance2.wire.GetRecursiveData("prop4");
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

                appInstance2.app.Dispose();

                var appInstance3 = await generator(new string[] { "prop1" });
                appInstance3.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance3.wire));

                await Task.Delay(5000);

                var children3 = appInstance3.wire.GetRecursiveData();
                Assert.Collection(children3,
                    i =>
                    {
                        Assert.Equal("test1", i.sync);
                        Assert.Null(i.local);
                        Assert.Equal("test1", i.value);
                        Assert.Equal(LocalDataChangesType.Synced, i.changesType);
                        Assert.Empty(i.path);
                    });

                appInstance3.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetRecursiveDataTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetRecursiveData(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetRecursiveData(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetRecursiveData(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetRecursiveData(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetRecursiveData(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                Assert.Equal("test1", appInstance1.wire.GetValue("prop1"));
                Assert.Equal("test2", appInstance1.wire.GetValue("prop2"));
                Assert.Equal("test3", appInstance1.wire.GetValue("prop3"));
                Assert.Equal("test4", appInstance1.wire.GetValue("prop4", "subProp1"));
                Assert.Equal("test5", appInstance1.wire.GetValue("prop4", "subProp2"));
                Assert.Equal("test6", appInstance1.wire.GetValue("prop4", "subProp3"));

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                Assert.Equal("test1", appInstance1.wire.GetValue("prop1"));
                Assert.Equal("test2", appInstance1.wire.GetValue("prop2"));
                Assert.Equal("test3", appInstance1.wire.GetValue("prop3"));
                Assert.Equal("test4", appInstance1.wire.GetValue("prop4", "subProp1"));
                Assert.Equal("test5", appInstance1.wire.GetValue("prop4", "subProp2"));
                Assert.Equal("test6", appInstance1.wire.GetValue("prop4", "subProp3"));

                appInstance1.app.Dispose();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance2.wire));

                Assert.Equal("test1", appInstance2.wire.GetValue("prop1"));
                Assert.Equal("test2", appInstance2.wire.GetValue("prop2"));
                Assert.Equal("test3", appInstance2.wire.GetValue("prop3"));
                Assert.Equal("test4", appInstance2.wire.GetValue("prop4", "subProp1"));
                Assert.Equal("test5", appInstance2.wire.GetValue("prop4", "subProp2"));
                Assert.Equal("test6", appInstance2.wire.GetValue("prop4", "subProp3"));

                appInstance2.app.Dispose();

                var appInstance3 = await generator(new string[] { "prop1" });
                appInstance3.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance3.wire));

                Assert.Equal("test1", appInstance3.wire.GetValue());

                appInstance3.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(GetValueTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetValue(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetValue(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetValue(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetValue(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.GetValue(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.False(appInstance1.wire.HasChildren());
                Assert.False(appInstance1.wire.HasChildren("prop4"));

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                await Task.Delay(1000);

                Assert.True(appInstance1.wire.HasChildren());
                Assert.True(appInstance1.wire.HasChildren("prop4"));

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                Assert.True(appInstance1.wire.HasChildren());
                Assert.True(appInstance1.wire.HasChildren("prop4"));

                appInstance1.app.Dispose();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance2.wire));

                Assert.True(appInstance2.wire.HasChildren());
                Assert.True(appInstance2.wire.HasChildren("prop4"));

                appInstance2.app.Dispose();

                var appInstance3 = await generator(new string[] { "prop4" });
                appInstance3.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance3.wire));

                Assert.True(appInstance3.wire.HasChildren());

                appInstance3.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(HasChildrenTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.HasChildren(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.HasChildren(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.HasChildren(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.HasChildren(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.HasChildren(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.False(appInstance1.wire.HasFirstStream);

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                Assert.True(appInstance1.wire.HasFirstStream);

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.False(appInstance1.wire.IsLocallyAvailable());
                Assert.False(appInstance1.wire.IsLocallyAvailable("prop1"));
                Assert.False(appInstance1.wire.IsLocallyAvailable("prop4"));

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                Assert.True(appInstance1.wire.IsLocallyAvailable());
                Assert.True(appInstance1.wire.IsLocallyAvailable("prop1"));
                Assert.True(appInstance1.wire.IsLocallyAvailable("prop4"));

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                Assert.True(appInstance1.wire.IsLocallyAvailable());
                Assert.True(appInstance1.wire.IsLocallyAvailable("prop1"));
                Assert.True(appInstance1.wire.IsLocallyAvailable("prop4"));

                appInstance1.app.Dispose();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                Assert.False(appInstance2.wire.IsLocallyAvailable());
                Assert.False(appInstance2.wire.IsLocallyAvailable("prop1"));
                Assert.False(appInstance2.wire.IsLocallyAvailable("prop4"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance2.wire));

                Assert.True(appInstance2.wire.IsLocallyAvailable());
                Assert.True(appInstance2.wire.IsLocallyAvailable("prop1"));
                Assert.True(appInstance2.wire.IsLocallyAvailable("prop4"));

                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(IsLocallyAvailableTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsLocallyAvailable(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsLocallyAvailable(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsLocallyAvailable(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsLocallyAvailable(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsLocallyAvailable(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(appInstance1.wire.IsNull());
                Assert.True(appInstance1.wire.IsNull("prop1"));
                Assert.True(appInstance1.wire.IsNull("prop2"));
                Assert.True(appInstance1.wire.IsNull("prop3"));
                Assert.True(appInstance1.wire.IsNull("prop4"));
                Assert.True(appInstance1.wire.IsNull("prop4", "subProp1"));
                Assert.True(appInstance1.wire.IsNull("prop4", "subProp2"));
                Assert.True(appInstance1.wire.IsNull("prop4", "subProp3"));

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                Assert.False(appInstance1.wire.IsNull());
                Assert.False(appInstance1.wire.IsNull("prop1"));
                Assert.False(appInstance1.wire.IsNull("prop2"));
                Assert.False(appInstance1.wire.IsNull("prop3"));
                Assert.False(appInstance1.wire.IsNull("prop4"));
                Assert.False(appInstance1.wire.IsNull("prop4", "subProp1"));
                Assert.False(appInstance1.wire.IsNull("prop4", "subProp2"));
                Assert.False(appInstance1.wire.IsNull("prop4", "subProp3"));

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                Assert.False(appInstance1.wire.IsNull());
                Assert.False(appInstance1.wire.IsNull("prop1"));
                Assert.False(appInstance1.wire.IsNull("prop2"));
                Assert.False(appInstance1.wire.IsNull("prop3"));
                Assert.False(appInstance1.wire.IsNull("prop4"));
                Assert.False(appInstance1.wire.IsNull("prop4", "subProp1"));
                Assert.False(appInstance1.wire.IsNull("prop4", "subProp2"));
                Assert.False(appInstance1.wire.IsNull("prop4", "subProp3"));

                appInstance1.app.Dispose();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                Assert.True(appInstance2.wire.IsNull());
                Assert.True(appInstance2.wire.IsNull("prop1"));
                Assert.True(appInstance2.wire.IsNull("prop2"));
                Assert.True(appInstance2.wire.IsNull("prop3"));
                Assert.True(appInstance2.wire.IsNull("prop4"));
                Assert.True(appInstance2.wire.IsNull("prop4", "subProp1"));
                Assert.True(appInstance2.wire.IsNull("prop4", "subProp2"));
                Assert.True(appInstance2.wire.IsNull("prop4", "subProp3"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance2.wire));

                Assert.False(appInstance2.wire.IsNull());
                Assert.False(appInstance2.wire.IsNull("prop1"));
                Assert.False(appInstance2.wire.IsNull("prop2"));
                Assert.False(appInstance2.wire.IsNull("prop3"));
                Assert.False(appInstance2.wire.IsNull("prop4"));
                Assert.False(appInstance2.wire.IsNull("prop4", "subProp1"));
                Assert.False(appInstance2.wire.IsNull("prop4", "subProp2"));
                Assert.False(appInstance2.wire.IsNull("prop4", "subProp3"));

                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(IsNullTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsNull(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsNull(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsNull(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsNull(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsNull(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
            });
        }
    }

    public class IsSubPath
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(IsSubPath), nameof(Normal), async generator =>
            {
                var appInstance1 = await generator(new string[] { "path1" });
                appInstance1.wire.Start();

                var appInstance2 = await generator(new string[] { "path1", "path2" });
                appInstance2.wire.Start();

                var appInstance3 = await generator(new string[] { "path1", "path2" });
                appInstance3.wire.Start();

                Assert.True(appInstance1.wire.IsSubPath(appInstance2.wire));
                Assert.False(appInstance2.wire.IsSubPath(appInstance1.wire));
                Assert.True(appInstance1.wire.IsSubPath(appInstance3.wire));
                Assert.False(appInstance2.wire.IsSubPath(appInstance3.wire));

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
                appInstance3.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(IsSubPath), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(ArgumentNullException), () => appInstance1.wire.IsSubPath(null));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                Assert.False(appInstance1.wire.IsSynced());

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                Assert.True(appInstance1.wire.IsSynced());

                appInstance1.app.Dispose();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                Assert.False(appInstance2.wire.IsSynced());

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance2.wire));

                Assert.True(appInstance2.wire.IsSynced());

                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(IsSyncedTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsSynced(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsSynced(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsSynced(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsSynced(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.IsSynced(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.Equal(appInstance1.app.Config.LocalDatabase, appInstance1.wire.LocalDatabase);

                var localDatabase = new SampleLocalDatabase();

                var wire2 = appInstance1.app.Database
                    .Child("users")
                    .Child(appInstance1.app.Auth.Session.LocalId)
                    .Child(nameof(RealtimeModuleTest))
                    .Child(nameof(LocalDatabaseTest))
                    .Child(nameof(Normal))
                    .AsRealtimeWire(localDatabase);
                wire2.Start();

                Assert.Equal(localDatabase, wire2.LocalDatabase);

                wire2.SetValue("test1", "prop1");

                await Task.Delay(1000);

                var wire3 = appInstance1.app.Database
                    .Child("users")
                    .Child(appInstance1.app.Auth.Session.LocalId)
                    .Child(nameof(RealtimeModuleTest))
                    .Child(nameof(LocalDatabaseTest))
                    .Child(nameof(Normal))
                    .AsRealtimeWire(localDatabase);
                wire3.Start();

                Assert.Null(appInstance1.wire.GetValue("prop1"));
                Assert.Equal("test1", wire2.GetValue("prop1"));
                Assert.Equal("test1", wire3.GetValue("prop1"));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                Assert.False(appInstance1.wire.IsSynced());

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 100;
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                Assert.True(appInstance1.wire.IsSynced());

                appInstance1.app.Dispose();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                Assert.False(appInstance2.wire.IsSynced());

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance2.wire));

                Assert.True(appInstance2.wire.IsSynced());

                appInstance2.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                var child1 = appInstance1.wire.Child("child1");
                var child1DataChanges = new List<DataChangesEventArgs>();
                child1.ImmediateDataChanges += (s, e) =>
                {
                    child1DataChanges.Add(e);
                };
                child1.Error += (s, e) =>
                {
                    Assert.True(false, e.Exception.Message);
                };

                var child2 = child1.Child("child2");
                var child2DataChanges = new List<DataChangesEventArgs>();
                child2.ImmediateDataChanges += (s, e) =>
                {
                    child2DataChanges.Add(e);
                };
                child2.Error += (s, e) =>
                {
                    Assert.True(false, e.Exception.Message);
                };

                Assert.Null(appInstance1.wire.Parent);
                Assert.Equal(appInstance1.wire, child1.Parent);
                Assert.Equal(child1, child2.Parent);

                Assert.Empty(appInstance1.dataChanges);
                Assert.Empty(child1DataChanges);
                Assert.Empty(child2DataChanges);

                Assert.True(child2.SetValue("test"));

                await Task.Delay(1000);

                Assert.Collection(appInstance1.dataChanges,
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

                appInstance1.wire.SetNull();
                await Task.Delay(1000);
                appInstance1.dataChanges.Clear();
                child1DataChanges.Clear();
                child2DataChanges.Clear();

                Assert.True(child1.SetValue("test"));

                await Task.Delay(1000);

                Assert.Collection(appInstance1.dataChanges,
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

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();


                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(PutModelTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseInvalidModel), () => appInstance1.wire.PutModel(new PutModelErrorTest(), new string[] { "path" }));

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.PutModel(new FirebaseProperty<string>(), new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.PutModel(new FirebaseProperty<string>(), new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.PutModel(new FirebaseProperty<string>(), new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.PutModel(new FirebaseProperty<string>(), new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.PutModel(new FirebaseProperty<string>(), new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                var appInstance2 = await generator(null);
                var test1WireClone = appInstance1.wire.Clone();
                var test2WireClone = appInstance2.wire.Clone();

                Assert.Equal(appInstance1.wire.Query.GetAbsoluteUrl(), appInstance2.wire.Query.GetAbsoluteUrl());
                Assert.Equal(test1WireClone.Query.GetAbsoluteUrl(), test2WireClone.Query.GetAbsoluteUrl());
                Assert.Equal(appInstance1.wire.Query.GetAbsoluteUrl(), test2WireClone.Query.GetAbsoluteUrl());
                Assert.Equal(test1WireClone.Query.GetAbsoluteUrl(), appInstance2.wire.Query.GetAbsoluteUrl());

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
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
                var appInstance1 = await generator(null);
                var test1WireClone = appInstance1.wire.Clone();
                var test1WireChild1 = test1WireClone.Child("child");
                var test1WireChild2 = test1WireChild1.Child("child");

                Assert.Equal(null, appInstance1.wire.Root);
                Assert.Equal(null, test1WireClone.Root);
                Assert.Equal(test1WireChild1.Root, test1WireClone);
                Assert.Equal(test1WireChild2.Root, test1WireClone);
                Assert.Equal(test1WireChild2.Root, test1WireChild1.Root);

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));
                appInstance1.dataChanges.Clear();

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.True(appInstance1.wire.SetValue("test3", "prop3"));
                Assert.True(appInstance1.wire.SetValue("test4", "prop4", "subProp1"));
                Assert.True(appInstance1.wire.SetValue("test5", "prop4", "subProp2"));
                Assert.True(appInstance1.wire.SetValue("test6", "prop4", "subProp3"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));
                appInstance1.dataChanges.Clear();

                Assert.True(appInstance1.wire.SetNull("prop1"));
                Assert.False(appInstance1.wire.SetNull("prop1"));

                var phase1 = appInstance1.wire.GetRecursiveData();
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

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase2 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                Assert.True(appInstance1.wire.SetNull("prop4"));
                Assert.False(appInstance1.wire.SetNull("prop4"));

                var phase3 = appInstance1.wire.GetRecursiveData();
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

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase4 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                Assert.True(appInstance1.wire.SetNull());
                Assert.False(appInstance1.wire.SetNull());

                var phase5 = appInstance1.wire.GetRecursiveData();
                Assert.Collection(phase5,
                    i =>
                    {
                        Assert.Equal(null, i.sync);
                        Assert.Equal(null, i.local);
                        Assert.Equal(null, i.value);
                        Assert.Equal(LocalDataChangesType.Delete, i.changesType);
                        Assert.Equal(0, i.path.Length);
                    });

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase6 = appInstance1.wire.GetRecursiveData();
                Assert.Empty(phase6);

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();
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

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));

                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));
                appInstance1.dataChanges.Clear();

                Assert.True(origin1.wire.SetNull("prop1"));
                Assert.False(origin1.wire.SetNull("prop1"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));
                await Task.Delay(5000);

                var phase1 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                appInstance1.dataChanges.Clear();

                appInstance1.wire.Stop();

                Assert.True(origin1.wire.SetNull("prop4"));
                Assert.False(origin1.wire.SetNull("prop4"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));

                Assert.True(appInstance1.wire.SetValue("test4Mod", "prop4", "subProp1"));
                Assert.False(appInstance1.wire.SetValue("test4Mod", "prop4", "subProp1"));

                var phase2 = appInstance1.wire.GetRecursiveData();
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

                appInstance1.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase3 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                appInstance1.wire.Stop();

                Assert.True(origin1.wire.SetValue("test2Mod", "prop2"));
                Assert.False(origin1.wire.SetValue("test2Mod", "prop2"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));

                Assert.True(appInstance1.wire.SetNull("prop2"));
                Assert.False(appInstance1.wire.SetNull("prop2"));

                var phase5 = appInstance1.wire.GetRecursiveData();
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

                appInstance1.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase6 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                Assert.True(origin1.wire.SetNull());
                Assert.False(origin1.wire.SetNull());

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));
                await Task.Delay(5000);

                var phase7 = appInstance1.wire.GetRecursiveData();
                Assert.Empty(phase7);

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                origin1.app.Dispose();
                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(SetNullTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SetNull(new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SetNull(new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SetNull(new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SetNull(new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SetNull(new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));
                appInstance1.dataChanges.Clear();

                Assert.True(appInstance1.wire.SetValue("test1", "prop1"));
                Assert.False(appInstance1.wire.SetValue("test1", "prop1"));

                var phase1 = appInstance1.wire.GetRecursiveData();
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

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase2 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                Assert.True(appInstance1.wire.SetValue("test2", "prop2"));
                Assert.False(appInstance1.wire.SetValue("test2", "prop2"));

                var phase3 = appInstance1.wire.GetRecursiveData();
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

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase4 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                Assert.True(appInstance1.wire.SetValue("test3", "prop3", "subProp1"));
                Assert.False(appInstance1.wire.SetValue("test3", "prop3", "subProp1"));

                var phase5 = appInstance1.wire.GetRecursiveData();
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

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase6 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                Assert.True(appInstance1.wire.SetValue("test4", "prop3", "subProp2"));
                Assert.False(appInstance1.wire.SetValue("test4", "prop3", "subProp2"));

                var phase7 = appInstance1.wire.GetRecursiveData();
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

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase8 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                Assert.True(appInstance1.wire.SetValue(null, "prop1"));
                Assert.False(appInstance1.wire.SetValue(null, "prop1"));

                var phase9 = appInstance1.wire.GetRecursiveData();
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

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase10 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                Assert.True(appInstance1.wire.SetValue("test2Mod", "prop2"));
                Assert.False(appInstance1.wire.SetValue("test2Mod", "prop2"));

                var phase11 = appInstance1.wire.GetRecursiveData();
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

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase12 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void CrossSync()
        {
            await Helpers.CleanTest(nameof(SetValueTest), nameof(CrossSync), async generator =>
            {
                var origin1 = await generator(null);
                origin1.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));
                origin1.dataChanges.Clear();

                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));
                appInstance1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue("test1", "prop1"));
                Assert.False(origin1.wire.SetValue("test1", "prop1"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));
                await Task.Delay(5000);

                var phase1 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    });
                appInstance1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue("test2", "prop2"));
                Assert.False(origin1.wire.SetValue("test2", "prop2"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));
                await Task.Delay(5000);

                var phase2 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    });
                appInstance1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue("test3", "prop3", "subProp1"));
                Assert.False(origin1.wire.SetValue("test3", "prop3", "subProp1"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));
                await Task.Delay(5000);

                var phase3 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue("test4", "prop3", "subProp2"));
                Assert.False(origin1.wire.SetValue("test4", "prop3", "subProp2"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));
                await Task.Delay(5000);

                var phase4 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue(null, "prop1"));
                Assert.False(origin1.wire.SetValue(null, "prop1"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));
                await Task.Delay(5000);

                var phase5 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop1", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                appInstance1.dataChanges.Clear();

                Assert.True(origin1.wire.SetValue("test2Mod", "prop2"));
                Assert.False(origin1.wire.SetValue("test2Mod", "prop2"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));
                await Task.Delay(10000);

                var phase6 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("prop2", i.Path[0]);
                    });
                appInstance1.dataChanges.Clear();

                appInstance1.wire.Stop();

                Assert.True(origin1.wire.SetValue("test2Mod2", "prop2"));
                Assert.False(origin1.wire.SetValue("test2Mod2", "prop2"));

                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(origin1.wire));

                Assert.True(appInstance1.wire.SetValue("conflict", "prop2"));
                Assert.False(appInstance1.wire.SetValue("conflict", "prop2"));

                var phase7 = appInstance1.wire.GetRecursiveData();
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

                appInstance1.wire.Start();
                Assert.True(await RestfulFirebase.Test.Helpers.WaitForSynced(appInstance1.wire));

                var phase8 = appInstance1.wire.GetRecursiveData();
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

                Assert.Collection(appInstance1.dataChanges,
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
                appInstance1.dataChanges.Clear();

                origin1.app.Dispose();
                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(SetValueTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SetValue("test", new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SetValue("test", new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SetValue("test", new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SetValue("test", new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SetValue("test", new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);

                Assert.False(appInstance1.wire.Started);

                appInstance1.wire.Start();

                Assert.True(appInstance1.wire.Started);

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);

                Assert.False(appInstance1.wire.Started);

                appInstance1.wire.Start();

                Assert.True(appInstance1.wire.Started);

                appInstance1.wire.Stop();

                Assert.False(appInstance1.wire.Started);

                appInstance1.wire.Start();

                Assert.True(appInstance1.wire.Started);

                appInstance1.app.Dispose();

                Assert.False(appInstance1.wire.Started);
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.True(appInstance1.wire.Started);

                appInstance1.wire.Stop();

                Assert.False(appInstance1.wire.Started);

                appInstance1.app.Dispose();
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
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();


                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void Throws()
        {
            await Helpers.CleanTest(nameof(SubModelTest), nameof(Throws), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                Assert.Throws(typeof(DatabaseInvalidModel), () => appInstance1.wire.SubModel(new SubModelErrorTest(), new string[] { "path" }));

                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SubModel(new FirebaseProperty<string>(), new string[] { "path", "1.1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SubModel(new FirebaseProperty<string>(), new string[] { "path", "1#1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SubModel(new FirebaseProperty<string>(), new string[] { "path", "1$1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SubModel(new FirebaseProperty<string>(), new string[] { "path", "1[1" }));
                Assert.Throws(typeof(DatabaseForbiddenNodeNameCharacter), () => appInstance1.wire.SubModel(new FirebaseProperty<string>(), new string[] { "path", "1]1" }));

                appInstance1.app.Dispose();
            });
        }
    }

    public class WaitForSynced
    {
        [Fact]
        public async void Normal()
        {
            var instance = await RestfulFirebase.Test.Helpers.AuthenticatedAppGenerator();
            var app1 = await instance.generator();
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
            
            Assert.True(await Task.Run(async delegate
            {
                for (int i = 0; i < 10; i++)
                {
                    if (await wire1.WaitForSynced(TimeSpan.FromMinutes(1)))
                    {
                        return true;
                    }
                }
                return false;
            }));

            app1.Dispose();

            var app2 = await instance.generator();
            var wire2 = app2.Database
                .Child("unauthorized")
                .AsRealtimeWire();
            var wire2Errors = new List<WireExceptionEventArgs>();
            wire2.Error += (s, e) =>
            {
                wire2Errors.Add(e);
            };

            wire2.Start();
            Assert.False(await wire2.WaitForSynced(TimeSpan.FromMinutes(5)));

            instance.dispose();
        }
    }
}
