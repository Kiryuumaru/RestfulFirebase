﻿using RestfulFirebase.Auth;
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
using RestfulFirebase.Database.Models.Primitive;
using ObservableHelpers.Serializers;

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

        #region Initializers

        public TestStorable()
            : base()
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
                LocalDatabase = new DatastoreBlob(true)
            });

            var signInResult = await app.Auth.SignInWithEmailAndPasswordAsync("t@st.com", "123123");
            var update = await app.Auth.UpdateProfileAsync("disp", "123123");
            userNode = app.Database.Child("users").Child(app.Auth.User.LocalId);

            Console.WriteLine("FIN");
            //TestObservableObject();
            //TestPropertyPut();
            //TestPropertySub();
            //TestObjectPut();s
            //TestObjectSub();
            //TestPropertyDictionaryPut();
            TestPropertyDictionarySub();
            //TestObjectDictionaryPut();
            //TestObjectDictionarySub();
            //ExperimentList();
        }

        public static void TestPropertyPut()
        {
            var props = new FirebaseProperty<string>();
            props.PropertyChanged += (s, e) =>
            {
                Console.WriteLine("Data: " + props.Value + " Prop: " + e.PropertyName);
            };
            props.Value = "numba22";
            userNode.Child("testing").Child("mock").PutAsRealtime("test", props).Start();
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
            props.Value = "numba11";
            userNode.Child("testing").Child("mock").SubAsRealtime("test", props).Start();
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
                        write += Serializer.Serialize(obj.IsOk);
                        break;
                    case nameof(TestStorable.Premium):
                        write += Serializer.Serialize(obj.Premium);
                        break;
                    case nameof(TestStorable.Premiums):
                        write += Serializer.Serialize(obj.Premiums);
                        break;
                    case nameof(TestStorable.Test):
                        write += Serializer.Serialize(obj.Test);
                        break;
                }
                Console.WriteLine(write);
            };
            obj.IsOk = true;
            obj.Premium = TimeSpan.FromSeconds(60);
            obj.Premiums = new List<TimeSpan>() { TimeSpan.FromSeconds(30) };
            obj.Test = "testuuuuu";
            userNode.Child("testing").PutAsRealtime("mock", obj).Start();
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
                        write += Serializer.Serialize(obj.IsOk);
                        break;
                    case nameof(TestStorable.Premium):
                        write += Serializer.Serialize(obj.Premium);
                        break;
                    case nameof(TestStorable.Premiums):
                        write += Serializer.Serialize(obj.Premiums);
                        break;
                    case nameof(TestStorable.Test):
                        write += Serializer.Serialize(obj.Test);
                        break;
                }
                Console.WriteLine(write);
            };
            //obj.IsOk = true;
            //obj.Premium = TimeSpan.FromSeconds(60);
            //obj.Premiums = new List<TimeSpan>() { TimeSpan.FromSeconds(30) };
            //obj.Test = "testuuuuu";
            userNode.Child("testing").SubAsRealtime("mock", obj).Start();
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
            userNode.Child("testing").PutAsRealtime("mock", dict).Start();
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
            var wire = userNode.Child("testing").SubAsRealtime("mock", dict);
            wire.OnDataChanges += (s, e) =>
            {
                Console.WriteLine("Total: " + e.TotalDataCount.ToString() + " Sync: " + e.SyncedDataCount.ToString());
            };
            wire.Start();
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
            userNode.Child("testing").PutAsRealtime("mock", dict).Start();
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
            userNode.Child("testing").SubAsRealtime("mock", dict).Start();
            while (true)
            {
                string line = Console.ReadLine();
                var prop = new FirebaseProperty();
                prop.SetValue(line);
            }
        }
    }
}
