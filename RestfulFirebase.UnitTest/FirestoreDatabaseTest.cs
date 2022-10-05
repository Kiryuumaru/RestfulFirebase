using RestfulFirebase.FirestoreDatabase.References;
using System.Collections.Generic;
using Xunit;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Queries;
using System.Threading.Tasks;
using RestfulFirebase.FirestoreDatabase.Transform;
using System;
using RestfulFirebase.FirestoreDatabase.Enums;

namespace RestfulFirebase.UnitTest;

public class FirestoreDatabaseTest
{
    //internal static async Task Cleanup(FirebaseApp app, CollectionReference collectionReference)
    //{
    //    var oldDataList = await app.FirestoreDatabase.QueryDocument(new QueryDocumentRequest()
    //    {
    //        Config = config,
    //        JsonSerializerOptions = Helpers.JsonSerializerOptions,
    //        From = collectionReference
    //    });
    //    Assert.NotNull(oldDataList.Result);

    //    List<Document> oldDocs = new();

    //    await foreach (var page in oldDataList.Result)
    //    {
    //        page.ThrowIfError();
    //        foreach (var doc in page.Result.Documents)
    //        {
    //            oldDocs.Add(doc.Document);
    //        }
    //    }
    //    var cleanups = await app.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
    //    {
    //        Config = config,
    //        DeleteDocument = oldDocs
    //    });
    //    cleanups.ThrowIfError();
    //}
}
