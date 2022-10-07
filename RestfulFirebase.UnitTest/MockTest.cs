using Xunit;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.Common.Models;
using System.Collections.Generic;
using System.Linq;
using RestfulFirebase.Common.Utilities;

namespace RestfulFirebase.UnitTest;

public class MockTest
{
    [Fact]
    public async void Test1()
    {
        FirebaseApp app = Helpers.GetFirebaseApp();

        CollectionReference testCollectionReference = app.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection("QueryDocument");

        var writeDocuments = await testCollectionReference.PatchAndGetDocuments(
            new (string, MixedModel?)[]
            {
                ($"test00", new()
                {
                    Val1 = long.MinValue,
                    Val2 = 3.3,
                    Val3 = "a"
                }),
                ($"test01", new()
                {
                    Val1 = 1,
                    Val2 = 4.4,
                    Val3 = "b"
                }),
                ($"test02", new()
                {
                    Val1 = 1,
                    Val2 = 5.5,
                    Val3 = "c"
                }),
                ($"test03", new()
                {
                    Val1 = 2,
                    Val2 = 6.6,
                    Val3 = "d"
                }),
                ($"test04", new()
                {
                    Val1 = 2,
                    Val2 = 7.7,
                    Val3 = "e"
                }),
                ($"test05", new()
                {
                    Val1 = 2,
                    Val2 = 8.8,
                    Val3 = "f"
                }),
                ($"test06", new()
                {
                    Val1 = 2,
                    Val2 = 9.9,
                    Val3 = "g"
                }),
                ($"test07", new()
                {
                    Val1 = 2,
                    Val2 = 10.1,
                    Val3 = "h"
                }),
                ($"test08", new()
                {
                    Val1 = 2,
                    Val2 = 10.11,
                    Val3 = "i"
                }),
                ($"test09", new()
                {
                    Val1 = long.MaxValue,
                    Val2 = 10.12,
                    Val3 = "j"
                })
            });
        Assert.NotNull(writeDocuments.Result);
        Assert.Equal(10, writeDocuments.Result.Found.Count);

        var docs = writeDocuments.Result.Found.Select(i => i.Document).OrderBy(i => i.Reference.Id).ToArray();

        var queryResponse1 = await testCollectionReference.Query<MixedModel>()
            .Ascending(nameof(MixedModel.Val3))
            .AscendingDocumentName()
            .StartAt(docs[5].Model.Val3)
            .StartAt(docs[5])
            .RunQuery();
        Assert.NotNull(queryResponse1.Result);

        var docs1 = queryResponse1.Result.Documents;
        var transaction1 = await queryResponse1.GetTransactionContentsAsString();

        var queryResponse2 = await testCollectionReference.Query<MixedModel>()
            .Descending(nameof(MixedModel.Val3))
            .DescendingDocumentName()
            .StartAt(docs[5].Model.Val3)
            .StartAt(docs[5])
            .EndAt(docs[1].Model.Val3)
            .EndAt(docs[1])
            .RunQuery();
        Assert.NotNull(queryResponse2.Result);

        var docs2 = queryResponse2.Result.Documents;
        var transaction2 = await queryResponse2.GetTransactionContentsAsString();

        Assert.True(true);
    }
}
