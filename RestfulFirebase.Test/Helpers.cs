using RestfulFirebase;
using RestfulFirebase.Local;
using RestfulFirebase.Test.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
