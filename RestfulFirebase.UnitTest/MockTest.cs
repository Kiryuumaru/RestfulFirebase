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
            ($"test1", new()
            {
                Val1 = 1,
                Val2 = 3.3
            }),
            ($"test2", new()
            {
                Val1 = 1,
                Val2 = 4.4
            }),
            ($"test3", new()
            {
                Val1 = 2,
                Val2 = 5.5
            }),
            ($"test4", new()
            {
                Val1 = 2,
                Val2 = 6.6
            }),
            ($"test5", new()
            {
                Val1 = 2,
                Val2 = 7.7
            }),
            ($"test6", new()
            {
                Val1 = 2,
                Val2 = 8.8
            }),
            ($"test7", new()
            {
                Val1 = 2,
                Val2 = 9.9
            }),
            ($"test8", new()
            {
                Val1 = 2,
                Val2 = 10.1
            }),
            ($"test9", new()
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
                .Ascending(nameof(NumberModel.Val1))
                .Descending(nameof(NumberModel.Val2)),
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

        Assert.Equal(5, iteration);
        Assert.Equal(10, docs.Count);
        Assert.Equivalent(writeDocuments[0], docs[1].Document);
        Assert.Equivalent(writeDocuments[1], docs[0].Document);
        Assert.Equivalent(writeDocuments[2], docs[9].Document);
        Assert.Equivalent(writeDocuments[3], docs[8].Document);
        Assert.Equivalent(writeDocuments[4], docs[7].Document);
        Assert.Equivalent(writeDocuments[5], docs[6].Document);
        Assert.Equivalent(writeDocuments[6], docs[5].Document);
        Assert.Equivalent(writeDocuments[7], docs[4].Document);
        Assert.Equivalent(writeDocuments[8], docs[3].Document);
        Assert.Equivalent(writeDocuments[9], docs[2].Document);

        Assert.True(true);
    }
}
