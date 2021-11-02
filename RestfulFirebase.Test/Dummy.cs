using RestfulFirebase;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Test.Utilities;
using RestfulFirebase.Test;
using Xunit;
using System;
using RestfulFirebase.Test.Models;
using System.Threading.Tasks;

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
                Console.WriteLine("Sync: " + wire.SyncedDataCount + "/" + wire.TotalDataCount + " Path: " + e.Path);
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
                Console.WriteLine("Sync: " + wire.SyncedDataCount + "/" + wire.TotalDataCount + " Path: " + e.Path);
            };
            wire.Error += (s, e) =>
            {
                Console.WriteLine("OnError: " + e.Uri + " Message: " + e.Exception.Message);
            };

            wire.Start();

            await Task.Delay(10000);

            await Task.Delay(10000);
        }
    }
}