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
using System.Diagnostics;
using RestfulFirebase.Common.Conversions;

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
                Modified = DateTime.UtcNow
            };
        }

        public static TestStorable Create(string key)
        {
            return new TestStorable(CreateFromKey(key))
            {
                Modified = DateTime.UtcNow
            };
        }

        public static TestStorable Create(string key, DateTime created, DateTime modified)
        {
            return new TestStorable(CreateFromKey(key))
            {
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

            var props1 = FirebaseProperty.CreateFromKeyAndValue("keyS", "numba22");
            var props2 = FirebaseProperty.CreateFromKeyAndValue("keyD", 999.9299);
            var props31 = TestStorable.Create();
            var props32 = TestStorable.Create();
            var props33 = TestStorable.Create();

            props1.Modified = DateTime.UtcNow;
            props2.Modified = DateTime.UtcNow;

            int x11 = 0;

            var signInResult = await app.Auth.SignInWithEmailAndPasswordAsync("t@st.com", "123123");
            var update = await app.Auth.UpdateProfileAsync("disp", "123123");
            var userNode = app.Database.Child("users").Child(app.Auth.User.LocalId);
            //userNode.Child("propCollection").SetStream(props1).Start();
            //userNode.Child("propCollection").SetStream(props2).Start();
            //userNode.Child("objCollection").SetStream(props31).Start();
            //userNode.Child("objCollection").SetStream(props32).Start();
            //userNode.Child("objCollection").SetStream(props33).Start();

            //await Task.Delay(2000);

            var ss111 = userNode.Child("propCollection").GetStreamAsProperty(props1.Key);
            //var ss11 = userNode.Child("objCollection").GetAsObject(props31.Key);
            //var ss1 = userNode.GetAsPropertyCollection("propCollection");
            //var ss2 = userNode.GetAsObjectCollection("objCollection");

            ss111.RealtimeModel.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("s111 blob: " + ss111.RealtimeModel.Blob);
            };
            //ss11.PropertyChanged += (s, e) =>
            //{

            //};
            //ss1.CollectionChanged += (s, e) =>
            //{

            //};
            //ss2.CollectionChanged += (s, e) =>
            //{

            //};

            ss111.Start();

            Console.WriteLine("FIN");

            while (true)
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line)) ss111.RealtimeModel.ModifyData(null);
                else ss111.RealtimeModel.ModifyData(DataTypeDecoder.GetDecoder<string>().Encode(line));
            }
        }
    }
}
