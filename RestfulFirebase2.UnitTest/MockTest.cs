using Xunit;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase.Requests;
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
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(FirestoreDatabaseTest.QueryDocument));

        await FirestoreDatabaseTest.Cleanup(config, testCollectionReference);

        Document<MixedModel>[] writeDocuments = testCollectionReference.CreateDocuments<MixedModel>(
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
                Val1 = 2,
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
            }));

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            PatchDocument = writeDocuments
        });

        List<Document<MixedModel>> docs = new();

        var runQueryTest1 = await Api.FirestoreDatabase.QueryDocument(new QueryDocumentRequest<MixedModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = writeDocuments,
            From = testCollectionReference,
            Order = OrderQuery
                .Ascending(nameof(MixedModel.Val1))
                .Descending(nameof(MixedModel.Val2))
                .DescendingDocumentName().StartAt(writeDocuments[5].Reference),
        });
        runQueryTest1.ThrowIfError();
        docs.AddRange(runQueryTest1.Result.Documents.Select(i => i.Document));

        //int iteration = 0;
        //await foreach (var page in runQueryTest1.Result)
        //{
        //    page.ThrowIfError();
        //    iteration++;
        //    docs.AddRange(page.Result.Documents.Select(i => i.Document));
        //}

        //Assert.Equal(3, iteration);
        Assert.Equal(5, docs.Count);
        Assert.Equivalent(docs[0], writeDocuments[5]);
        Assert.Equivalent(docs[1], writeDocuments[6]);
        Assert.Equivalent(docs[2], writeDocuments[7]);
        Assert.Equivalent(docs[3], writeDocuments[8]);
        Assert.Equivalent(docs[4], writeDocuments[9]);

        Assert.True(true);
    }
}
