using RestfulFirebase.Auth;
using RestfulFirebase.Database;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Offline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestfulFirebase;
using RestfulFirebase.Database.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using ObservableHelpers;
using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Utilities;
using System.ComponentModel;
using System.Windows.Threading;
using RestfulFirebase.Local;
using System.Collections;
using System.Collections.Concurrent;

namespace RestTest
{
    public class TestStorable : FirebaseObject
    {
        #region Properties

        public bool IsOk
        {
            get => GetFirebasePropertyWithKey<bool>("isOk");
            set => SetFirebasePropertyWithKey(value, "isOk");
        }

        public TimeSpan Premium
        {
            get => GetFirebasePropertyWithKey<TimeSpan>("premium");
            set => SetFirebasePropertyWithKey(value, "premium");
        }

        public List<TimeSpan> Premiums
        {
            get => GetFirebasePropertyWithKey("premiums", new List<TimeSpan>());
            set => SetFirebasePropertyWithKey(value, "premiums");
        }

        public TimeSpan[] Premiums1
        {
            get => GetFirebasePropertyWithKey("premiums1", new TimeSpan[0]);
            set => SetFirebasePropertyWithKey(value, "premiums1");
        }

        public decimal Num1
        {
            get => GetFirebasePropertyWithKey<decimal>("num1");
            set => SetFirebasePropertyWithKey(value, "num1");
        }

        public decimal Num2
        {
            get => GetFirebasePropertyWithKey<decimal>("num2");
            set => SetFirebasePropertyWithKey(value, "num2");
        }

        public decimal Num3
        {
            get => GetFirebasePropertyWithKey<decimal>("num3");
            set => SetFirebasePropertyWithKey(value, "num3");
        }

        public string Test
        {
            get => GetFirebasePropertyWithKey<string>("test");
            set => SetFirebasePropertyWithKey(value, "test");
        }

        public string Dummy
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        #endregion

        #region Methods

        public TestStorable()
        {
            Dummy = "test";
            InitializeProperties();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }
    }
    public class TestStorable2 : FirebaseObject
    {
        #region Properties

        public List<DateTime> List
        {
            get => GetFirebasePropertyWithKey("list", new List<DateTime>());
            set => SetFirebasePropertyWithKey(value, "list");
        }

        public DateTime[] Array
        {
            get => GetFirebasePropertyWithKey("array", new DateTime[0]);
            set => SetFirebasePropertyWithKey(value, "array");
        }

        public Queue Queue
        {
            get => GetFirebasePropertyWithKey("queue", new Queue());
            set => SetFirebasePropertyWithKey(value, "queue");
        }

        public Queue<DateTime> QueueTyped
        {
            get => GetFirebasePropertyWithKey("queueTyped", new Queue<DateTime>());
            set => SetFirebasePropertyWithKey(value, "queueTyped");
        }

        public ConcurrentDictionary<string, DateTime> ConcurrentDictionary
        {
            get => GetFirebasePropertyWithKey("concurrentDictionary", new ConcurrentDictionary<string, DateTime>());
            set => SetFirebasePropertyWithKey(value, "concurrentDictionary");
        }

        #endregion
    }
    public class TestStorable3 : FirebaseObject
    {
        #region Properties

        public DateTime? NullableType
        {
            get => GetFirebasePropertyWithKey<DateTime?>("nullable");
            set => SetFirebasePropertyWithKey(value, "nullable");
        }

        #endregion
    }

    public class CascadeStorable : FirebaseObject
    {
        #region Properties

        public TestStorable Storable1
        {
            get => GetFirebasePropertyWithKey<TestStorable>("storable1");
            set => SetFirebasePropertyWithKey(value, "storable1");
        }

        public TestStorable Storable2
        {
            get => GetFirebasePropertyWithKey<TestStorable>("storable2");
            set => SetFirebasePropertyWithKey(value, "storable2");
        }

        public FirebaseDictionary<FirebaseProperty> PropertyDictionary
        {
            get => GetFirebasePropertyWithKey<FirebaseDictionary<FirebaseProperty>>("props");
            set => SetFirebasePropertyWithKey(value, "storable2");
        }

        public FirebaseDictionary<TestStorable> ObjectDictionary
        {
            get => GetFirebasePropertyWithKey<FirebaseDictionary<TestStorable>>("objs");
            set => SetFirebasePropertyWithKey(value, "storable2");
        }

        //public FirebaseDictionary<CascadeStorable> CascadeDictionary
        //{
        //    get => GetFirebasePropertyWithKey<FirebaseDictionary<CascadeStorable>>("cascade");
        //    set => SetFirebasePropertyWithKey(value, "cascade");
        //}

