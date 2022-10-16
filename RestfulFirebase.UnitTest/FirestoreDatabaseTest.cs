using RestfulFirebase.FirestoreDatabase.References;
using System.Collections.Generic;
using Xunit;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Queries;
using System.Threading.Tasks;
using RestfulFirebase.FirestoreDatabase.Transform;
using System;
using RestfulFirebase.FirestoreDatabase.Enums;

namespace RestfulFirebase.UnitTest;

public class FirestoreDatabaseTest
{
    internal static async Task Cleanup(CollectionReference collectionReference)
    {
        var oldDataList = await collectionReference.Query()
            .RunQuery();
        Assert.NotNull(oldDataList.Result);

        List<Document> oldDocs = new();

        await foreach (var page in oldDataList.Result)
        {
            page.ThrowIfError();
            foreach (var doc in page.Result.Documents)
            {
                oldDocs.Add(doc.Document);
            }
        }
        var cleanups = await collectionReference.DeleteDocuments(oldDocs.Select(i => i.Reference.Id));
        cleanups.ThrowIfError();
    }

    [Fact]
    public async void QueryDocument()
    {
        FirebaseApp app = Helpers.GetFirebaseApp();

        CollectionReference testCollectionReference1 = app.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(QueryDocument));
        CollectionReference testCollectionReference2 = app.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(QueryDocument) + "1");

        await Cleanup(testCollectionReference1);
        await Cleanup(testCollectionReference2);

        var writeDocuments1 = await testCollectionReference1.PatchAndGetDocuments(
            new (string, MixedModel?)[]
            {
                ($"mixedTest00", new()
                {
                    Val1 = long.MinValue,
                    Val2 = 3.3,
                    Val3 = null
                }),
                ($"mixedTest01", new()
                {
                    Val1 = 1,
                    Val2 = 4.4,
                    Val3 = "b"
                }),
                ($"mixedTest02", new()
                {
                    Val1 = 1,
                    Val2 = 5.5,
                    Val3 = "c"
                }),
                ($"mixedTest03", new()
                {
                    Val1 = 2,
                    Val2 = 6.6,
                    Val3 = "d"
                }),
                ($"mixedTest04", new()
                {
                    Val1 = 2,
                    Val2 = 7.7,
                    Val3 = "e"
                }),
                ($"mixedTest05", new()
                {
                    Val1 = 2,
                    Val2 = 8.8,
                    Val3 = "f"
                }),
                ($"mixedTest06", new()
                {
                    Val1 = 2,
                    Val2 = 9.9,
                    Val3 = "g"
                }),
                ($"mixedTest07", new()
                {
                    Val1 = 2,
                    Val2 = 10.1,
                    Val3 = "h"
                }),
                ($"mixedTest08", new()
                {
                    Val1 = 2,
                    Val2 = 10.11,
                    Val3 = "i"
                }),
                ($"mixedTest09", new()
                {
                    Val1 = long.MaxValue,
                    Val2 = 10.12,
                    Val3 = "j"
                })
            });
        Assert.NotNull(writeDocuments1.Result);
        Assert.Equal(10, writeDocuments1.Result.Found.Count);
        var testDocs1 = writeDocuments1.Result.Found.Select(i => i.Document).OrderBy(i => i.Reference.Id).ToArray();
        Assert.Equal(10, testDocs1.Length);

        var writeDocuments2 = await testCollectionReference2.PatchAndGetDocuments(
            new (string, ArrayModel?)[]
            {
                ($"arrayTest00", new()
                {
                    Val1 = new int[]{ 1, 2, 3 }
                }),
                ($"arrayTest01", new()
                {
                    Val1 = new int[]{ 3, 4, 5 }
                }),
            });
        Assert.NotNull(writeDocuments2.Result);
        Assert.Equal(2, writeDocuments2.Result.Found.Count);
        var testDocs2 = writeDocuments2.Result.Found.Select(i => i.Document).OrderBy(i => i.Reference.Id).ToArray();
        Assert.Equal(2, testDocs2.Length);

        var queryResponse1 = await testCollectionReference1.Query<MixedModel>()
            .Select(nameof(MixedModel.Val2))
            .PageSize(2)
            .RunQuery();

        var transaction1 = await queryResponse1.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse1.Result);
        var docs1 = queryResponse1.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(2, docs1.Length);
        Assert.Equal(0, docs1[0].Model?.Val1);
        Assert.Equivalent(docs1[0].Model?.Val2, testDocs1[0].Model?.Val2);
        Assert.Null(docs1[0].Model?.Val3);
        Assert.Equal(0, docs1[1].Model?.Val1);
        Assert.Equivalent(docs1[1].Model?.Val2, testDocs1[1].Model?.Val2);
        Assert.Null(docs1[1].Model?.Val3);

        var queryResponse2 = await testCollectionReference1.Query<MixedModel>()
            .Where(nameof(MixedModel.Val1), FieldOperator.Equal, 1)
            .RunQuery();

        var transaction2 = await queryResponse2.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse2.Result);
        var docs2 = queryResponse2.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(2, docs2.Length);
        Assert.Equivalent(docs2[0], testDocs1[1]);
        Assert.Equivalent(docs2[1], testDocs1[2]);

        var queryResponse3 = await testCollectionReference1.Query<MixedModel>()
            .Where(nameof(MixedModel.Val1), FieldOperator.In, new object[] { 1, 2 })
            .RunQuery();

        var transaction3 = await queryResponse3.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse3.Result);
        var docs3 = queryResponse3.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(8, docs3.Length);
        Assert.Equivalent(docs3[0], testDocs1[1]);
        Assert.Equivalent(docs3[1], testDocs1[2]);
        Assert.Equivalent(docs3[2], testDocs1[3]);
        Assert.Equivalent(docs3[3], testDocs1[4]);
        Assert.Equivalent(docs3[4], testDocs1[5]);
        Assert.Equivalent(docs3[5], testDocs1[6]);
        Assert.Equivalent(docs3[6], testDocs1[7]);
        Assert.Equivalent(docs3[7], testDocs1[8]);

        var queryResponse4 = await testCollectionReference1.Query<MixedModel>()
            .Where(nameof(MixedModel.Val3), UnaryOperator.IsNull)
            .RunQuery();

        var transaction4 = await queryResponse4.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse4.Result);
        var docs4 = queryResponse4.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Single(docs4);
        Assert.Equivalent(docs4[0], testDocs1[0]);

        var queryResponse5 = await testCollectionReference2.Query<ArrayModel>()
            .Where(nameof(ArrayModel.Val1), FieldOperator.ArrayContains, 3)
            .RunQuery();

        var transaction5 = await queryResponse5.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse5.Result);
        var docs5 = queryResponse5.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(2, docs5.Length);
        Assert.Equivalent(docs5, testDocs2);

        var queryResponse6 = await testCollectionReference2.Query<ArrayModel>()
            .Where(nameof(ArrayModel.Val1), FieldOperator.ArrayContainsAny, new object[] { 1, 5 })
            .RunQuery();

        var transaction6 = await queryResponse6.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse6.Result);
        var docs6 = queryResponse6.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(2, docs6.Length);
        Assert.Equivalent(docs6, testDocs2);

        var queryResponse7 = await testCollectionReference1.Query<MixedModel>()
            .Ascending(nameof(MixedModel.Val1))
            .AscendingDocumentName()
            .RunQuery();

        var transaction7 = await queryResponse7.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse7.Result);
        var docs7 = queryResponse7.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(10, docs7.Length);
        Assert.Equivalent(docs7[0], testDocs1[0]);
        Assert.Equivalent(docs7[1], testDocs1[1]);
        Assert.Equivalent(docs7[2], testDocs1[2]);
        Assert.Equivalent(docs7[3], testDocs1[3]);
        Assert.Equivalent(docs7[4], testDocs1[4]);
        Assert.Equivalent(docs7[5], testDocs1[5]);
        Assert.Equivalent(docs7[6], testDocs1[6]);
        Assert.Equivalent(docs7[7], testDocs1[7]);
        Assert.Equivalent(docs7[8], testDocs1[8]);
        Assert.Equivalent(docs7[9], testDocs1[9]);

        var queryResponse8 = await testCollectionReference1.Query<MixedModel>()
            .Ascending(nameof(MixedModel.Val1))
            .AscendingDocumentName()
            .StartAt(2)
            .StartAt(testDocs1[5])
            .RunQuery();

        var transaction8 = await queryResponse8.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse8.Result);
        var docs8 = queryResponse8.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(5, docs8.Length);
        Assert.Equivalent(docs8[0], testDocs1[5]);
        Assert.Equivalent(docs8[1], testDocs1[6]);
        Assert.Equivalent(docs8[2], testDocs1[7]);
        Assert.Equivalent(docs8[3], testDocs1[8]);
        Assert.Equivalent(docs8[4], testDocs1[9]);

        var queryResponse9 = await testCollectionReference1.Query<MixedModel>()
            .Ascending(nameof(MixedModel.Val1))
            .AscendingDocumentName()
            .StartAfter(testDocs1[5])
            .RunQuery();

        var transaction9 = await queryResponse9.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse9.Result);
        var docs9 = queryResponse9.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(4, docs9.Length);
        Assert.Equivalent(docs9[0], testDocs1[6]);
        Assert.Equivalent(docs9[1], testDocs1[7]);
        Assert.Equivalent(docs9[2], testDocs1[8]);
        Assert.Equivalent(docs9[3], testDocs1[9]);

        var queryResponse10 = await testCollectionReference1.Query<MixedModel>()
            .AscendingDocumentName()
            .StartAt(testDocs1[5])
            .RunQuery();

        var transaction10 = await queryResponse10.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse10.Result);
        var docs10 = queryResponse10.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(5, docs10.Length);
        Assert.Equivalent(docs10[0], testDocs1[5]);
        Assert.Equivalent(docs10[1], testDocs1[6]);
        Assert.Equivalent(docs10[2], testDocs1[7]);
        Assert.Equivalent(docs10[3], testDocs1[8]);
        Assert.Equivalent(docs10[4], testDocs1[9]);

        var queryResponse11 = await testCollectionReference1.Query<MixedModel>()
            .StartAfter(testDocs1[5])
            .RunQuery();

        var transaction11 = await queryResponse11.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse11.Result);
        var docs11 = queryResponse11.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(4, docs11.Length);
        Assert.Equivalent(docs11[0], testDocs1[6]);
        Assert.Equivalent(docs11[1], testDocs1[7]);
        Assert.Equivalent(docs11[2], testDocs1[8]);
        Assert.Equivalent(docs11[3], testDocs1[9]);

        var queryResponse12 = await testCollectionReference1.Query<MixedModel>()
            .StartAfter(testDocs1[5])
            .EndAt(testDocs1[8])
            .RunQuery();

        var transaction12 = await queryResponse12.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse12.Result);
        var docs12 = queryResponse12.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(3, docs12.Length);
        Assert.Equivalent(docs12[0], testDocs1[6]);
        Assert.Equivalent(docs12[1], testDocs1[7]);
        Assert.Equivalent(docs12[2], testDocs1[8]);

        var queryResponse13 = await testCollectionReference1.Query<MixedModel>()
            .StartAfter(testDocs1[5])
            .EndBefore(testDocs1[9])
            .RunQuery();

        var transaction13 = await queryResponse13.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse13.Result);
        var docs13 = queryResponse13.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(3, docs13.Length);
        Assert.Equivalent(docs13[0], testDocs1[6]);
        Assert.Equivalent(docs13[1], testDocs1[7]);
        Assert.Equivalent(docs13[2], testDocs1[8]);

        var queryResponse14 = await testCollectionReference1.Query<MixedModel>()
            .Ascending(nameof(MixedModel.Val1))
            .Descending(nameof(MixedModel.Val2))
            .RunQuery();

        var transaction14 = await queryResponse14.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse14.Result);
        var docs14 = queryResponse14.Result.Documents.Select(i => i.Document).ToArray();
        Assert.Equal(10, docs14.Length);
        Assert.Equivalent(docs14[0], testDocs1[0]);
        Assert.Equivalent(docs14[1], testDocs1[2]);
        Assert.Equivalent(docs14[2], testDocs1[1]);
        Assert.Equivalent(docs14[3], testDocs1[8]);
        Assert.Equivalent(docs14[4], testDocs1[7]);
        Assert.Equivalent(docs14[5], testDocs1[6]);
        Assert.Equivalent(docs14[6], testDocs1[5]);
        Assert.Equivalent(docs14[7], testDocs1[4]);
        Assert.Equivalent(docs14[8], testDocs1[3]);
        Assert.Equivalent(docs14[9], testDocs1[9]);

        var queryResponse15 = await testCollectionReference1.Query<MixedModel>()
            .Ascending(nameof(MixedModel.Val1))
            .Descending(nameof(MixedModel.Val2))
            .PageSize(2)
            .RunQuery();

        var transaction15 = await queryResponse15.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse15.Result);
        List<Document<MixedModel>> docs15 = new();

        int pages15 = 0;
        await foreach (var response in queryResponse15.Result)
        {
            var t = await response.GetTransactionContentsAsString();
            Assert.NotNull(response.Result);
            var d = response.Result.Documents.Select(i => i.Document).ToArray();
            docs15.AddRange(d);
            Assert.True(true);
            pages15++;
        }

        Assert.Equal(5, pages15);
        Assert.Equivalent(docs14, docs15);

        var queryResponse16 = await testCollectionReference1.Query<MixedModel>()
            .Ascending(nameof(MixedModel.Val1))
            .Descending(nameof(MixedModel.Val2))
            .EndAt(testDocs1[4])
            .PageSize(2)
            .RunQuery();

        var transaction16 = await queryResponse16.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse16.Result);
        List<Document<MixedModel>> docs16 = new();

        int pages16 = 0;
        await foreach (var response in queryResponse16.Result)
        {
            var t = await response.GetTransactionContentsAsString();
            Assert.NotNull(response.Result);
            var d = response.Result.Documents.Select(i => i.Document).ToArray();
            docs16.AddRange(d);
            Assert.True(true);
            pages16++;
        }

        Assert.Equal(4, pages16);
        Assert.Equivalent(docs16[0], testDocs1[0]);
        Assert.Equivalent(docs16[1], testDocs1[2]);
        Assert.Equivalent(docs16[2], testDocs1[1]);
        Assert.Equivalent(docs16[3], testDocs1[8]);
        Assert.Equivalent(docs16[4], testDocs1[7]);
        Assert.Equivalent(docs16[5], testDocs1[6]);
        Assert.Equivalent(docs16[6], testDocs1[5]);
        Assert.Equivalent(docs16[7], testDocs1[4]);

        var queryResponse17 = await testCollectionReference1.Query<MixedModel>()
            .Ascending(nameof(MixedModel.Val1))
            .Descending(nameof(MixedModel.Val2))
            .PageSize(2)
            .SkipPage(3)
            .RunQuery();

        var transaction17 = await queryResponse17.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse17.Result);
        List<Document<MixedModel>> docs17 = new();

        int pages17 = 0;
        await foreach (var response in queryResponse17.Result)
        {
            var t = await response.GetTransactionContentsAsString();
            Assert.NotNull(response.Result);
            var d = response.Result.Documents.Select(i => i.Document).ToArray();
            docs17.AddRange(d);
            Assert.True(true);
            pages17++;
        }

        Assert.Equal(2, pages17);
        Assert.Equal(4, docs17.Count);
        Assert.Equivalent(docs17[0], testDocs1[5]);
        Assert.Equivalent(docs17[1], testDocs1[4]);
        Assert.Equivalent(docs17[2], testDocs1[3]);
        Assert.Equivalent(docs17[3], testDocs1[9]);

        var queryResponse18 = await testCollectionReference1.Query<MixedModel>()
            .Ascending(nameof(MixedModel.Val1))
            .Descending(nameof(MixedModel.Val2))
            .EndAt(testDocs1[3])
            .PageSize(2)
            .SkipPage(3)
            .RunQuery();

        var transaction18 = await queryResponse18.GetTransactionContentsAsString();
        Assert.NotNull(queryResponse18.Result);
        List<Document<MixedModel>> docs18 = new();

        int pages18 = 0;
        await foreach (var response in queryResponse18.Result)
        {
            var t = await response.GetTransactionContentsAsString();
            Assert.NotNull(response.Result);
            var d = response.Result.Documents.Select(i => i.Document).ToArray();
            docs18.AddRange(d);
            Assert.True(true);
            pages18++;
        }

        Assert.Equal(2, pages18);
        Assert.Equal(3, docs18.Count);
        Assert.Equivalent(docs18[0], testDocs1[5]);
        Assert.Equivalent(docs18[1], testDocs1[4]);
        Assert.Equivalent(docs18[2], testDocs1[3]);

        await Cleanup(testCollectionReference1);
        await Cleanup(testCollectionReference2);

        Assert.True(true);
    }
}
