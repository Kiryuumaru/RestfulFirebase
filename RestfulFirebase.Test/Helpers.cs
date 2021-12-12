using RestfulFirebase;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Local;
using RestfulFirebase.Test.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestfulFirebase.Test
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
        private const int MaxAppInstances = 10;

        private static RestfulFirebaseApp? app;
        private static bool appInitializing = false;
        private static int appInstanceCount = 0;
        private static object appInstancesLocker = new object();

        public static Func<Task<RestfulFirebaseApp>> AppGenerator()
        {
            return new Func<Task<RestfulFirebaseApp>>(
                async delegate
                {
                    while (true)
                    {
                        lock (appInstancesLocker)
                        {
                            if (appInstanceCount < MaxAppInstances)
                            {
                                appInstanceCount++;
                                break;
                            }
                        }
                        await Task.Delay(1000);
                    }
                    FirebaseConfig config = Config.YourConfig();
                    config.LocalDatabase = new SampleLocalDatabase();
                    RestfulFirebaseApp app = new RestfulFirebaseApp(config);
                    app.Disposing += App_Disposing;
                    return app;
                });
        }

        private static void App_Disposing(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                RestfulFirebaseApp app = (RestfulFirebaseApp)sender;
                app.Disposing -= App_Disposing;
            }
            lock (appInstancesLocker)
            {
                appInstanceCount--;
            }
        }

        public static async Task<Func<Task<RestfulFirebaseApp>>> AuthenticatedAppGenerator()
        {
            var generator = AppGenerator();
            if (app == null)
            {
                if (!appInitializing)
                {
                    appInitializing = true;
                    var initApp = await generator();
                    await initApp.Auth.SignInWithEmailAndPassword("t@st.com", "123123");
                    app = initApp;
                    appInitializing = false;
                }
                else
                {
                    while (appInitializing)
                    {
                        await Task.Delay(1000);
                    }
                }
            }

            return new Func<Task<RestfulFirebaseApp>>(
                async delegate
                {
                    var appCopy = await generator();
                    appCopy.Auth.CopyAuthenticationFrom(app);
                    return appCopy;
                });
        }

        public static async Task<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>> AuthenticatedTestApp(
            string unitName,
            string testName,
            string factName)
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
                            .Child(unitName)
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
                            .Child(unitName)
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
            string unitName,
            string testName,
            string factName,
            Func<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>, Task> test)
        {
            var appGenerator = await AuthenticatedTestApp(unitName, testName, factName);

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
            string unitName,
            string testName,
            string factName,
            Action<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>> test)
        {
            return CleanTest(unitName, testName, factName, t => Task.Run(delegate { test(t); }));
        }
    }
}
