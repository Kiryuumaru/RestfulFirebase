using RestfulFirebase.Attributes;
using RestfulFirebase.Local;
using RestfulFirebase.RealtimeDatabase.Realtime;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace RestfulFirebase.Playground;

public static class Helpers
{
    public static Task<(Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>> generator, Action dispose)> AuthenticatedTestApp(string testName, string factName)
    {
        return TestCore.Helpers.AuthenticatedTestApp(nameof(Playground), testName, factName);
    }

    public static Task CleanTest(
        string testName,
        string factName,
        Func<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>, Task> test)
    {
        return TestCore.Helpers.CleanTest(nameof(Playground), testName, factName, test);
    }

    public static Task CleanTest(
        string testName,
        string factName,
        Action<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>> test)
    {
        return TestCore.Helpers.CleanTest(nameof(Playground), testName, factName, test);
    }
}

public class UnitTest1
{
    [Fact]
    public async void Mockk1()
    {
        await Helpers.CleanTest(nameof(UnitTest1), nameof(Mockk1), async generator =>
        {
            var (app, wire, dataChanges) = await generator(null);

            if (app.Auth.Session?.LocalId == null)
            {
                Assert.True(false, "Auth is null");
                return;
            }

            //var user1 = app.CloudFirestore
            //    .Collection("public")
            //    .Document("uNxJbRMEipmQ2Qza0oES");

            //var user2 = app.CloudFirestore
            //    .Collection("public")
            //    .Document("uNxJbRMEipmQ2Qza0oES")
            //    .Collection("awd")
            //    .Document("dwa");

            //JsonDocument get1;
            //JsonDocument get2;
            //try
            //{
            //    get1 = await user1.GetAsync();
            //    get2 = await user2.GetAsync();
            //}
            //catch (Exception ex)
            //{

            //}

            Assert.True(true);
        });
    }

    public class TestModel
    {
        [FirebaseProperty]
        public string? Name { get; set; }
    }

    [Fact]
    public async void Mockk2()
    {
        await Helpers.CleanTest(nameof(UnitTest1), nameof(Mockk2), async generator =>
        {
            var (app, wire, dataChanges) = await generator(null);

            if (app.Auth.Session?.LocalId == null)
            {
                Assert.True(false, "Auth is null");
                return;
            }

            var wire1 = app.RealtimeDatabase
                .Database()
                .Child("awd")
                .AsRealtimeWire();

            wire1.Start();

            var dict = new Dictionary<string, string>();
            var col = new List<decimal>();
            var obj = new TestModel();

            var rw1 = wire1.Subscribe(dict, "1");
            var rw2 = wire1.Subscribe(col, "2");
            var rw3 = wire1.Subscribe(obj, "3");


            Assert.True(true);
        });
    }
}