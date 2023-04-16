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

public class CreateGetDeleteTest
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
            .Collection(nameof(CreateGetDeleteTest));

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        NormalMVVMModel model1 = new()
        {
            Val1 = "1test1",
            Val2 = "1test2"
        };

        NormalMVVMModel model2 = new()
        {
            Val1 = "2test1",
            Val2 = "2test2"
        };

        var createTest1 = await app.FirestoreDatabase.Write()
            .Create(model1, testCollectionReference)
            .Create(model2, testCollectionReference, $"{nameof(FirestoreDatabaseTest)}{nameof(CreateGetDeleteTest)}documentIdSample")
            .RunAndGet<NormalMVVMModel>();
        Assert.NotNull(createTest1.Result);
        Assert.Equal(2, createTest1.Result.Found.Count);

        var getTest1 = await testCollectionReference.Query<NormalMVVMModel>()
            .Run();
        Assert.NotNull(getTest1.Result);
        Assert.Equal(2, getTest1.Result.Documents.Count);

        var orderedCreate = createTest1.Result.Found.Select(i => i.Document).OrderBy(i => i.Name);
        var orderedGet = getTest1.Result.Documents.Select(i => i.Document).OrderBy(i => i.Name);

        Assert.Equivalent(orderedCreate, orderedGet);

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        Assert.True(true);
    }
}
