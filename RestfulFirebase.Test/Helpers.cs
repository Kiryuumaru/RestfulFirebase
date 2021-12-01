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
        private static RestfulFirebaseApp? app;
        private static bool appInitializing = false;

        public static Func<RestfulFirebaseApp> AppGenerator()
        {
            return new Func<RestfulFirebaseApp>(
                delegate
                {
                    FirebaseConfig config = Config.YourConfig();
                    config.LocalDatabase = new SampleLocalDatabase();
                    return new RestfulFirebaseApp(config);
                });
        }

        public static async Task<Func<RestfulFirebaseApp>> AuthenticatedAppGenerator()
        {
            var generator = AppGenerator();
            if (app == null)
            {
                if (!appInitializing)
                {
                    appInitializing = true;
                    var initApp = generator();
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

            return new Func<RestfulFirebaseApp>(
                delegate
                {
                    var appCopy = generator();
                    appCopy.Auth.CopyAuthenticationFrom(app);
                    return appCopy;
                });
        }
    }
}
