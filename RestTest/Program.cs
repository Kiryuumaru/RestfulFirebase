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
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Common;
using System.Threading;

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
            TestObjectPut();
            //TestObjectSub();
            //TestPropertyGroupPut();
            //TestPropertyGroupSub();
            //TestObjectGroupPut();
            //TestObjectGroupSub();
            //ExperimentList();


        }

        public static void TestPropertyPut()
        {
            var props = new FirebaseObject<string>("test");
            props.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Data: " + props.Value + " Prop: " + e.PropertyName);
            };
            props.Value = "numba22";
            userNode.Child("testing").Child("mock").PutAsRealtime(props).Start();
            while (true)
            {
                string line = Console.ReadLine();
                props.Value = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestPropertySub()
        {
            var sub = userNode.Child("testing").Child("mock").SubAsRealtime<FirebaseObject<string>>("test");
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
            userNode.Child("testing").PutAsRealtime(obj).Start();
            while (true)
            {
                string line = Console.ReadLine();
                obj.Test = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestObjectSub()
        {
            var sub = userNode.Child("testing").SubAsRealtime<TestStorable>("mock");
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

        public class Pager : FirebaseObject
        {
            private const string PagesKey = "pages";
            private const int KeysPerPageCount = 10;

            public string Testu
            {
                get => GetPersistableProperty<string>("1", "");
                set => SetPersistableProperty(value, "1");
            }

            public int PageCount
            {
                get => GetPersistableProperty<int>(PagesKey, 0);
                set => SetPersistableProperty(value, PagesKey);
            }

            public List<string> Keys
            {
                get
                {
                    var count = PageCount;
                    var keys = new List<string>();
                    for (int i = 0; i < count; i++)
                    {
                        var data = GetPersistableProperty<string>(PagesKey + i.ToString());
                        var deserialized = Helpers.DeserializeString(data);
                        if (deserialized == null) continue;
                        keys.AddRange(deserialized);
                    }
                    return keys;
                }
                set
                {
                    if (value == null)
                    {
                        var count = PageCount;
                        var keys = new List<string>();
                        SetPersistableProperty(0, PagesKey);
                        for (int i = 0; i < count; i++)
                        {
                            DeleteProperty(PagesKey + i.ToString());
                        }
                    }
                    else
                    {
                        var iterations = (value.Count + (KeysPerPageCount - 1)) / KeysPerPageCount;
                        var index = 0;
                        var count = PageCount;
                        var keys = new List<string>();
                        for (int i = 0; i < iterations; i++)
                        {
                            var pageKeys = new List<string>();
                            for (int j = 0; j < KeysPerPageCount; j++)
                            {
                                if (value.Count <= index) break;
                                pageKeys.Add(value[index]);
                                index++;
                            }
                            var page = Helpers.SerializeString(pageKeys.ToArray());
                            SetPersistableProperty(page, (PagesKey + i.ToString()));
                        }
                        SetPersistableProperty(iterations, PagesKey);
                    }
                }
            }

            public Pager(IAttributed attributed)
                : base(attributed)
            {

            }

            public Pager(string key)
                : base(key)
            {

            }
        }

        public static void ExperimentList()
        {
            var obj = new Pager("testPager");
            obj.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Prop: " + e.PropertyName);
            };
            userNode.Child("testing").PutAsRealtime(obj).Start();
            var index = 0;
            while (true)
            {
                //string line = Console.ReadLine();
                if (index == 10) index = 0;
                string line = (index++).ToString();
                if (string.IsNullOrEmpty(line))
                {
                    obj.Keys = null;
                }
                else
                {
                    var keys = obj.Keys;
                    //keys.Insert(0, line);
                    keys.Add(line);
                    obj.Keys = keys;
                }
                Thread.Sleep(100);
            }
        }
    }
}
