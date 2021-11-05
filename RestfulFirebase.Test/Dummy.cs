using RestfulFirebase;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Test.Utilities;
using RestfulFirebase.Test;
using Xunit;
using System;
using RestfulFirebase.Test.Models;
using System.Threading.Tasks;
using RestfulFirebase.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using RestfulFirebase.Local;
using System.Threading;

namespace DummyTest
{
    public class Dummy
    {
        [Fact]
        public async void Normal()
        {
            var app = new RestfulFirebaseApp(Config.YourConfig());
            await app.Auth.SignInWithEmailAndPassword("t@st.com", "123123");

            var userNode = app.Database.Child("users").Child(app.Auth.Session.LocalId);

            var wire = app.Database.Child("public").AsRealtimeWire();

            wire.DataChanges += (s, e) =>
            {
                (int total, int sycned) = wire.GetDataCount();
                Console.WriteLine("Sync: " + sycned + "/" + total + " Path: " + e.Path);
            };
            wire.Error += (s, e) =>
            {
                Console.WriteLine("OnError: " + e.Uri + " Message: " + e.Exception.Message);
            };

            wire.Start();

            var model = await wire.SubModelAsync(new TestStorable());

            model.Test1 = "sad1";
            model.Test2 = "das1";
            model.Test3 = "asd1";

            await Task.Delay(10000);

            await Task.Delay(10000);
        }

        [Fact]
        public async void NewNormal()
        {
            var app = new RestfulFirebaseApp(Config.YourConfig());
            await app.Auth.SignInWithEmailAndPassword("t@st.com", "123123");

            var userNode = app.Database.Child("users").Child(app.Auth.Session.LocalId);

            var wire = app.Database.Child("public").AsRealtimeWire();

            wire.DataChanges += (s, e) =>
            {
                (int total, int sycned) = wire.GetDataCount();
                Console.WriteLine("Sync: " + sycned + "/" + total + " Path: " + e.Path);
            };
            wire.Error += (s, e) =>
            {
                Console.WriteLine("OnError: " + e.Uri + " Message: " + e.Exception.Message);
            };

            wire.Start();

            await Task.Delay(10000);

            await Task.Delay(10000);
        }

        [Fact]
        public async void NewNormal2()
        {
            var app = new RestfulFirebaseApp(Config.YourConfig());
            await app.Auth.SignInWithEmailAndPassword("t@st.com", "123123");

            var userNode = app.Database.Child("users").Child(app.Auth.Session.LocalId);

            var wire = app.Database.Child("public").AsRealtimeWire();

            wire.DataChanges += (s, e) =>
            {
                (int total, int sycned) = wire.GetDataCount();
                Console.WriteLine("Sync: " + sycned + "/" + total + " Path: " + e.Path);
            };
            wire.Error += (s, e) =>
            {
                Console.WriteLine("OnError: " + e.Uri + " Message: " + e.Exception.Message);
            };

            wire.Start();

            while (true) { }
        }

        [Fact]
        public async void NewNormal3()
        {
            var app = new RestfulFirebaseApp(Config.YourConfig());
            await app.Auth.SignInWithEmailAndPassword("t@st.com", "123123");

            var userNode = app.Database.Child("users").Child(app.Auth.Session.LocalId);

            var wire = app.Database.Child("publisdc").AsRealtimeWire();

            wire.DataChanges += (s, e) =>
            {
                (int total, int sycned) = wire.GetDataCount();
                Console.WriteLine("Sync: " + sycned + "/" + total + " Path: " + e.Path);
            };
            wire.Error += (s, e) =>
            {
                Console.WriteLine("OnError: " + e.Uri + " Message: " + e.Exception.Message);
            };

            app.LocalDatabase.Subscribe((s, e) =>
            {

            });

            wire.Start();

            await Task.Delay(5000);

            var ssss = wire.GetAllChildren();

            wire.SetValue("test21", "one", "two", "threee1");
            ////await Task.Delay(5000);
            //var s3 = wire.GetAllChildren();
            //wire.SetValue("test22", "one", "two", "threee2");
            ////await Task.Delay(5000);
            //var s4 = wire.GetAllChildren();
            //wire.SetValue("test23", "one", "two", "threee3");
            ////await Task.Delay(5000);
            //var s5 = wire.GetAllChildren();
            //wire.SetValue(null, "one", "two", "threee3");
            ////await Task.Delay(5000);
            //var s6 = wire.GetAllChildren();
            //wire.SetValue("test23", "one", "two", "threee3");
            ////await Task.Delay(5000);
            //var s7 = wire.GetAllChildren();
            //wire.SetValue("t", "one");
            ////await Task.Delay(5000);
            //var s8 = wire.GetAllChildren();

            while (true) { }
        }

        [Fact]
        public void Norm()
        {
            List<string[]> samples = new List<string[]>();
            Stopwatch s = new Stopwatch();
            Random random = RandomUtilities.GetThreadRandom();
            s.Restart();
            for (int i = 0; i < 1000; i++)
            {
                List<string> sample = new List<string>();
                for (int j = 0; j < random.Next(100, 1000); j++)
                {
                    sample.Add(UIDFactory.GenerateUID(random.Next(5, 1000)));
                }
                samples.Add(sample.ToArray());
            }
            long stamp1 = s.ElapsedMilliseconds;
            s.Restart();

            foreach (string[] sample in samples)
            {
                string serialized = StringUtilities.Serialize(sample);
                string[] deserialized = StringUtilities.Deserialize(serialized);
                Assert.Equal(sample.Length, deserialized.Length);
                for (int j = 0; j < sample.Length; j++)
                {
                    Assert.Equal(sample[j], deserialized[j]);
                }
            }

            long stamp2 = s.ElapsedMilliseconds;
            s.Restart();
        }
    }
}