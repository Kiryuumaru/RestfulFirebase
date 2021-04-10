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

        public List<TimeSpan> Premiums
        {
            get => GetPersistableProperty<List<TimeSpan>>("premiums", new List<TimeSpan>());
            set => SetPersistableProperty(value, "premiums");
        }

        public string Test
        {
            get => GetPersistableProperty<string>("test");
            set => SetPersistableProperty(value, "test");
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
        private static RestfulFirebaseApp app;
        private static ChildQuery userNode;
        public static void Main(string[] args)
        {
            Run().GetAwaiter().GetResult();
        }

        public static async Task Run()
        {
            app = new RestfulFirebaseApp(new FirebaseConfig()
            {
                ApiKey = "AIzaSyBZfLYmm5SyxmBk0lzBh0_AcDILjOLUD9o",
                DatabaseURL = "https://restfulplayground-default-rtdb.firebaseio.com/",
                StorageBucket = "restfulplayground.appspot.com"
            });

            //var signInResult = await app.Auth.SignInWithEmailAndPasswordAsync("t@st.com", "123123");
            //var update = await app.Auth.UpdateProfileAsync("disp", "123123");
            //userNode = app.Database.Child("users").Child(app.Auth.User.LocalId);

            Console.WriteLine("FIN");
            ObjectProperty();
        }

        public static void TestProperty()
        {
            var props = FirebaseProperty.CreateFromKeyAndValue("keyS", "numba22");
            userNode.Child("propCollection").AsRealtimeProperty(props).Start();
            props.PropertyChanged += (s, e) => { Console.WriteLine("Data: " + props.Value + " Prop: " + e.PropertyName); };
            while (true)
            {
                string line = Console.ReadLine();
                props.Value = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void ObjectProperty()
        {
            var obj = TestStorable.Create();
            userNode.Child("objCollection").AsRealtimeObject<TestStorable>(obj).Start();
            obj.PropertyChanged += (s, e) => { Console.WriteLine("Prop: " + e.PropertyName); };
            while (true)
            {
                string line = Console.ReadLine();
                obj.Test = string.IsNullOrEmpty(line) ? null : line;
            }
        }
    }
}
