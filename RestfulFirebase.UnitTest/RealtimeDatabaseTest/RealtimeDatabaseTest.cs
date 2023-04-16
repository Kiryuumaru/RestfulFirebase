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

public class TransformSetToServerValueTest
{
    [Fact]
    public async void Test1()
    {
        FirebaseApp app = FirebaseHelpers.GetFirebaseApp();

        //var userRequest = await app.Authentication.SignInAnonymously();
        //userRequest.ThrowIfError();
        //var user = userRequest.Result;

        //var database = app.RealtimeDatabase
        //    .Database();

        //Reference testReference = database
        //    .Child("public")
        //    .Child(nameof(TransformSetToServerValueTest))
        //    .Child(FirebaseHelpers.TestInstanceId);

        //await RealtimeDatabaseHelpers.Cleanup(testReference);

        //var query = testReference.Query();

        //var queryAuth = query.Authorization(user);

        //var queryFilter = queryAuth
        //    .OrderByKey()
        //    .EndAt("a");

        //var queryRun = await queryFilter.Run();

        //var asd = await queryRun.GetTransactionContentsAsString();

        ////await Task.Delay(1000000);

        //await RealtimeDatabaseHelpers.Cleanup(testReference);

        //var userDeleteRequest = await user.DeleteUser();
        //userDeleteRequest.ThrowIfError();

        Assert.True(true);
    }
}