        public string Test
        {
            get => GetFirebasePropertyWithKey<string>("test");
            set => SetFirebasePropertyWithKey(value, "test");
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
            await Task.Delay(2000);

            // Attach your config
            app = new RestfulFirebaseApp(Config.YourConfig());

            if (!app.Auth.IsAuthenticated)
            {
                await app.Auth.SignInWithEmailAndPassword("t@st.com", "123123");
                Console.WriteLine("NEW AUTH");
            }
            else
            {
                Console.WriteLine("OLD AUTH");
            }
            userNode = app.Database.Child("users").Child(app.Auth.Session.LocalId);

            Console.WriteLine("FIN");

            //TestSerializers();
            //TestObservableObject();
            //TestRealtimeWire();
            //TestRealtimeWire2();
            //await TestModelAttachDetach();
            //TestPropertyPut();
            //TestPropertySub();
            //TestPropertySub2();
            //TestObjectPut();
            //TestObjectSub();
            //TestObjectDefaults();
            //TestObjectNullable();
            //TestPropertyDictionaryPut();
            //TestPropertyDictionarySub();
            //TestPropertyDictionarySub2();
            //TestPropertyDictionarySub3();
            TestPropertyDictionarySub4();
            //TestObjectDictionaryPut();
            //TestObjectDictionarySub();
            //TestObjectDictionarySub2();
            //TestObjectDictionarySub3();
            //ExperimentList();
            //await TestDef();
            //await TestRoutineWrite();
            //TestCascadeObjectPut();
            //TestCascadeObjectMassPut();
            //TestCascadeObjectSub();
            //await TestCascadeObjectSetNull();

            while (true)
            {
            }
        }

        public static void TestSerializers()
        {
            var dict = new FirebaseDictionary<FirebaseProperty>();
            dict.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("Count: " + dict.Keys.Count);
            };
            while (true)
            {
                string line = Console.ReadLine();
                if (line == "end")
                {
                    break;
                }
                var prop = new FirebaseProperty();
                prop.SetValue(line);
                dict.Add(UIDFactory.GenerateUID(5), prop);
            }

            var serialized = dict.GenerateSerializedValue();
            var dictFromS = new FirebaseDictionary<FirebaseProperty>();
            dictFromS.LoadFromSerializedValue(serialized);

            var encrypted = dict.GenerateSerializedValue(1, 4, 2, 3);
            var dictFromE = new FirebaseDictionary<FirebaseProperty>();
            dictFromS.LoadFromSerializedValue(encrypted, 1, 4, 2, 3);

            while (true)
            {
                Console.ReadLine();
            }
        }

        public static void TestRealtimeWire()
        {
            var wire = app.Database.Child("public").AsRealtimeWire();
            wire.DataChanges += (s, e) =>
            {
                Console.WriteLine("Sync: " + wire.SyncedDataCount + "/" + wire.TotalDataCount + " Path: " + e.Path);
            };
            wire.Error += (s, e) =>
            {
                Console.WriteLine("OnError: " + e.Uri + " Message: " + e.Exception.Message);
            };
            wire.Start();
            while (true)
            {
                string line = Console.ReadLine();
                line = line.TrimStart(' ');
                if (line?.Contains(' ') ?? false)
                {
                    var path = line.Substring(0, line.IndexOf(' '));
                    if (line.EndsWith(" "))
                    {
                        var data = line.Substring(line.IndexOf(' ') + 1);
                        var separated = UrlUtilities.Separate(path);
                        RealtimeInstance subWire = wire;
                        for (int i = 0; i < separated.Length; i++)
                        {
                            subWire = subWire.Child(separated[i]);
                        }
                        subWire.SetBlob(null);
                    }
                    else
                    {
                        var data = line.Substring(line.IndexOf(' ') + 1);
                        var separated = UrlUtilities.Separate(path);
                        RealtimeInstance subWire = wire;
                        for (int i = 0; i < separated.Length; i++)
                        {
                            subWire = subWire.Child(separated[i]);
                        }
                        subWire.SetBlob(data);
                    }
                }
                else if (line == "view")
                {
                    var db = ((DatastoreBlob)app.Config.LocalDatabase).GetDB();
                    foreach (var pair in db)
                    {
                        Console.WriteLine("KEY: " + pair.Key + " VAL: " + pair.Value);
                    }
                }
            }
        }

