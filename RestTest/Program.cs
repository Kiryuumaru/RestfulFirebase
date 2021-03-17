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

namespace RestTest
{
    public class TestStorable : FirebaseObject
    {
        #region Properties

        public DateTime Created
        {
            get => GetProperty<DateTime>("_cr");
            set => SetPersistableProperty(value, "_cr", nameof(Created));
        }
        public DateTime Modified
        {
            get => GetProperty<DateTime>("_md");
            set => SetPersistableProperty(value, "_md", nameof(Modified));
        }

        #endregion

        #region Initializers

        public TestStorable()
        {
            Created = DateTime.UtcNow;
            Modified = DateTime.UtcNow;
        }

        public TestStorable(ObservableObjectHolder holder) : base(holder)
        {
            Created = DateTime.UtcNow;
            Modified = DateTime.UtcNow;
        }

        public TestStorable(string id) : base(id)
        {
            Created = DateTime.UtcNow;
            Modified = DateTime.UtcNow;
        }

        public TestStorable(string key, DateTime created, DateTime modified) : base(key)
        {
            Created = created;
            Modified = modified;
        }

        public TestStorable(string key, IEnumerable<DistinctProperty> cellModels) : base(key, cellModels)
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

            var props1 = FirebaseProperty.Create(999.9299, "keyD");
            var props2 = FirebaseProperty.Create("numba22", "keyS");
            var props3 = new TestStorable();

            await app.Database.Child("public").Child("prop").SetAsync(props1);
            await app.Database.Child("public").Child("prop").SetAsync(props2);
            await app.Database.Child("public").Child("prop").SetAsync(props3);

            try
            {
                await app.Database.Child("users").Child("a").Child("prop").SetAsync(props1);
                await app.Database.Child("users").Child("a").Child("prop").SetAsync(props2);
                await app.Database.Child("users").Child("a").Child("prop").SetAsync(props3);
            }
            catch(FirebaseException s)
            {

            }

            var ss = props3.Key;

            await app.Auth.SignInWithEmailAndPasswordAsync("t@st.com", "123123");
            await app.Auth.UpdateProfileAsync("disp", "123123");
            await app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop").SetAsync(props1);
            await app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop").SetAsync(props2);
            await app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop").SetAsync(props3);

            var ss1 = await app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop").GetAsPropertyAsync<double>(props1.Key);
            var ss2 = await app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop").GetAsPropertyAsync<string>(props2.Key);
            var ss3 = await app.Database.Child("users").Child(app.Auth.User.LocalId).Child("prop").GetAsStorableAsync<TestStorable>(props3.Key);

            //var dinos = await firebase
            //    .Child("ss").AsRealtimeDatabase("", "", StreamingOptions.LatestOnly, InitialPullStrategy.MissingOnly, true)
            //    .OnceAsync<object>();

            int x = 0;

            await Task.Delay(100000);

            //foreach (var dino in dinos)
            //{
            //    Console.WriteLine($"{dino.Key} is {dino.Object.Height}m high.");
            //}
        }
    }
}
