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
using RestfulFirebase.Common.Converters;

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
                Modified = SmallDateTime.UtcNow
            };
        }

        public static TestStorable Create(string key)
        {
            return new TestStorable(CreateFromKey(key))
            {
                Modified = SmallDateTime.UtcNow
            };
        }

        public static TestStorable Create(string key, DateTime created, SmallDateTime modified)
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
            var asdasdas1 = DataTypeConverter.GetConverter<IEnumerable<string>>().Encode(new List<string>(){ "one", "two", "three" });
            var asdasdas2 = DataTypeConverter.GetConverter<IEnumerable<string>>().Decode(asdasdas1);

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

            props1.Modified = SmallDateTime.UtcNow;
            props2.Modified = SmallDateTime.UtcNow;

            int x11 = 0;

            var signInResult = await app.Auth.SignInWithEmailAndPasswordAsync("t@st.com", "123123");
            var update = await app.Auth.UpdateProfileAsync("disp", "123123");
            var userNode = app.Database.Child("users").Child(app.Auth.User.LocalId);

            //await Task.Delay(2000);

            var ss111 = userNode.Child("propCollection").AsRealtimeProperty(props1.Key);
            //var ss11 = userNode.Child("objCollection").GetAsObject(props31.Key);
            //var ss1 = userNode.GetAsPropertyCollection("propCollection");
            //var ss2 = userNode.GetAsObjectCollection("objCollection");
            var asdasd = ss111.RealtimeModel.ParseModel<string>();

            asdasd.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Data: " + asdasd.Value);
            };

            ss111.Start();

            Console.WriteLine("FIN");

            while (true)
            {
                string line = Console.ReadLine();
                asdasd.Value = string.IsNullOrEmpty(line) ? null : line;
            }
        }
    }
}
