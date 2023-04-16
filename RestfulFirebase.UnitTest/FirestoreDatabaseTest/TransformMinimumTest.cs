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

public class TransformMinimumTest
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
            .Collection(nameof(TransformMinimumTest));

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        var writeResult = await model1Reference.PatchAndGetDocument(new NumberModel()
        {
            Val1 = 3,
            Val2 = 3,
        });

        var writeTest1Model1 = writeResult.Result?.Found?.Document;

        Assert.NotNull(writeTest1Model1?.Model);

        var transformTest1 = await model1Reference.Transform<NumberModel>()
            .PropertyMinimum(2, nameof(NumberModel.Val1))
            .Cache(writeTest1Model1)
            .RunAndGet();
        transformTest1.ThrowIfError();

        Assert.Equal(2, writeTest1Model1.Model.Val1);

        var transformTest2 = await model1Reference.Transform<NumberModel>()
            .PropertyMinimum(1.5, nameof(NumberModel.Val2))
            .Cache(writeTest1Model1)
            .RunAndGet();
        transformTest2.ThrowIfError();

        Assert.Equal(1.5, writeTest1Model1.Model.Val2);

        var transformTest3 = await model1Reference.Transform<NumberModel>()
            .PropertyMinimum(1, nameof(NumberModel.Val1))
            .PropertyMinimum(1, nameof(NumberModel.Val2))
            .Cache(writeTest1Model1)
            .RunAndGet();
        transformTest3.ThrowIfError();

        Assert.Equal(1, writeTest1Model1.Model.Val1);
        Assert.Equal(1, writeTest1Model1.Model.Val2);

        var transformTest4 = await model1Reference.Transform<NumberModel>()
            .PropertyMinimum(3, nameof(NumberModel.Val1))
            .Cache(writeTest1Model1)
            .RunAndGet();
        transformTest4.ThrowIfError();

        Assert.Equal(1, writeTest1Model1.Model.Val1);

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        Assert.True(true);
    }
}
