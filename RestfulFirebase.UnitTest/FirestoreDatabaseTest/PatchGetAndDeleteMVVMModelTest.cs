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

public class PatchGetAndDeleteMVVMModelTest
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
            .Collection(nameof(PatchGetAndDeleteMVVMModelTest));

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        MVVMModelWithIncludeOnlyAttribute model1 = new()
        {
            Val1 = "test val 1",
            Val2 = "test val 2",
            Val3 = "test val 3",
        };
        MVVMModelWithIncludeOnlyAttribute model2 = new()
        {
            Val1 = "test val 4",
            Val2 = "test val 5",
            Val3 = "test val 6",
        };

        List<string?> model2PropertyChangedNames = new();
        model2.PropertyChanged += (s, e) =>
        {
            model2PropertyChangedNames.Add(e.PropertyName);
        };

        var writeAndGetResult1 = await model1Reference.PatchAndGetDocument(model1, new Document[] { new Document<MVVMModelWithIncludeOnlyAttribute>(model1Reference, model2) });

        Assert.NotNull(writeAndGetResult1.Result);
        Assert.Contains(nameof(MVVMModelWithIncludeOnlyAttribute.Val1), model2PropertyChangedNames);
        Assert.Contains(nameof(MVVMModelWithIncludeOnlyAttribute.Val2), model2PropertyChangedNames);
        Assert.DoesNotContain(nameof(MVVMModelWithIncludeOnlyAttribute.Val3), model2PropertyChangedNames);
        Assert.NotEqual(model1.Val3, model2.Val3);
        if (writeAndGetResult1.Result?.Found?.Document?.Model is MVVMModelWithIncludeOnlyAttribute model)
        {
            model.Val3 = model1.Val3;
        }
        Assert.Equivalent(model1, model2);

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        Assert.True(true);
    }
}
