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

        public static RestfulFirebaseApp GenerateApp()
        {
            FirebaseConfig config = Config.YourConfig();
            config.LocalDatabase = new SampleLocalDatabase();
            return new RestfulFirebaseApp(config);
        }

        public static (Func<Task<RestfulFirebaseApp>> generator, Action dispose) AppGenerator()
        {
            List<RestfulFirebaseApp> apps = new List<RestfulFirebaseApp>();
            Func<Task<RestfulFirebaseApp>> generator = async delegate
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
                RestfulFirebaseApp app = GenerateApp();

                void App_Disposing(object? sender, EventArgs e)
                {
                    if (sender != null)
                    {
                        app.Disposing -= App_Disposing;
                    }
                    lock (appInstancesLocker)
                    {
                        apps.Remove(app);
                        appInstanceCount--;
                    }
                }

                lock (appInstancesLocker)
                {
                    apps.Add(app);
                }
                app.Disposing += App_Disposing;
                return app;
            };
            Action dispose = () =>
            {
                List<RestfulFirebaseApp> currentApps;
                lock (appInstancesLocker)
                {
                    currentApps = apps.ToList();
                }
                foreach (var app in currentApps)
                {
                    app.Dispose();
                }
            };
            return (generator, dispose);
        }

        public static async Task<(Func<Task<RestfulFirebaseApp>> generator, Action dispose)> AuthenticatedAppGenerator()
        {
            if (app == null)
            {
                if (!appInitializing)
                {
                    appInitializing = true;
                    RestfulFirebaseApp initApp = GenerateApp();
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

            var instance = AppGenerator();

            return (new Func<Task<RestfulFirebaseApp>>(
                async delegate
                {
                    var appCopy = await instance.generator();
                    appCopy.Auth.CopyAuthenticationFrom(app);
                    return appCopy;
                }), instance.dispose);
        }

        public static async Task<(Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>> generator, Action dispose)> AuthenticatedTestApp(
            string unitName,
            string testName,
            string factName)
        {
            var instance = await RestfulFirebase.Test.Helpers.AuthenticatedAppGenerator();
            var generator = new Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>(async subNode =>
            {
                RestfulFirebaseApp app = await instance.generator();

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
                wire.ImmediateDataChanges += (s, e) =>
                {
                    dataChanges.Add(e);
                };

                return (app, wire, dataChanges);
            });
            return (generator, instance.dispose);
        }

        public static async Task CleanTest(
            string unitName,
            string testName,
            string factName,
            Func<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>, Task> test)
        {
            var instance = await AuthenticatedTestApp(unitName, testName, factName);

            try
            {
                var app1 = await instance.generator(null);
                app1.wire.Start();
                app1.wire.SetNull();
                Assert.True(await WaitForSynced(app1.wire));
                app1.app.Dispose();

                await test(instance.generator);

                var app2 = await instance.generator(null);
                app2.wire.Start();
                app2.wire.SetNull();
                Assert.True(await WaitForSynced(app2.wire));
                app2.app.Dispose();
            }
            finally
            {
                instance.dispose();
            }
        }

        public static Task CleanTest(
            string unitName,
            string testName,
            string factName,
            Action<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>> test)
        {
            return CleanTest(unitName, testName, factName, t => Task.Run(delegate { test(t); }));
        }

        public static async Task<bool> WaitForSynced(RealtimeInstance realtimeInstance)
        {
            for (int i = 0; i < 10; i++)
            {
                if (await realtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
