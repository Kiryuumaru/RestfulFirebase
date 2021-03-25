using RestfulFirebase.Auth;
using RestfulFirebase.Database;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestfulFirebase.Common;
using RestfulFirebase;
using RestfulFirebase.Database.Models;
using System.Collections.ObjectModel;

namespace RestTest
{
    public class TestStorable : FirebaseObject
    {
        #region Properties

        private DateTime SSS
        {
            get => GetPersistableProperty<DateTime>("ss");
            set => SetPersistableProperty(value, "ss");
        }

        public DateTime Created
        {
            get => GetPersistableProperty<DateTime>("_cr");
            set => SetPersistableProperty(value, "_cr");
        }
        public DateTime Modified
        {
            get => GetPersistableProperty<DateTime>("_md");
            set => SetPersistableProperty(value, "_md");
        }

        public bool IsOk
        {
            get => GetPersistableProperty<bool>("isOk");
            set => SetPersistableProperty(value, "isOk");
        }

        public TimeSpan Premium
        {
            get => GetPersistableProperty<TimeSpan>("premium");
            set => SetPersistableProperty(value, "premium");
        }

        #endregion

        #region Initializers

        public static new TestStorable Create()
        {
            return new TestStorable(FirebaseObject.Create())
            {
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };
        }

        public static TestStorable Create(string key)
        {
            return new TestStorable(CreateFromKey(key))
            {
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };
        }

        public static TestStorable Create(string key, DateTime created, DateTime modified)
        {
            return new TestStorable(CreateFromKey(key))
            {
                Created = created,
                Modified = modified
            };
        }

        public static TestStorable Create(string key, IEnumerable<(string Key, string Data)> properties)
        {
            return new TestStorable(CreateFromKeyAndProperties(key, properties));
        }

        public TestStorable(IAttributed attributed) : base(attributed)
        {

        }

        #endregion

        #region Methods



        #endregion
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Run().GetAwaiter().GetResult();
        }

        public static async Task Run()
        {
            var app = new RestfulFirebaseApp(new FirebaseConfig()
            {
                ApiKey = "AIzaSyBZfLYmm5SyxmBk0lzBh0_AcDILjOLUD9o",
                DatabaseURL = "https://restfulplayground-default-rtdb.firebaseio.com/",
                StorageBucket = "restfulplayground.appspot.com"
            });

            app.Database.OfflineDatabase.Set("a/b/c/", new OfflineData("s", "s"));
            app.Database.OfflineDatabase.Set("a/b/c", new OfflineData("a", "a"));
            app.Database.OfflineDatabase.Set("a/b/q", new OfflineData("a", "a"));
            app.Database.OfflineDatabase.Set("a/b/cd", new OfflineData("a", "a"));
            app.Database.OfflineDatabase.Set("a/s", new OfflineData("a", "a"));
            app.Database.OfflineDatabase.Set("a/s/s", new OfflineData("a", "a"));
            app.Database.OfflineDatabase.Set("a/s/s/a", new OfflineData("a", "a"));
            var ssasa = app.Database.OfflineDatabase.Get("a/b/c");
            var ssas = app.Database.OfflineDatabase.Get("a/b/c/");
            var ssacas = app.Database.OfflineDatabase.GetSubPaths("a/b/");
            var ssaacas = app.Database.OfflineDatabase.GetSubPaths("a/s/");

            var props1 = FirebaseProperty.CreateFromKeyAndValue("keyD", 999.9299);
            var props2 = FirebaseProperty.CreateFromKeyAndValue("keyS", "numba22");
            var props31 = TestStorable.Create();
            var props32 = TestStorable.Create();
            var props33 = TestStorable.Create();

            int x11 = 0;

            await app.Auth.SignInWithEmailAndPasswordAsync("t@st.com", "123123");
            await app.Auth.UpdateProfileAsync("disp", "123123");
            var userNode = app.Database.Child("users").Child(app.Auth.User.LocalId);
            userNode.Child("propCollection").Set(props1);
            userNode.Child("propCollection").Set(props2);
            userNode.Child("objCollection").Set(props31);
            userNode.Child("objCollection").Set(props32);
            userNode.Child("objCollection").Set(props33);

            var ss11 = userNode.Child("objCollection").GetAsObject(props31.Key);
            var ss1 = userNode.GetAsPropertyCollection("propCollection");
            var ss2 = userNode.GetAsObjectCollection("objCollection");

            ss11.PropertyChanged += (s, e) =>
            {

            };
            ss1.CollectionChanged += (s, e) =>
            {

            };
            ss2.CollectionChanged += (s, e) =>
            {

            };

            Console.WriteLine("FIN");

            await Task.Delay(10000000);
        }
    }
}
