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

namespace RestTest
{
    public class TestStorable : FirebaseObject
    {
        #region Properties

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
            app = new RestfulFirebaseApp(Config.YourConfig());

            var signInResult = await app.Auth.SignInWithEmailAndPassword("t@st.com", "123123");
            var update = await app.Auth.UpdateProfile("disp", "123123");
            userNode = app.Database.Child("users").Child(app.Auth.Session.LocalId);

            Console.WriteLine("FIN");
            //TestObservableObject();
            TestRealtimeWire();
            //TestRealtimeWire2();
            //TestPropertyPut();
            //TestPropertySub();
            //TestPropertySub2();
            //TestObjectPut();
            //TestObjectSub();
            //TestPropertyDictionaryPut();
            //TestPropertyDictionarySub();
            //TestPropertyDictionarySub2();
            //TestObjectDictionaryPut();
            //TestObjectDictionarySub();
            //ExperimentList();
        }

        public static void TestRealtimeWire()
        {
            var wire = app.Database.Child("public").AsRealtimeWire();
            wire.OnChanges += (s, e) =>
            {
                Console.WriteLine("Sync: " + e.SyncedDataCount.ToString() + "/" + e.TotalDataCount.ToString() + " Path: " + e.Path);
            };
            wire.OnError += (s, e) =>
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
                        var separated = Utils.SeparateUrl(path);
                        RealtimeIntance subWire = wire;
                        for (int i = 0; i < separated.Length; i++)
                        {
                            subWire = subWire.Child(separated[i]);
                        }
                        subWire.SetBlob(null);
                    }
                    else
                    {
                        var data = line.Substring(line.IndexOf(' ') + 1);
                        var separated = Utils.SeparateUrl(path);
                        RealtimeIntance subWire = wire;
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
                        Console.WriteLine("Key: " + pair.Key + " Value: " + pair.Value);
                    }
                }
            }
        }

        public static void TestRealtimeWire2()
        {
            var wire = app.Database.Child("public").AsRealtimeWire();
            wire.OnChanges += (s, e) =>
            {
                Console.WriteLine("Main Sync: " + e.SyncedDataCount.ToString() + "/" + e.TotalDataCount.ToString() + " Path: " + e.Path);
            };
            wire.OnError += (s, e) =>
            {
                Console.WriteLine("Main OnError: " + e.Uri + " Message: " + e.Exception.Message);
            };
            var subWire1 = wire.Child("sub1");
            subWire1.OnChanges += (s, e) =>
            {
                Console.WriteLine("Sub1 Sync: " + e.SyncedDataCount.ToString() + "/" + e.TotalDataCount.ToString() + " Path: " + e.Path);
            };
            subWire1.OnError += (s, e) =>
            {
                Console.WriteLine("Sub1 OnError: " + e.Uri + " Message: " + e.Exception.Message);
            };
            var subWire2 = wire.Child("sub2");
            subWire2.OnChanges += (s, e) =>
            {
                Console.WriteLine("Sub2 Sync: " + e.SyncedDataCount.ToString() + "/" + e.TotalDataCount.ToString() + " Path: " + e.Path);
            };
            subWire2.OnError += (s, e) =>
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
                            var separated = Utils.SeparateUrl(path);
                            RealtimeIntance subWire = subWire1;
                            for (int i = 0; i < separated.Length; i++)
                            {
                                subWire = subWire.Child(separated[i]);
                            }
                            subWire.SetBlob(null);
                        }
                        else
                        {
                            var data = line.Substring(line.IndexOf(' ') + 1);
                            var separated = Utils.SeparateUrl(path);
                            RealtimeIntance subWire = subWire1;
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
                            var separated = Utils.SeparateUrl(path);
                            RealtimeIntance subWire = subWire2;
                            for (int i = 0; i < separated.Length; i++)
                            {
                                subWire = subWire.Child(separated[i]);
                            }
                            subWire.SetBlob(null);
                        }
                        else
                        {
                            var data = line.Substring(line.IndexOf(' ') + 1);
                            var separated = Utils.SeparateUrl(path);
                            RealtimeIntance subWire = subWire2;
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
                        Console.WriteLine("Key: " + pair.Key + " Value: " + pair.Value);
                    }
                }
            }
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
                        write += obj.Premiums.ToString();
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
                        write += obj.Premiums.ToString();
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

        public static void TestPropertyDictionaryPut()
        {
            var dict = new FirebasePropertyDictionary();
            dict.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("Count: " + dict.Keys.Count);
            };
            var prop1 = new FirebaseProperty();
            prop1.SetValue("111");
            dict.Add("aaa", prop1);
            var prop2 = new FirebaseProperty();
            prop2.SetValue("222");
            dict.Add("bbb", prop2);
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
            var dict = new FirebasePropertyDictionary();
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
            wire.OnChanges += (s, e) =>
            {
                Console.WriteLine("Total: " + e.TotalDataCount.ToString() + " Sync: " + e.SyncedDataCount.ToString());
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
            var dict = new FirebasePropertyDictionary();
            dict.CollectionChanged += (s, e) =>
            {
                //Console.WriteLine("Count: " + dict.Keys.Count);
            };

            var wire = userNode.Child("testing").Child("mock").AsRealtimeWire();
            wire.Start();
            wire.SubModel(dict);
            wire.OnChanges += (s, e) =>
            {
                Console.WriteLine("Total: " + e.TotalDataCount.ToString() + " Sync: " + e.SyncedDataCount.ToString());
            };

            string lin11e = Console.ReadLine();

            for (int i = 0; i < 1000; i++)
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

        public static void TestObjectDictionaryPut()
        {
            var dict = new FirebaseObjectDictionary();
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
            var dict = new FirebaseObjectDictionary();
            dict.CollectionChanged += (s, e) =>
            {
                Console.WriteLine("Count: " + dict.Keys.Count);
            };
            //var obj1 = new TestStorable();
            //dict.Add("aaa", obj1);
            //var obj2 = new TestStorable();
            //obj2.IsOk = true;
            //obj2.Premium = TimeSpan.FromSeconds(60);
            //obj2.Premiums = new List<TimeSpan>() { TimeSpan.FromSeconds(30) };
            //obj2.Test = "testuuuuu";
            //dict.Add("bbb", obj2);
            //var obj3 = new TestStorable();
            //obj2.IsOk = false;
            //obj2.Premium = TimeSpan.FromSeconds(3600);
            //obj2.Premiums = new List<TimeSpan>() { TimeSpan.FromSeconds(7200) };
            //obj2.Test = "CLynt";
            //dict.Add("ccc", obj3);
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
    }
}
