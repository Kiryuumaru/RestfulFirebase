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

        public TestStorable(string key)
            : base(key)
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
                LocalDatabase = new DatastoreBlob(false)
            });

            var signInResult = await app.Auth.SignInWithEmailAndPasswordAsync("t@st.com", "123123");
            var update = await app.Auth.UpdateProfileAsync("disp", "123123");
            userNode = app.Database.Child("users").Child(app.Auth.User.LocalId);

            Console.WriteLine("FIN");
            //TestObservableObject();
            //TestPropertyPut();
            //TestPropertySub();
            //TestObjectPut();
            TestObjectSub();
            //TestPropertyGroupPut();
            //TestPropertyGroupSub();
            //TestObjectGroupPut();
            //TestObjectGroupSub();
        }

        public static void TestObservableObject()
        {
            var props = new FirebaseObject<string>("test");
            props.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Data: " + props.Blob + " Prop: " + e.PropertyName);
            };
            props.Value = "numba22";
            while (true)
            {
                string line = Console.ReadLine();
                props.Value = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestPropertyPut()
        {
            var props = new FirebaseObject<string>("test");
            props.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Data: " + props.Value + " Prop: " + e.PropertyName);
            };
            props.Value = "numba22";
            userNode.Child("testing").Child("mock").AsRealtime(props).Start();
            while (true)
            {
                string line = Console.ReadLine();
                props.Value = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestPropertySub()
        {
            var sub = userNode.Child("testing").Child("mock").AsRealtime<FirebaseObject<string>>("test");
            var props = sub.Model;
            props.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Data: " + props.Value + " Prop: " + e.PropertyName);
            };
            sub.Start();
            while (true)
            {
                string line = Console.ReadLine();
                props.Value = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestObjectPut()
        {
            var obj = new TestStorable("mock");
            obj.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Prop: " + e.PropertyName);
            };
            userNode.Child("testing").AsRealtime(obj).Start();
            while (true)
            {
                string line = Console.ReadLine();
                obj.Test = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestObjectSub()
        {
            var sub = userNode.Child("testing").AsRealtime<TestStorable>("mock");
            var obj = sub.Model;
            obj.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Prop: " + e.PropertyName);
            };
            sub.Start();
            while (true)
            {
                string line = Console.ReadLine();
                obj.Test = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        //public static void TestPropertyGroupPut()
        //{
        //    var props = FirebasePropertyGroup.CreateFromKey("mock");
        //    props.CollectionChanged += (s, e) =>
        //    {
        //        Console.WriteLine("Count: " + props.Count);
        //    };
        //    userNode.Child("testing").AsRealtimePropertyGroup(props).Start();
        //    while (true)
        //    {
        //        string line = Console.ReadLine();
        //        var prop = FirebaseProperty.CreateFromKey<string>(RestfulFirebase.Common.Helpers.GenerateSafeUID());
        //        prop.Value = line;
        //        prop.PropertyChanged += (s, e) =>
        //        {
        //            Console.WriteLine("Prop: " + e.PropertyName + " Data: " + prop.Blob);
        //        };
        //        props.Add(prop);
        //    }
        //}

        //public static void TestPropertyGroupSub()
        //{
        //    var props = userNode.Child("testing").AsRealtimePropertyGroup("mock");
        //    props.Model.CollectionChanged += (s, e) =>
        //    {
        //        Console.WriteLine("Count: " + props.Model.Count);
        //    };
        //    props.Start();
        //    while (true)
        //    {
        //        string line = Console.ReadLine();
        //        var prop = FirebaseProperty.CreateFromKey<string>(RestfulFirebase.Common.Helpers.GenerateSafeUID());
        //        prop.Value = line;
        //        prop.PropertyChanged += (s, e) =>
        //        {
        //            Console.WriteLine("Prop: " + e.PropertyName + " Data: " + prop.Blob);
        //        };
        //        props.Model.Add(prop);
        //    }
        //}

        //public static void TestObjectGroupPut()
        //{
        //    var objs = FirebaseObjectGroup.CreateFromKey("testing");
        //    objs.CollectionChanged += (s, e) =>
        //    {
        //        Console.WriteLine("Count: " + objs.Count);
        //    };
        //    userNode.AsRealtimeObjectGroup(objs).Start();
        //    while (true)
        //    {
        //        string line = Console.ReadLine();
        //        var obj = TestStorable.Create();
        //        obj.Test = line;
        //        obj.PropertyChanged += (s, e) =>
        //        {
        //            Console.WriteLine("Prop: " + e.PropertyName);
        //        };
        //        objs.Add(obj);
        //    }
        //}

        //public static void TestObjectGroupSub()
        //{
        //    var objs = userNode.AsRealtimeObjectGroup("testing");
        //    objs.Model.CollectionChanged += (s, e) =>
        //    {
        //        Console.WriteLine("Count: " + objs.Model.Count);
        //    };
        //    objs.Start();
        //    while (true)
        //    {
        //        string line = Console.ReadLine();
        //        var obj = TestStorable.Create();
        //        obj.Test = line;
        //        obj.PropertyChanged += (s, e) =>
        //        {
        //            Console.WriteLine("Prop: " + e.PropertyName);
        //        };
        //        objs.Model.Add(obj);
        //    }
        //}
    }
}
