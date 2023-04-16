using RestfulFirebase.RealtimeDatabase.References;
using System.Collections.Generic;
using Xunit;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using RestfulFirebase.Authentication;

namespace RealtimeDatabaseTest;

public class RealtimeDatabaseHelpers
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
}
