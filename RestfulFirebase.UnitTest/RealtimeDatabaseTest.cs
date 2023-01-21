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
            .Child("test1");

        await Cleanup(testReference);

        var ss = await testReference.Query()
            .Run();

        var asd = await ss.GetTransactionContentsAsString();

        //await Task.Delay(1000000);

        await Cleanup(testReference);

        Assert.True(true);
    }
}
