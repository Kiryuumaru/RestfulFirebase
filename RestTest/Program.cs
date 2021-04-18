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
using RestfulFirebase;
using RestfulFirebase.Database.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using RestfulFirebase.Common.Converters;

namespace RestTest
{
    public class TestObs : RestfulFirebase.Common.Models.ObservableObjects
    {
        public string TestProp1
        {
            get => GetProperty<string>("test1");
            set => SetProperty(value, "test1");
        }
    }

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

        public IEnumerable<TimeSpan> Premiums
        {
            get => GetPersistableProperty<IEnumerable<TimeSpan>>("premiums", new List<TimeSpan>());
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
                StorageBucket = "restfulplayground.appspot.com",
                LocalDatabase = new Datastore()
            });

            //var signInResult = await app.Auth.SignInWithEmailAndPasswordAsync("t@st.com", "123123");
            //var update = await app.Auth.UpdateProfileAsync("disp", "123123");
            //userNode = app.Database.Child("users").Child(app.Auth.User.LocalId);

            Console.WriteLine("FIN");
            TestObservable();
            //TestPropertyPut();
            //TestPropertySub();
            //TestObjectPut();
            //TestObjectSub();
            //TestPropertyGroupPut();
            //TestPropertyGroupSub();
            //TestObjectGroupPut();
            //TestObjectGroupSub();
        }

        public static void TestObservable()
        {
            var obs = new TestObs();
            obs.PropertyChanged += (s, e) =>
            {

            };
            obs.TestProp1 = "testtt";
            obs.TestProp1 = "testtt";
            obs.TestProp1 = null;
            obs.TestProp1 = null;
            var ss = "svsvsvs";
        }

        public static void TestPropertyPut()
        {
            var props = FirebaseProperty.CreateFromKey<string>("test");
            props.Value = "numba22";
            props.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Data: " + props.Value + " Prop: " + e.PropertyName);
            };
            userNode.Child("testing").Child("mock").AsRealtimeProperty(props).Start();
            while (true)
            {
                string line = Console.ReadLine();
                props.Value = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestPropertySub()
        {
            var props = userNode.Child("testing").Child("mock").AsRealtimeProperty<string>("test");
            props.Model.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Data: " + props.Model.Value + " Prop: " + e.PropertyName);
            };
            props.Start();
            while (true)
            {
                string line = Console.ReadLine();
                props.Model.Value = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestObjectPut()
        {
            var obj = TestStorable.Create("mock");
            obj.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Prop: " + e.PropertyName);
            };
            userNode.Child("testing").AsRealtimeObject<TestStorable>(obj).Start();
            while (true)
            {
                string line = Console.ReadLine();
                obj.Test = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestObjectSub()
        {
            var obj = userNode.Child("testing").AsRealtimeObject<TestStorable>("mock");
            obj.Model.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Prop: " + e.PropertyName);
            };
            obj.Start();
            while (true)
            {
                string line = Console.ReadLine();
                obj.Model.Test = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestPropertyGroupPut()
        {
            var props = FirebasePropertyGroup.CreateFromKey("mock");
            props.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("Count: " + props.Count);
            };
            userNode.Child("testing").AsRealtimePropertyGroup(props).Start();
            while (true)
            {
                string line = Console.ReadLine();
                var prop = FirebaseProperty.CreateFromKey<string>(RestfulFirebase.Common.Helpers.GenerateSafeUID());
                prop.Value = line;
                prop.PropertyChanged += (s, e) =>
                {
                    Console.WriteLine("Prop: " + e.PropertyName + " Data: " + prop.Blob);
                };
                props.Add(prop);
            }
        }

        public static void TestPropertyGroupSub()
        {
            var props = userNode.Child("testing").AsRealtimePropertyGroup("mock");
            props.Model.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("Count: " + props.Model.Count);
            };
            props.Start();
            while (true)
            {
                string line = Console.ReadLine();
                var prop = FirebaseProperty.CreateFromKey<string>(RestfulFirebase.Common.Helpers.GenerateSafeUID());
                prop.Value = line;
                prop.PropertyChanged += (s, e) =>
                {
                    Console.WriteLine("Prop: " + e.PropertyName + " Data: " + prop.Blob);
                };
                props.Model.Add(prop);
            }
        }

        public static void TestObjectGroupPut()
        {
            var objs = FirebaseObjectGroup.CreateFromKey("testing");
            objs.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("Count: " + objs.Count);
            };
            userNode.AsRealtimeObjectGroup(objs).Start();
            while (true)
            {
                string line = Console.ReadLine();
                var obj = TestStorable.Create();
                obj.Test = line;
                obj.PropertyChanged += (s, e) =>
                {
                    Console.WriteLine("Prop: " + e.PropertyName);
                };
                objs.Add(obj);
            }
        }

        public static void TestObjectGroupSub()
        {
            var objs = userNode.AsRealtimeObjectGroup("testing");
            objs.Model.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("Count: " + objs.Model.Count);
            };
            objs.Start();
            while (true)
            {
                string line = Console.ReadLine();
                var obj = TestStorable.Create();
                obj.Test = line;
                obj.PropertyChanged += (s, e) =>
                {
                    Console.WriteLine("Prop: " + e.PropertyName);
                };
                objs.Model.Add(obj);
            }
        }
    }
}
