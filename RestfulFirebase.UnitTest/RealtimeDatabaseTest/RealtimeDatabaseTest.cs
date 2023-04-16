using RestfulFirebase.RealtimeDatabase.References;
using System.Collections.Generic;
using Xunit;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using RestfulFirebase.Authentication;
using RestfulFirebase;
using RestfulFirebase.UnitTest;

namespace RealtimeDatabaseTest;

public class RealtimeDatabaseTest
{
    [Fact]
    public async void TransformSetToServerValueTest()
    {
        FirebaseApp app = FirebaseHelpers.GetFirebaseApp();

        Reference testReference = app.RealtimeDatabase
            .Database()
            .Child("public")
            .Child("test1");

        await RealtimeDatabaseHelpers.Cleanup(testReference);


        var ss = await testReference.Query()
            .OrderByKey()
            .EndAt("a")
            .Run();

        var asd = await ss.GetTransactionContentsAsString();

        //await Task.Delay(1000000);

        await RealtimeDatabaseHelpers.Cleanup(testReference);

        Assert.True(true);
    }
}