        public static void TestRealtimeWire2()
        {
            var wire = app.Database.Child("public").AsRealtimeWire();
            wire.DataChanges += (s, e) =>
            {
                Console.WriteLine("Main Sync: " + wire.SyncedDataCount + "/" + wire.TotalDataCount + " Path: " + e.Path);
            };
            wire.Error += (s, e) =>
            {
                Console.WriteLine("Main OnError: " + e.Uri + " Message: " + e.Exception.Message);
            };
            var subWire1 = wire.Child("sub1");
            subWire1.DataChanges += (s, e) =>
            {
                Console.WriteLine("Sub1 Sync: " + wire.SyncedDataCount + "/" + wire.TotalDataCount + " Path: " + e.Path);
            };
            subWire1.Error += (s, e) =>
            {
                Console.WriteLine("Sub1 OnError: " + e.Uri + " Message: " + e.Exception.Message);
            };
            var subWire2 = wire.Child("sub2");
            subWire2.DataChanges += (s, e) =>
            {
                Console.WriteLine("Sub2 Sync: " + wire.SyncedDataCount + "/" + wire.TotalDataCount + " Path: " + e.Path);
            };
            subWire2.Error += (s, e) =>
            {
                Console.WriteLine("Sub2 OnError: " + e.Uri + " Message: " + e.Exception.Message);
            };
            wire.Start();
            while (true)
            {
                string line = Console.ReadLine();
                line = line.TrimStart(' ');
                if (line.StartsWith("1 "))
                {
                    line = line.Substring(2).TrimStart(' ');
                    if (line?.Contains(' ') ?? false)
                    {
                        var path = line.Substring(0, line.IndexOf(' '));
                        if (line.EndsWith(" "))
                        {
                            var data = line.Substring(line.IndexOf(' ') + 1);
                            var separated = UrlUtilities.Separate(path);
                            RealtimeInstance subWire = subWire1;
                            for (int i = 0; i < separated.Length; i++)
                            {
                                subWire = subWire.Child(separated[i]);
                            }
                            subWire.SetBlob(null);
                        }
                        else
                        {
                            var data = line.Substring(line.IndexOf(' ') + 1);
                            var separated = UrlUtilities.Separate(path);
                            RealtimeInstance subWire = subWire1;
                            for (int i = 0; i < separated.Length; i++)
                            {
                                subWire = subWire.Child(separated[i]);
                            }
                            subWire.SetBlob(data);
                        }
                    }
                }
                else if (line.StartsWith("2 "))
                {
                    line = line.Substring(2).TrimStart(' ');
                    if (line?.Contains(' ') ?? false)
                    {
                        var path = line.Substring(0, line.IndexOf(' '));
                        if (line.EndsWith(" "))
                        {
                            var data = line.Substring(line.IndexOf(' ') + 1);
                            var separated = UrlUtilities.Separate(path);
                            RealtimeInstance subWire = subWire2;
                            for (int i = 0; i < separated.Length; i++)
                            {
                                subWire = subWire.Child(separated[i]);
                            }
                            subWire.SetBlob(null);
                        }
                        else
                        {
                            var data = line.Substring(line.IndexOf(' ') + 1);
                            var separated = UrlUtilities.Separate(path);
                            RealtimeInstance subWire = subWire2;
                            for (int i = 0; i < separated.Length; i++)
                            {
                                subWire = subWire.Child(separated[i]);
                            }
                            subWire.SetBlob(data);
                        }
                    }
                }
                else if (line == "view")
                {
                    var db = ((DatastoreBlob)app.Config.LocalDatabase).GetDB();
                    foreach (var pair in db)
                    {
                        Console.WriteLine("KEY: " + pair.Key + " VAL: " + pair.Value);
                    }
                }
            }
        }

        public static async Task TestModelAttachDetach()
        {
            var objs = new FirebaseDictionary<TestStorable>();
            objs.RealtimeAttached += (s, e) =>
            {
                Console.WriteLine("Main: Observable model ATTACHED to wire");
            };
            objs.RealtimeDetached += (s, e) =>
            {
                Console.WriteLine("Main: Observable model DETACHED to wire");
            };

            for (int i = 0; i < 2; i++)
            {
                var obj = new TestStorable();
                var iteration = i.ToString();
                obj.RealtimeAttached += (s, e) =>
                {
                    Console.WriteLine(iteration + ": Observable model ATTACHED to wire");
                };
                obj.RealtimeDetached += (s, e) =>
                {
                    Console.WriteLine(iteration + ": Observable model DETACHED to wire");
                };
                obj.IsOk = true;
                obj.Premium = TimeSpan.FromSeconds(60);
                obj.Premiums = new List<TimeSpan>() { TimeSpan.FromSeconds(30) };
                obj.Test = iteration;
                objs.Add(iteration, obj);
            }

            var wire = userNode.Child("testing").Child("mock").Child("test").AsRealtimeWire();

            wire.Start();

            wire.Child("wire_child1").Child("wire_child2").PutModel(objs);

            await wire.WaitForSynced();

            wire.Dispose();
            objs.Clear();
        }

