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

public class PatchGetAndDeleteModelTest
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
            .Collection(nameof(PatchGetAndDeleteModelTest));

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Document<NestedType> writeTest1ModelNull = new(model1Reference, null);
        Document<NestedType> writeTest1ModelEmpty = new(model1Reference, NestedType.Empty(app));
        Document<NestedType> writeTest1ModelFilled1 = new(model1Reference, NestedType.Filled1(app));
        Document<NestedType> writeTest1ModelFilled2 = new(model1Reference, NestedType.Filled2(app));

        var writeTest1 = await model1Reference.PatchAndGetDocument(writeTest1ModelFilled1.Model, new Document[] { writeTest1ModelFilled1 });
        Assert.NotNull(writeTest1.Result?.Found);

        var getTest1 = await model1Reference.GetDocument(new Document[] { writeTest1ModelNull });
        Assert.NotNull(getTest1.Result?.Found);

        var getTest2 = await model1Reference.GetDocument(new Document[] { writeTest1ModelEmpty });
        Assert.NotNull(getTest2.Result?.Found);

        var getTest3 = await model1Reference.GetDocument(new Document[] { writeTest1ModelFilled2 });
        Assert.NotNull(getTest3.Result?.Found);

        Assert.Equivalent(writeTest1ModelFilled1, getTest1.Result.Found.Document);
        Assert.Equivalent(writeTest1ModelFilled1, getTest2.Result.Found.Document);
        Assert.Equivalent(writeTest1ModelFilled1, getTest3.Result.Found.Document);

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        Assert.True(true);
    }
}
