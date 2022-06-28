using RestfulFirebase;
using RestfulFirebase.RealtimeDatabase.Realtime;
using RestfulFirebase.Local;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RestfulFirebase.TestCore;

public class SampleLocalDatabase : ILocalDatabase
{
    public ConcurrentDictionary<string, string?> Db { get; } = new();

    public bool ContainsKey(string key)
    {
        return Db.ContainsKey(key);
    }

    public string? Get(string key)
    {
        if (!Db.TryGetValue(key, out string? value))
        {
            return null;
        }
        return value;
    }

    public void Set(string key, string? value)
    {
        Db.AddOrUpdate(key, value, delegate { return value; });
    }

    public void Delete(string key)
    {
        Db.TryRemove(key, out _);
    }

    public void Clear()
    {
        Db.Clear();
    }
}

public class Helpers
{
    private const int MaxAppInstances = 10;
    private const int WaitErrorNumTries = 5;
    private const int TestErrorNumTries = 5;

    private static RestfulFirebaseApp? app;
    private static bool appInitializing = false;
    private static int appInstanceCount = 0;
    private static readonly object appInstancesLocker = new();

    public static RestfulFirebaseApp GenerateApp()
    {
        FirebaseConfig config = Credentials.YourConfig();
        config.LocalDatabase = new SampleLocalDatabase();
        return new RestfulFirebaseApp(config);
    }

    public static (Func<Task<RestfulFirebaseApp>> generator, Action dispose) AppGenerator()
    {
        List<RestfulFirebaseApp> apps = new();
        async Task<RestfulFirebaseApp> generator()
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
        }
        void dispose()
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
        }
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

        var (generator, dispose) = AppGenerator();

        return (new Func<Task<RestfulFirebaseApp>>(
            async delegate
            {
                var appCopy = await generator();
                if (app != null)
                {
                    appCopy.Auth.CopyAuthenticationFrom(app);
                }
                return appCopy;
            }), dispose);
    }

    public static async Task<(Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>> generator, Action dispose)> AuthenticatedTestApp(
        string unitName,
        string testName,
        string factName)
    {
        var instance = await AuthenticatedAppGenerator();
        var generator = new Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>(async subNode =>
        {
            RestfulFirebaseApp app = await instance.generator();

            if (app.Auth.Session?.LocalId == null)
            {
                throw new Exception("Not authenticated.");
            }

            RealtimeWire wire;
            subNode ??= Array.Empty<string>();
            if (subNode.Length == 0)
            {
                wire = app.RealtimeDatabase
                    .Database(Credentials.DefaultRealtimeDatabaseUrl)
                    .Child("users")
                    .Child(app.Auth.Session.LocalId)
                    .Child(unitName)
                    .Child(testName)
                    .Child(factName)
                    .AsRealtimeWire();
            }
            else
            {
                StringBuilder builder = new();
                foreach (var subPath in subNode)
                {
                    if (string.IsNullOrEmpty(subPath))
                    {
                        builder.Append('/');
                    }
                    else
                    {
                        builder.Append(subPath);
                        if (!subPath.EndsWith("/"))
                        {
                            builder.Append('/');
                        }
                    }
                }
                string additionalPath = builder.ToString();
                additionalPath = additionalPath[0..^1];
                wire = app.RealtimeDatabase
                    .Database(Credentials.DefaultRealtimeDatabaseUrl)
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
        return (generator, instance.dispose);
    }

    public static async Task CleanTest(
        string unitName,
        string testName,
        string factName,
        Action<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>> test)
    {
        (Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>> generator, Action dispose)? instance;

        for (int i = 0; i < TestErrorNumTries; i++)
        {
            instance = null;

            try
            {
                instance = await AuthenticatedTestApp(unitName, testName, factName);

                var app1 = await instance.Value.generator(null);
                app1.wire.Start();
                app1.wire.SetNull();
                Assert.True(await WaitForSynced(app1.wire));
                app1.app.Dispose();

                test(instance.Value.generator);

                var app2 = await instance.Value.generator(null);
                app2.wire.Start();
                app2.wire.SetNull();
                Assert.True(await WaitForSynced(app2.wire));
                app2.app.Dispose();

                break;
            }
            catch
            {
                if (i >= TestErrorNumTries - 1)
                {
                    throw;
                }

                await Task.Delay(5000);
            }
            finally
            {
                instance?.dispose();
            }
        }
    }

    public static async Task CleanTest(
        string unitName,
        string testName,
        string factName,
        Func<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>, Task> test)
    {
        (Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>> generator, Action dispose)? instance;

        for (int i = 0; i < TestErrorNumTries; i++)
        {
            instance = null;

            try
            {
                instance = await AuthenticatedTestApp(unitName, testName, factName);

                var app1 = await instance.Value.generator(null);
                app1.wire.Start();
                app1.wire.SetNull();
                Assert.True(await WaitForSynced(app1.wire));
                app1.app.Dispose();

                await test(instance.Value.generator);

                var app2 = await instance.Value.generator(null);
                app2.wire.Start();
                app2.wire.SetNull();
                Assert.True(await WaitForSynced(app2.wire));
                app2.app.Dispose();

                break;
            }
            catch
            {
                if (i >= TestErrorNumTries - 1)
                {
                    throw;
                }

                await Task.Delay(5000);
            }
            finally
            {
                instance?.dispose();
            }
        }
    }

    public static async Task<bool> WaitForSynced(RealtimeInstance? realtimeInstance)
    {
        if (realtimeInstance == null)
        {
            return false;
        }
        for (int i = 0; i < WaitErrorNumTries; i++)
        {
            if (await realtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)))
            {
                return true;
            }
        }
        return false;
    }
}
