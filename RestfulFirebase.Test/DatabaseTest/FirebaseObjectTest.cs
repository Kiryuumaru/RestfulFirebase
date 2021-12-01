using RestfulFirebase;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Test.Utilities;
using RestfulFirebase.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DatabaseTest.DatabaseTest
{
    public class FirebaseObjectTest
    {
        [Fact]
        public async void Normal()
        {
            var app = new RestfulFirebaseApp(Config.YourConfig());
            await app.Auth.SignInWithEmailAndPassword("t@st.com", "123123");

            Assert.True(app.Auth.IsAuthenticated);

            var dict = new FirebaseDictionary<DateTime>();

            var wire = app.Database
                .Child("users")
                .Child(app.Auth.Session.LocalId)
                .Child(nameof(FirebaseDictionaryTest))
                .AsRealtimeWire();

            wire.Start();

            wire.SubModel(dict);
        }
    }
}
