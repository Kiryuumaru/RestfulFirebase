using RestfulFirebase.FirestoreDatabase.References;
using System.Collections.Generic;
using Xunit;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Queries;
using System.Threading.Tasks;
using RestfulFirebase.FirestoreDatabase.Writes;
using System;
using RestfulFirebase.FirestoreDatabase.Enums;

namespace FirestoreDatabaseTest;

public class FirestoreDatabaseHelpers
{
    internal static async Task Cleanup(CollectionReference collectionReference)
    {
        var oldDataList = await collectionReference.Query().Run();
        Assert.NotNull(oldDataList.Result);

        List<Document> oldDocs = new();

        await foreach (var page in oldDataList.Result)
        {
            page.ThrowIfError();
            foreach (var doc in page.Result.Documents)
            {
                oldDocs.Add(doc.Document);
            }
        }
        var cleanups = await collectionReference.DeleteDocuments(oldDocs.Select(i => i.Reference.Id));
        cleanups.ThrowIfError();
    }
}
