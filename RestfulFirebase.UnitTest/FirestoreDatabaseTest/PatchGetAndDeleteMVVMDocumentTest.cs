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

public class PatchGetAndDeleteMVVMDocumentTest
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
            .Collection(nameof(PatchGetAndDeleteMVVMDocumentTest));

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        var writeResult = await model1Reference.PatchAndGetDocument(new MVVMModelWithIncludeOnlyAttribute()
        {
            Val1 = "test val 1",
            Val2 = "test val 2",
            Val3 = "test val 3",
        });

        var writeTest1Model1 = writeResult.Result?.Found?.Document;

        Assert.NotNull(writeTest1Model1?.Model);

        List<string?> documentPropertyChangedNames = new();
        writeTest1Model1.PropertyChanged += (s, e) =>
        {
            documentPropertyChangedNames.Add(e.PropertyName);
        };

        var patchAndGetTest = await model1Reference.PatchAndGetDocument(new MVVMModelWithIncludeOnlyAttribute()
        {
            Val1 = "another test val 1",
            Val2 = "another test val 2",
            Val3 = "another test val 3",
        }, new Document[] { writeTest1Model1 });
        Assert.NotNull(patchAndGetTest.Result?.Found);

        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.Model), documentPropertyChangedNames);
        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.Reference), documentPropertyChangedNames);
        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.CreateTime), documentPropertyChangedNames);
        Assert.Contains(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.UpdateTime), documentPropertyChangedNames);
        Assert.Equal(writeTest1Model1, patchAndGetTest.Result.Found.Document);

        await FirestoreDatabaseHelpers.Cleanup(testCollectionReference);

        Assert.True(true);
    }
}
