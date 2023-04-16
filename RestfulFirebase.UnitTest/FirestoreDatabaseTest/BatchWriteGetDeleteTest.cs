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
using RestfulFirebase.UnitTest;
using RestfulFirebase;

namespace FirestoreDatabaseTest;

public class BatchWriteGetDeleteTest
{
    [Fact]
    public async void Test1()
    {
        FirebaseApp app = FirebaseHelpers.GetFirebaseApp();

        CollectionReference testCollectionReference = app.FirestoreDatabase
            .Collection("public")
            .Document(FirebaseHelpers.TestInstanceId)
            .Collection("test")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(BatchWriteGetDeleteTest));

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        Document<NormalMVVMModel>[] writeDocuments = new Document<NormalMVVMModel>[]
        {
            new(testCollectionReference.Document("test1"), new()
            {
                Val1 = "1 test 1",
                Val2 = "1 test 2"
            }),
            new(testCollectionReference.Document("test2"), new()
            {
                Val1 = "2 test 1",
                Val2 = "2 test 2"
            }),
            new(testCollectionReference.Document("test3"), new()
            {
                Val1 = "3 test 1",
                Val2 = "3 test 2"
            }),
            new(testCollectionReference.Document("test4"), new()
            {
                Val1 = "4 test 1",
                Val2 = "4 test 2"
            }),
            new(testCollectionReference.Document("test5"), new()
            {
                Val1 = "5 test 1",
                Val2 = "5 test 2"
            })
        };

        Document<NormalMVVMModel>[] emptyPropsDocuments = new Document<NormalMVVMModel>[]
        {
            new(testCollectionReference.Document("test1"), null),
            new(testCollectionReference.Document("test2"), null),
            new(testCollectionReference.Document("test3"), null),
            new(testCollectionReference.Document("test4"), null),
            new(testCollectionReference.Document("test5"), null)
        };

        var writeTest1 = await app.FirestoreDatabase.Write()
            .Patch(writeDocuments)
            .Run();
        writeTest1.ThrowIfError();

        var getTest1 = await app.FirestoreDatabase.Fetch()
            .Document(writeDocuments)
            .Run();
        var getTest2 = await app.FirestoreDatabase.Fetch()
            .Document(emptyPropsDocuments)
            .Run();

        Assert.NotNull(getTest1.Result);
        Assert.NotNull(getTest2.Result);
        Assert.Equivalent(writeDocuments, emptyPropsDocuments);
        Assert.Equivalent(writeDocuments, getTest1.Result.Found.Select(i => i.Document));
        Assert.Equivalent(writeDocuments, getTest2.Result.Found.Select(i => i.Document));

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        Assert.True(true);
    }
}
