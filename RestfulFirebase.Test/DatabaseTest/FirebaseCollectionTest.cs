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
    public class FirebaseCollectionTest
    {
        [Fact]
        public async void Normal()
        {
            var generator = await Helpers.AuthenticatedAppGenerator();
            var app = await generator();

            Assert.True(app.Auth.IsAuthenticated);

            var dict = new FirebaseDictionary<DateTime>();

            var wire = app.Database
                .Child("users")
                .Child(app.Auth.Session.LocalId)
                .Child(nameof(FirebaseDictionaryTest))
                .AsRealtimeWire();

            wire.Start();

            wire.SubModel(dict);

            app.Dispose();
        }
    }
}
