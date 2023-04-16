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

public class TransformAppendMissingElementsTest
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
            .Collection(nameof(TransformAppendMissingElementsTest));

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        var model1Reference = testCollectionReference.Document("model1");

        var writeResult = await model1Reference.PatchAndGetDocument(new ArrayModel()
        {
            Val1 = new int[] { 1, 2, 3, 4, 5 }
        });

        var writeTest1Model1 = writeResult.Result?.Found?.Document;

        Assert.NotNull(writeTest1Model1?.Model);

        var transformTest1 = await model1Reference.Transform<ArrayModel>()
            .PropertyAppendMissingElements(new object[] { 6, 7 }, nameof(ArrayModel.Val1))
            .Cache(writeTest1Model1)
            .RunAndGet();
        transformTest1.ThrowIfError();

        Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 7 }, writeTest1Model1.Model.Val1);

        var transformTest2 = await model1Reference.Transform<ArrayModel>()
            .PropertyAppendMissingElements(new object[] { 7, 8 }, nameof(ArrayModel.Val1))
            .Cache(writeTest1Model1)
            .RunAndGet();
        transformTest2.ThrowIfError();

        Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, writeTest1Model1.Model.Val1);

        var transformTest3 = await model1Reference.Transform<ArrayModel>()
            .PropertyAppendMissingElements(new object[] { 1, 2 }, nameof(ArrayModel.Val1))
            .Cache(writeTest1Model1)
            .RunAndGet();
        transformTest3.ThrowIfError();

        Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, writeTest1Model1.Model.Val1);

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        Assert.True(true);
    }
}
