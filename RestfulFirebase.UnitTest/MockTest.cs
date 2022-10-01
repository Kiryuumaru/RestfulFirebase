using Xunit;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase.Requests;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.Common.Models;
using System.Collections.Generic;

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

        Document<NumberModel>[] writeDocuments = testCollectionReference.CreateDocuments<NumberModel>(
            ($"test01", new()
            {
                Val1 = 1,
                Val2 = 3.3
            }),
            ($"test02", new()
            {
                Val1 = 1,
                Val2 = 4.4
            }),
            ($"test03", new()
            {
                Val1 = 2,
                Val2 = 5.5
            }),
            ($"test04", new()
            {
                Val1 = 2,
                Val2 = 6.6
            }),
            ($"test05", new()
            {
                Val1 = 2,
                Val2 = 7.7
            }),
            ($"test06", new()
            {
                Val1 = 2,
                Val2 = 8.8
            }),
            ($"test07", new()
            {
                Val1 = 2,
                Val2 = 9.9
            }),
            ($"test08", new()
            {
                Val1 = 2,
                Val2 = 10.1
            }),
            ($"test09", new()
            {
                Val1 = 2,
                Val2 = 10.11
            }),
            ($"test10", new()
            {
                Val1 = 2,
                Val2 = 10.12
            }));

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            PatchDocument = writeDocuments
        });

        var runQueryTest1 = await Api.FirestoreDatabase.QueryDocument(new QueryDocumentRequest<NumberModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = writeDocuments,
            From = testCollectionReference,
            OrderBy = OrderByQuery
                .AscendingDocumentName(),
            StartAt = CursorQuery
                .Add(writeDocuments[5].Reference),
            PageSize = 2
        });
        runQueryTest1.ThrowIfError();

        int iteration = 0;
        List<DocumentTimestamp<NumberModel>> docs = new();
        await foreach(var page in runQueryTest1.Result)
        {
            page.ThrowIfError();
            iteration++;
            docs.AddRange(page.Result.Documents);
        }

        Assert.Equal(3, iteration);
        Assert.Equal(5, docs.Count);
        Assert.Equivalent(docs[0].Document, writeDocuments[5]);
        Assert.Equivalent(docs[1].Document, writeDocuments[6]);
        Assert.Equivalent(docs[2].Document, writeDocuments[7]);
        Assert.Equivalent(docs[3].Document, writeDocuments[8]);
        Assert.Equivalent(docs[4].Document, writeDocuments[9]);

        Assert.True(true);
    }
}
