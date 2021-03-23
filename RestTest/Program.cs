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

            var props1 = FirebaseProperty.CreateFromKeyAndValue("keyD", 999.9299);
            var props2 = FirebaseProperty.CreateFromKeyAndValue("keyS", "numba22");
            var props31 = TestStorable.Create();
            var props32 = TestStorable.Create();
            var props33 = TestStorable.Create();

            int x11 = 0;

            await app.Auth.SignInWithEmailAndPasswordAsync("t@st.com", "123123");
            await app.Auth.UpdateProfileAsync("disp", "123123");
            var userNode = app.Database.Child("users").Child(app.Auth.User.LocalId);
            await userNode.Child("propCollection").SetAsync(props1);
            await userNode.Child("propCollection").SetAsync(props2);
            await userNode.Child("objCollection").SetAsync(props31);
            await userNode.Child("objCollection").SetAsync(props32);
            await userNode.Child("objCollection").SetAsync(props33);

            var ss1 = await userNode.GetAsPropertyCollectionAsync("propCollection");
            var ss2 = await userNode.GetAsObjectCollectionAsync("objCollection");
            ss1.CollectionChanged += (s, e) =>
            {

            };
            ss2.CollectionChanged += (s, e) =>
            {

            };
            ss2[0].PropertyChanged += (s, e) =>
            {

            };

            Console.WriteLine("FIN");

            await Task.Delay(10000000);

            //foreach (var dino in dinos)
            //{
            //    Console.WriteLine($"{dino.Key} is {dino.Object.Height}m high.");
            //}
        }
    }
}
