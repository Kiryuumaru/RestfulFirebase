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

public class TransformSetToServerValueTest
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
            .Collection(nameof(TransformSetToServerValueTest));

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");
        DocumentReference model2Reference = testCollectionReference.Document("model2");

        var writeResult1 = await model1Reference.PatchAndGetDocument(new TimestampModel());
        var writeResult2 = await model2Reference.PatchAndGetDocument(new TimestampModel());
        var writeResult3 = await model2Reference.PatchAndGetDocument(new TimestampModel());

        var writeTest1Model1 = writeResult1.Result?.Found?.Document;
        var writeTest1Model2 = writeResult2.Result?.Found?.Document;
        var writeTest1Model3 = writeResult3.Result?.Found?.Document;

        Assert.NotNull(writeTest1Model1?.Model);
        Assert.NotNull(writeTest1Model2?.Model);
        Assert.NotNull(writeTest1Model3?.Model);

        var transformTest1 = await app.FirestoreDatabase.Write()
            .Transform<TimestampModel>(model1Reference)
            .PropertySetToServerRequestTime(nameof(TimestampModel.Val1))
            .PropertySetToServerRequestTime(nameof(TimestampModel.Val2))
            .Transform<TimestampModel>(model2Reference)
            .PropertySetToServerRequestTime(nameof(TimestampModel.Val1))
            .PropertySetToServerRequestTime(nameof(TimestampModel.Val2))
            .Cache(writeTest1Model1, writeTest1Model2)
            .RunAndGet();

        Assert.Equal(writeTest1Model1.Model.Val1, writeTest1Model1.Model.Val2);
        Assert.Equal(writeTest1Model2.Model.Val1, writeTest1Model2.Model.Val2);
        Assert.NotEqual(writeTest1Model1.Model.Val1, writeTest1Model3.Model.Val1);
        Assert.NotEqual(writeTest1Model1.Model.Val2, writeTest1Model3.Model.Val2);
        Assert.NotEqual(writeTest1Model2.Model.Val1, writeTest1Model3.Model.Val1);
        Assert.NotEqual(writeTest1Model2.Model.Val2, writeTest1Model3.Model.Val2);

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        Assert.True(true);
    }
}
