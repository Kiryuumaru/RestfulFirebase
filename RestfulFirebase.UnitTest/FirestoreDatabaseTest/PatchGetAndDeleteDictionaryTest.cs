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

public class PatchGetAndDeleteDictionaryTest
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
            .Collection(nameof(PatchGetAndDeleteDictionaryTest));

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Document<Dictionary<string, NestedType>> dictionaryNull = new(model1Reference, null);
        Document<Dictionary<string, NestedType>> dictionaryEmpty = new(model1Reference, new Dictionary<string, NestedType>());
        Document<Dictionary<string, NestedType>> dictionary1 = new(model1Reference, new Dictionary<string, NestedType>()
        {
            { "item1", NestedType.Filled1(app) },
            { "item2", NestedType.Filled2(app) },
            { "item3", NestedType.Empty(app) },
        });
        Document<Dictionary<string, NestedType>> dictionary2 = new(model1Reference, new Dictionary<string, NestedType>()
        {
            { "anotherItem1", NestedType.Filled2(app) },
            { "anotherItem2", NestedType.Filled1(app) },
        });

        var writeTest1 = await model1Reference.PatchAndGetDocument(dictionary1.Model, new Document[] { dictionary1 });
        Assert.NotNull(writeTest1.Result?.Found);

        var getTest1 = await model1Reference.GetDocument(new Document[] { dictionaryNull });
        Assert.NotNull(getTest1.Result?.Found);

        var getTest2 = await model1Reference.GetDocument(new Document[] { dictionaryEmpty });
        Assert.NotNull(getTest2.Result?.Found);

        var getTest3 = await model1Reference.GetDocument(new Document[] { dictionary2 });
        Assert.NotNull(getTest3.Result?.Found);

        Assert.Equivalent(dictionary1, getTest1.Result.Found.Document);
        Assert.Equivalent(dictionary1, getTest2.Result.Found.Document);
        Assert.Equivalent(dictionary1, getTest3.Result.Found.Document);

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        Assert.True(true);
    }
}