        public static void TestPropertyPut()
        {
            var props = new FirebaseProperty<string>();
            props.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Data: " + props.Value + " Prop: " + e.PropertyName);
            };
            props.Value = "numba22";
            var wire = userNode.Child("testing").Child("mock").Child("test").AsRealtimeWire();
            wire.Start();
            wire.PutModel(props);
            while (true)
            {
                string line = Console.ReadLine();
                props.Value = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestPropertySub()
        {
            var props = new FirebaseProperty<string>();
            props.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Data: " + props.Value + " Prop: " + e.PropertyName);
            };
            //props.Value = "numba11";
            var wire = userNode.Child("testing").Child("mock").Child("test").AsRealtimeWire();
            wire.Start();
            wire.SubModel(props);
            while (true)
            {
                string line = Console.ReadLine();
                props.Value = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestPropertySub2()
        {
            var props = new FirebaseProperty<string>();
            props.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Data: " + props.Value + " Prop: " + e.PropertyName);
            };
            props.Value = "numba11";
            var wire = userNode.Child("testing").Child("mock").Child("test").AsRealtimeWire();
            wire.Start();
            wire.SubModel(props);

            for (int i = 0; i < 1000; i++)
            {
                props.Value = i.ToString();
            }

            while (true)
            {
                string line = Console.ReadLine();
                props.Value = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestObjectPut()
        {
            var obj = new TestStorable();
            obj.PropertyChanged += (s, e) =>
            {
                var write = "Prop: " + e.PropertyName + ": ";
                switch (e.PropertyName)
                {
                    case nameof(TestStorable.IsOk):
                        write += obj.IsOk.ToString();
                        break;
                    case nameof(TestStorable.Premium):
                        write += obj.Premium.ToString();
                        break;
                    case nameof(TestStorable.Premiums):
                        write += obj.Premiums?.ToString();
                        break;
                    case nameof(TestStorable.Test):
                        write += obj.Test;
                        break;
                }
                Console.WriteLine(write);
            };
            obj.IsOk = true;
            obj.Premium = TimeSpan.FromSeconds(60);
            obj.Premiums = new List<TimeSpan>() { TimeSpan.FromSeconds(30) };
            obj.Test = "testuuuuu";
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.PutModel(obj);

            while (true)
            {
                string line = Console.ReadLine();
                obj.Test = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static void TestObjectSub()
        {
            var obj = new TestStorable();
            obj.PropertyChanged += (s, e) =>
            {
                var write = "Prop: " + e.PropertyName + ": ";
                switch (e.PropertyName)
                {
                    case nameof(TestStorable.IsOk):
                        write += obj.IsOk.ToString();
                        break;
                    case nameof(TestStorable.Premium):
                        write += obj.Premium.ToString();
                        break;
                    case nameof(TestStorable.Premiums):
                        write += obj.Premiums?.ToString();
                        break;
                    case nameof(TestStorable.Test):
                        write += obj.Test;
                        break;
                }
                Console.WriteLine(write);
            };
            //obj.IsOk = true;
            //obj.Premium = TimeSpan.FromSeconds(60);
            //obj.Premiums = new List<TimeSpan>() { TimeSpan.FromSeconds(30) };
            //obj.Test = "testuuuuu";
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.SubModel(obj);
            while (true)
            {
                string line = Console.ReadLine();
                obj.Test = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public async static void TestObjectDefaults()
        {
            var obj = new TestStorable2();
            obj.ImmediatePropertyChanged += (s, e) =>
            {
                var write = "Prop: " + e.PropertyName + ": ";
                switch (e.PropertyName)
                {
                    case nameof(TestStorable2.List):
                        write += obj.List?.ToString();
                        break;
                    case nameof(TestStorable2.Array):
                        write += obj.Array?.ToString();
                        break;
                    case nameof(TestStorable2.Queue):
                        write += obj.Queue?.ToString();
                        break;
                    case nameof(TestStorable2.QueueTyped):
                        write += obj.QueueTyped?.ToString();
                        break;
                    default:
                        break;
                }
                Console.WriteLine(write);
            };
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.SubModel(obj);
            await wire.WaitForFirstStream();

            var newList1 = obj.List;
            newList1.Add(DateTime.UtcNow);
            obj.List = newList1;
            await wire.WaitForFirstStream();
            var newList2 = obj.List;
            newList2.Add(DateTime.UtcNow);
            obj.List = newList2;
            await wire.WaitForFirstStream();

            var newArr1 = obj.Array.ToList();
            newArr1.Add(DateTime.UtcNow);
            obj.Array = newArr1.ToArray();
            await wire.WaitForFirstStream();
            var newArr2 = obj.Array.ToList();
            newArr2.Add(DateTime.UtcNow);
            obj.Array = newArr2.ToArray();
            await wire.WaitForFirstStream();

            var newQueue1 = obj.Queue;
            newQueue1.Enqueue(DateTime.UtcNow);
            obj.Queue = newQueue1;
            await wire.WaitForFirstStream();
            var newQueue2 = obj.Queue;
            newQueue2.Enqueue(DateTime.UtcNow);
            obj.Queue = newQueue2;
            await wire.WaitForFirstStream();

            var newQueueTyped1 = obj.QueueTyped;
            newQueueTyped1.Enqueue(DateTime.UtcNow);
            obj.QueueTyped = newQueueTyped1;
            await wire.WaitForFirstStream();
            var newQueueTyped2 = obj.QueueTyped;
            newQueueTyped2.Enqueue(DateTime.UtcNow);
            obj.QueueTyped = newQueueTyped2;
            await wire.WaitForFirstStream();

            var newDict1 = obj.ConcurrentDictionary;
            newDict1.TryAdd((obj.ConcurrentDictionary.Count + 1).ToString(), DateTime.UtcNow);
            obj.ConcurrentDictionary = newDict1;
            await wire.WaitForFirstStream();
            var newDict2 = obj.ConcurrentDictionary;
            newDict2.TryAdd((obj.ConcurrentDictionary.Count + 1).ToString(), DateTime.UtcNow);
            obj.ConcurrentDictionary = newDict2;
            await wire.WaitForFirstStream();

            while (true)
            {
                string line = Console.ReadLine();
            }
        }

        public async static void TestObjectNullable()
        {
            var obj = new TestStorable3();
            obj.ImmediatePropertyChanged += (s, e) =>
            {
                var write = "Prop: " + e.PropertyName + ": ";
                switch (e.PropertyName)
                {
                    case nameof(TestStorable3.NullableType):
                        write += obj.NullableType?.ToString();
                        break;
                    default:
                        break;
                }
                Console.WriteLine(write);
            };
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.SubModel(obj);
            await wire.WaitForFirstStream();

            obj.NullableType = DateTime.UtcNow;
            await wire.WaitForFirstStream();

            while (true)
            {
                string line = Console.ReadLine();
            }
        }

        public static void TestPropertyDictionaryPut()
        {
            var dict = new FirebaseDictionary<FirebaseProperty>(key => new FirebaseProperty());
            dict.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("Count: " + dict.Keys.Count);
            };
            var prop1 = new FirebaseProperty();
            prop1.SetValue("111");
            dict.Add("aaa", prop1);
            //var prop2 = new FirebaseProperty();
            //prop2.SetValue("222");
            //dict.Add("bbb", prop2);
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.PutModel(dict);
            while (true)
            {
                string line = Console.ReadLine();
                var prop = new FirebaseProperty();
                prop.SetValue(line);
                dict.Add(UIDFactory.GenerateSafeUID(), prop);
            }
        }

        public static void TestPropertyDictionarySub()
        {
            var dict = new FirebaseDictionary<FirebaseProperty>(key => new FirebaseProperty());
            dict.CollectionChanged += (s, e) =>
            {
                //Console.WriteLine("Count: " + dict.Keys.Count);
            };
            var prop1 = new FirebaseProperty();
            prop1.SetValue("333");
            dict.Add("ccc", prop1);
            var prop2 = new FirebaseProperty();
            prop2.SetValue("444");
            dict.Add("ddd", prop2);
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.SubModel(dict);
            wire.DataChanges += (s, e) =>
            {
                Console.WriteLine("Total: " + wire.TotalDataCount + " Sync: " + wire.SyncedDataCount);
            };
            while (true)
            {
                string line = Console.ReadLine();
                var prop = new FirebaseProperty();
                prop.SetValue(line);
                dict.Add(UIDFactory.GenerateSafeUID(), prop);
            }
        }

        public static void TestPropertyDictionarySub2()
        {
            var dict = new FirebaseDictionary<FirebaseProperty>(key => new FirebaseProperty());
            dict.CollectionChanged += (s, e) =>
            {
                //Console.WriteLine("Count: " + dict.Keys.Count);
            };

            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.SubModel(dict);
            wire.DataEvaluated += (s, e) =>
            {
                Console.WriteLine("Writes: " + app.Database.PendingWrites);
                Console.WriteLine("Total: " + e.TotalDataCount + " Sync: " + e.SyncedDataCount);
                Console.WriteLine("Count: " + dict.Keys.Count);
            };

            Console.WriteLine("START");
            Console.ReadLine();

            for (int i = 0; i < 2000; i++)
            {
                var prop = new FirebaseProperty();
                prop.SetValue(i.ToString());
                dict.Add(UIDFactory.GenerateSafeUID(), prop);
            }

            while (true)
            {
                string line = Console.ReadLine();
                var prop = new FirebaseProperty();
                prop.SetValue(line);
                dict.Add(UIDFactory.GenerateSafeUID(), prop);
            }
        }

        public static void TestPropertyDictionarySub3()
        {
            var dict = new FirebaseDictionary<FirebaseProperty>(key => new FirebaseProperty());
            dict.CollectionChanged += (s, e) =>
            {
                //Console.WriteLine("Count: " + dict.Keys.Count);
            };
            
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.SubModel(dict);
            wire.DataChanges += (s, e) =>
            {
                Console.WriteLine("Total: " + wire.TotalDataCount + " Sync: " + wire.SyncedDataCount);
            };

            string lin11e = Console.ReadLine();

            for (int i = 0; i < 10; i++)
            {
                var prop = new FirebaseProperty();
                prop.SetValue(i.ToString());
                dict.Add(i.ToString(), prop);
            }

            while (true)
            {
                string line = Console.ReadLine();
                dict.Remove(line);
            }
        }

        public static void TestPropertyDictionarySub4()
        {
            var dict = new FirebaseDictionary<string>();
            dict.CollectionChanged += (s, e) =>
            {
                //Console.WriteLine("Count: " + dict.Keys.Count);
            };

            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.SubModel(dict);
            wire.DataEvaluated += (s, e) =>
            {
                Console.WriteLine("Writes: " + app.Database.PendingWrites);
                Console.WriteLine("Total: " + e.TotalDataCount + " Sync: " + e.SyncedDataCount);
                Console.WriteLine("Count: " + dict.Keys.Count);
            };

            Console.WriteLine("START");
            Console.ReadLine();

            for (int i = 0; i < 100; i++)
            {
                dict.Add(UIDFactory.GenerateSafeUID(), i.ToString());
            }

            while (true)
            {
                string line = Console.ReadLine();
                dict.Add(UIDFactory.GenerateSafeUID(), line);
            }
        }

        public static void TestObjectDictionaryPut()
        {
            var dict = new FirebaseDictionary<FirebaseObject>(key => new FirebaseObject());
            dict.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("Count: " + dict.Keys.Count);
            };
            var obj1 = new TestStorable();
            dict.Add("aaa", obj1);
            var obj2 = new TestStorable();
            obj2.IsOk = true;
            obj2.Premium = TimeSpan.FromSeconds(60);
            obj2.Premiums = new List<TimeSpan>() { TimeSpan.FromSeconds(30) };
            obj2.Test = "testuuuuu";
            dict.Add("bbb", obj2);
            var obj3 = new TestStorable();
            obj2.IsOk = false;
            obj2.Premium = TimeSpan.FromSeconds(3600);
            obj2.Premiums = new List<TimeSpan>() { TimeSpan.FromSeconds(7200) };
            obj2.Test = "CLynt";
            dict.Add("ccc", obj3);
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.PutModel(dict);
            while (true)
            {
                string line = Console.ReadLine();
                var prop = new FirebaseProperty();
                prop.SetValue(line);
            }
        }

        public static void TestObjectDictionarySub()
        {
            var dict = new FirebaseDictionary<TestStorable>(key =>
            {
                return new TestStorable();
            });
            dict.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("Count: " + dict.Keys.Count);
            };
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.SubModel(dict);
            while (true)
            {
                string line = Console.ReadLine();
                var prop = new FirebaseProperty();
                prop.SetValue(line);
            }
        }

        public static void TestObjectDictionarySub2()
        {
            var dict = new FirebaseDictionary<FirebaseObject>(key => new FirebaseObject());
            dict.CollectionChanged += (s, e) =>
            {
                //Console.WriteLine("Count: " + dict.Keys.Count);
            };

            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.SubModel(dict);
            wire.DataEvaluated += (s, e) =>
            {
                Console.WriteLine("Writes: " + app.Database.PendingWrites);
                Console.WriteLine("Total: " + e.TotalDataCount + " Sync: " + e.SyncedDataCount);
                Console.WriteLine("Count: " + dict.Keys.Count);
            };

            string lin11e = Console.ReadLine();

            for (int i = 0; i < 300; i++)
            {
                var obj = new TestStorable();
                obj.Test = i.ToString();
                dict.Add(UIDFactory.GenerateSafeUID(), obj);
            }

            while (true)
            {
                string line = Console.ReadLine();
                var prop = new FirebaseProperty();
                prop.SetValue(line);
            }
        }

        public static void TestObjectDictionarySub3()
        {
            var dict = new FirebaseDictionary<FirebaseObject>(key => new FirebaseObject());
            dict.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("Count: " + dict.Keys.Count);
            };
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.DataChanges += (s, e) =>
            {
                Console.WriteLine("Sync: " + wire.SyncedDataCount + "/" + wire.TotalDataCount);
            };
            wire.Start();
            wire.SubModel(dict);

            string lin11e = Console.ReadLine();

            for (int i = 0; i < 3; i++)
            {
                var obj = new TestStorable();
                obj.Test = i.ToString();
                dict.Add(i.ToString(), obj);
            }

            while (true)
            {
                string line = Console.ReadLine();
                dict.Remove(line);
            }
        }

        public static async Task TestDef()
        {
            var obj = new TestStorable();
            obj.IsOk = true;
            obj.Premium = TimeSpan.FromSeconds(60);
            obj.Premiums = new List<TimeSpan>() { TimeSpan.FromSeconds(30) };
            obj.Test = "testuuuuu";
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.PutModel(obj);
            wire.DataChanges += (s, e) =>
            {
                Console.WriteLine("Sync: " + wire.SyncedDataCount + "/" + wire.TotalDataCount);
            };

            await wire.WaitForSynced();

            Console.WriteLine("Put Done");

            Console.ReadLine();

            obj.IsOk = false;
            obj.Premium = default;
            obj.Premiums = default;
            obj.Test = default;
            obj.Num1 = default;
            obj.Num2 = default;
            obj.Num3 = default;

            Console.WriteLine("All mod");

            await wire.WaitForSynced();

            Console.WriteLine("Modify Done");

            Console.ReadLine();

            while (true)
            {
                string line = Console.ReadLine();
                obj.Test = string.IsNullOrEmpty(line) ? null : line;
            }
        }

        public static async Task TestRoutineWrite()
        {
            var dict = new FirebaseDictionary<FirebaseProperty>(key => new FirebaseProperty());
            dict.CollectionChanged += (s, e) =>
            {
                //Console.WriteLine("Count: " + dict.Keys.Count);
            };

            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.SubModel(dict);

            string lin11e = Console.ReadLine();

            for (int i = 0; i < 10; i++)
            {
                var prop = new FirebaseProperty();
                prop.SetValue(UIDFactory.GenerateSafeUID());
                dict.Add(i.ToString(), prop);
            }

            await wire.WaitForSynced();

            Stopwatch watch = new Stopwatch();
            while (true)
            {
                watch.Restart();
                foreach (var prop in dict)
                {
                    prop.Value.SetValue(UIDFactory.GenerateSafeUID());
                }
                await wire.WaitForSynced();
                Console.WriteLine("DURATION: " + watch.ElapsedMilliseconds);
            }
        }

        public static void TestCascadeObjectPut()
        {
            var obj = new CascadeStorable();
            obj.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Main: " + e.PropertyName);
            };
            obj.Storable1.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Storable1: " + e.PropertyName);
            };
            obj.Storable2.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Storable2: " + e.PropertyName);
            };
            obj.PropertyDictionary.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("PropertyDictionary: " + obj.PropertyDictionary.Count);
            };
            obj.ObjectDictionary.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("ObjectDictionary: " + obj.ObjectDictionary.Count);
            };

            bool isRun = false;
            bool toRun = false;
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.DataChanges += (s, e) =>
            {
                toRun = true;
                if (isRun) return;
                isRun = true;
                Task.Run(async delegate
                {
                    while (toRun)
                    {
                        toRun = false;
                        Console.WriteLine("Writes: " + app.Database.PendingWrites);
                        Console.WriteLine("Total: " + wire.TotalDataCount + " Sync: " + wire.SyncedDataCount);
                        await Task.Delay(500);
                    }
                    isRun = false;
                }).ConfigureAwait(false);
            };
            wire.Start();

            wire.PutModel(obj);

            obj.Test = "cscs";

            for (int i = 0; i < 10; i++)
            {
                var prop = new FirebaseProperty();
                prop.SetValue(i.ToString());
                obj.PropertyDictionary.Add(UIDFactory.GenerateSafeUID(), prop);
            }

            for (int i = 0; i < 5; i++)
            {
                var stor = new TestStorable();
                stor.Test = i.ToString();
                obj.ObjectDictionary.Add(UIDFactory.GenerateSafeUID(), stor);
            }

            while (true)
            {
                string line = Console.ReadLine();
                if (line == "view")
                {
                    var db = ((DatastoreBlob)app.Config.LocalDatabase).GetDB();
                    foreach (var pair in db)
                    {
                        Console.WriteLine("KEY: " + pair.Key + " VAL: " + pair.Value);
                    }
                }
                else
                {
                    obj.Test = string.IsNullOrEmpty(line) ? null : line;
                }
            }
        }

        public static void TestCascadeObjectMassPut()
        {
            var obj = new CascadeStorable();
            obj.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Main: " + e.PropertyName);
            };
            obj.Storable1.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Storable1: " + e.PropertyName);
            };
            obj.Storable2.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Storable2: " + e.PropertyName);
            };
            obj.PropertyDictionary.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("PropertyDictionary: " + obj.PropertyDictionary.Count);
            };
            obj.ObjectDictionary.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("ObjectDictionary: " + obj.ObjectDictionary.Count);
            };

            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.DataEvaluated += (s, e) =>
            {
                Console.WriteLine("Writes: " + app.Database.PendingWrites);
                Console.WriteLine("Total: " + wire.TotalDataCount + " Sync: " + wire.SyncedDataCount);
            };
            wire.Start();

            wire.PutModel(obj);

            string lin11e = null;

            while (lin11e != "q")
            {
                lin11e = Console.ReadLine();
                Console.WriteLine("Writes: " + app.Database.PendingWrites);
                Console.WriteLine("Total: " + wire.TotalDataCount + " Sync: " + wire.SyncedDataCount);
            }

            obj.Test = "cscs";

            for (int i = 0; i < 1000; i++)
            {
                var prop = new FirebaseProperty();
                prop.SetValue(i.ToString());
                obj.PropertyDictionary.Add(UIDFactory.GenerateSafeUID(), prop);
            }

            for (int i = 0; i < 50; i++)
            {
                var stor = new TestStorable();
                stor.Test = i.ToString();
                obj.ObjectDictionary.Add(UIDFactory.GenerateSafeUID(), stor);
            }

            while (true)
            {
                string line = Console.ReadLine();
                if (line == "view")
                {
                    var db = ((DatastoreBlob)app.Config.LocalDatabase).GetDB();
                    foreach (var pair in db.Where(i => i.Key.Contains("changes")))
                    {
                        Console.WriteLine("KEY: " + pair.Key + " VAL: " + pair.Value);
                    }
                }
                else
                {
                    obj.Test = string.IsNullOrEmpty(line) ? null : line;
                }
            }
        }

        public static void TestCascadeObjectSub()
        {
            var obj = new CascadeStorable();
            obj.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Main: " + e.PropertyName);
            };
            obj.Storable1.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Storable1: " + e.PropertyName);
            };
            obj.Storable2.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Storable2: " + e.PropertyName);
            };
            obj.PropertyDictionary.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("PropertyDictionary: " + obj.PropertyDictionary.Count);
            };
            obj.ObjectDictionary.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("ObjectDictionary: " + obj.ObjectDictionary.Count);
            };

            bool isRun = false;
            bool toRun = false;
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.DataChanges += (s, e) =>
            {
                toRun = true;
                if (isRun) return;
                isRun = true;
                Task.Run(async delegate
                {
                    while (toRun)
                    {
                        toRun = false;
                        Console.WriteLine("Writes: " + app.Database.PendingWrites);
                        Console.WriteLine("Total: " + wire.TotalDataCount + " Sync: " + wire.SyncedDataCount);
                        await Task.Delay(500);
                    }
                    isRun = false;
                }).ConfigureAwait(false);
            };
            wire.Start();

            Console.WriteLine("STARTSUB");
            wire.SubModel(obj);
            Console.WriteLine("DONESUB");

            //obj.Test = "cscs";

            //for (int i = 0; i < 100; i++)
            //{
            //    var prop = new FirebaseProperty();
            //    prop.SetValue(i.ToString());
            //    obj.PropertyDictionary.Add(UIDFactory.GenerateSafeUID(), prop);
            //}

            //for (int i = 0; i < 25; i++)
            //{
            //    var stor = new TestStorable();
            //    stor.Test = i.ToString();
            //    obj.ObjectDictionary.Add(UIDFactory.GenerateSafeUID(), stor);
            //}

            while (true)
            {
                string line = Console.ReadLine();
                if (line == "view")
                {
                    var db = ((DatastoreBlob)app.Config.LocalDatabase).GetDB();
                    foreach (var pair in db)
                    {
                        Console.WriteLine("KEY: " + pair.Key + " VAL: " + pair.Value);
                    }
                }
                else
                {
                    obj.Test = string.IsNullOrEmpty(line) ? null : line;
                }
            }
        }

        public static async Task TestCascadeObjectSetNull()
        {
            var obj = new CascadeStorable();
            obj.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Main: " + e.PropertyName);
            };
            obj.Storable1.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Storable1: " + e.PropertyName);
            };
            obj.Storable2.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Storable2: " + e.PropertyName);
            };
            obj.PropertyDictionary.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("PropertyDictionary: " + obj.PropertyDictionary.Count);
            };
            obj.ObjectDictionary.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("ObjectDictionary: " + obj.ObjectDictionary.Count);
            };

            bool isRun = false;
            bool toRun = false;
            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.DataChanges += (s, e) =>
            {
                toRun = true;
                if (isRun) return;
                isRun = true;
                Task.Run(async delegate
                {
                    while (toRun)
                    {
                        toRun = false;
                        Console.WriteLine("Writes: " + app.Database.PendingWrites);
                        Console.WriteLine("Total: " + wire.TotalDataCount + " Sync: " + wire.SyncedDataCount);
                        await Task.Delay(500);
                    }
                    isRun = false;
                }).ConfigureAwait(false);
            };
            wire.Start();

            wire.PutModel(obj);

            obj.Test = "cscs";

            for (int i = 0; i < 10; i++)
            {
                var prop = new FirebaseProperty();
                prop.SetValue(i.ToString());
                obj.PropertyDictionary.Add(UIDFactory.GenerateSafeUID(), prop);
            }

            for (int i = 0; i < 3; i++)
            {
                var stor = new TestStorable();
                stor.Test = i.ToString();
                obj.ObjectDictionary.Add(UIDFactory.GenerateSafeUID(), stor);
            }

            for (int i = 0; i < 5; i++)
            {
                var cas = new CascadeStorable();
                cas.Test = i.ToString();

                for (int j = 0; j < 10; j++)
                {
                    var prop = new FirebaseProperty();
                    prop.SetValue(j.ToString());
                    cas.PropertyDictionary.Add(UIDFactory.GenerateSafeUID(), prop);
                }

                for (int j = 0; j < 3; j++)
                {
                    var stor = new TestStorable();
                    stor.Test = j.ToString();
                    cas.ObjectDictionary.Add(UIDFactory.GenerateSafeUID(), stor);
                }

                //obj.CascadeDictionary.Add(i.ToString(), cas);
            }

            await wire.WaitForSynced();

            Console.WriteLine("ISNULL: " + (obj.IsNull() ? "Yes" : "No"));
            Console.WriteLine("SETNULL");
            obj.SetNull();
            Console.WriteLine("ISNULL: " + (obj.IsNull() ? "Yes" : "No"));
            Console.WriteLine("WAIT");
            await wire.WaitForSynced();
            Console.WriteLine("ISNULL: " + (obj.IsNull() ? "Yes" : "No"));

            while (true)
            {
                string line = Console.ReadLine();
                if (line == "view")
                {
                    var db = ((DatastoreBlob)app.Config.LocalDatabase).GetDB();
                    foreach (var pair in db)
                    {
                        Console.WriteLine("KEY: " + pair.Key + " VAL: " + pair.Value);
                    }
                }
                else
                {
                    obj.Test = string.IsNullOrEmpty(line) ? null : line;
                }
            }
        }
    }
}
