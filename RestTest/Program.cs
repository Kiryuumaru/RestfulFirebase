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

        public static TestStorable Create(string key, IEnumerable<DistinctProperty> properties)
        {
            return new TestStorable(CreateFromKeyAndProperties(key, properties));
        }

        public TestStorable(AttributeHolder holder) : base(holder)
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

            var props1 = FirebaseProperty.CreateFromKeyAndValue("keyD", 999.9299);
            var props2 = FirebaseProperty.CreateFromKeyAndValue("keyS", "numba22");
            var props3 = TestStorable.Create();

            var pr = props3.GetRawPersistableProperties().ToList();

            int x11 = 0;

            await app.Database.Child("public").Child("prop").SetAsync(props1);
            await app.Database.Child("public").Child("prop").SetAsync(props2);
            await app.Database.Child("public").Child("prop").SetAsync(props3);

            //try
            //{
            //    await app.Database.Child("users").Child("a").Child("prop").SetAsync(props1);
            //    await app.Database.Child("users").Child("a").Child("prop").SetAsync(props2);
            //    await app.Database.Child("users").Child("a").Child("prop").SetAsync(props3);
            //}
            //catch(FirebaseException s)
            //{

            //}

            var ss = props3.Key;

            await app.Auth.SignInWithEmailAndPasswordAsync("t@st.com", "123123");
            await app.Auth.UpdateProfileAsync("disp", "123123");
            await app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop").SetAsync(props1);
            await app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop").SetAsync(props2);
            await app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop").SetAsync(props3);

            var node = app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop");
            var path = node.GetAbsolutePath();

            var ss1 = await app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop").GetAsPropertyAsync<double>(props1.Key);
            var ss2 = await app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop").GetAsPropertyAsync<string>(props2.Key);
            var ss3 = await node.GetAsObjectAsync<TestStorable>(props3.Key);

            //var dinos = await firebase
            //    .Child("ss").AsRealtimeDatabase("", "", StreamingOptions.LatestOnly, InitialPullStrategy.MissingOnly, true)
            //    .OnceAsync<object>();

            int x = 0;

            await Task.Delay(10000000);

            //foreach (var dino in dinos)
            //{
            //    Console.WriteLine($"{dino.Key} is {dino.Object.Height}m high.");
            //}
        }
    }
}
