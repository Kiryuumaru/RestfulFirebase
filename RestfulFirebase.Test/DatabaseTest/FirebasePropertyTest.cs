﻿using RestfulFirebase;
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
    public class FirebasePropertyTest
    {
        [Fact]
        public async void Normal()
        {
            var generator = await Helpers.AuthenticatedAppGenerator();
            var app = generator();

            var model = new FirebaseProperty<string>();

            var wire = app.Database
                .Child("users")
                .Child(app.Auth.Session.LocalId)
                .Child(nameof(FirebasePropertyTest))
                .AsRealtimeWire();

            wire.Start();

            wire.SubModel(model);

            model.Value = "pass";

            await model.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1));

            var model2 = new FirebaseProperty<string>();

            var wire2 = app.Database
                .Child("users")
                .Child(app.Auth.Session.LocalId)
                .Child(nameof(FirebasePropertyTest))
                .AsRealtimeWire();

            wire2.Start();

            wire2.SubModel(model2);

            await model2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1));

            Assert.Equal(model.Value, model2.Value);

            Assert.True(wire.SetNull());
            wire.MaxConcurrentWrites = 10;
            Assert.True(await wire.WaitForSynced(true));
        }
    }
}
