using RestfulFirebase.RealtimeDatabase.References;
using System.Collections.Generic;
using Xunit;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using RestfulFirebase.Authentication;

namespace RestfulFirebase.UnitTest;

public class RealtimeDatabaseTest
{
    internal static async Task Cleanup(Reference testReference)
    {
        //var oldDataList = await testReference.Query().Run();
        //Assert.NotNull(oldDataList.Result);

        //List<Document> oldDocs = new();

        //await foreach (var page in oldDataList.Result)
        //{
        //    page.ThrowIfError();
        //    foreach (var doc in page.Result.Documents)
        //    {
        //        oldDocs.Add(doc.Document);
        //    }
        //}
        //var cleanups = await testReference.DeleteDocuments(oldDocs.Select(i => i.Reference.Id));
        //cleanups.ThrowIfError();
    }

    [Fact]
    public async void TransformSetToServerValueTest()
    {
        FirebaseApp app = Helpers.GetFirebaseApp();

        Reference testReference = app.RealtimeDatabase
            .Database()
            .Child("public")
            .Child(nameof(FirestoreDatabaseTest))
            .Child(nameof(TransformSetToServerValueTest));

        await Cleanup(testReference);

        FirebaseUser? user;

        // Login to firebase auth
        var loginRequest = await app.Authentication.SignInWithEmailAndPassword("test@mail.com", "123123");

        if (loginRequest.IsSuccess)
        {
            user = loginRequest.Result;
        }
        else
        {
            // Create firebase auth
            var signupRequest = await app.Authentication.CreateUserWithEmailAndPassword("test@mail.com", "123123");

            signupRequest.ThrowIfError();

            user = signupRequest.Result;
        }

        var ss = await testReference.Query()
            .OrderByValue()
            .StartAt(1)
            .EndAt(5)
            .Build();

        await Cleanup(testReference);

        Assert.True(true);
    }
}
