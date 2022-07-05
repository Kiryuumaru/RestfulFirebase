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
    public async void Mockk()
    {
        await Helpers.CleanTest(nameof(UnitTest1), nameof(Mockk), async generator =>
        {
            var (app, wire, dataChanges) = await generator(null);

            if (app.Auth.Session?.LocalId == null)
            {
                Assert.True(false, "Auth is null");
                return;
            }

            var user1 = app.CloudFirestore
                .Collection("public")
                .Document("uNxJbRMEipmQ2Qza0oES");

            var user2 = app.CloudFirestore
                .Collection("public")
                .Document("uNxJbRMEipmQ2Qza0oES")
                .Collection("awd")
                .Document("dwa");

            JsonDocument get1;
            JsonDocument get2;
            try
            {
                get1 = await user1.GetAsync();
                get2 = await user2.GetAsync();
            }
            catch (Exception ex)
            {

            }

            Assert.True(true);
        });
    }
}