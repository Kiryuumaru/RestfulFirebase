using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Requests;
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
    internal static async Task Cleanup(FirebaseConfig config, CollectionReference collectionReference)
    {
        var oldDataList = await Api.FirestoreDatabase.ListDocuments(new ListDocumentsRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            CollectionReference = collectionReference,
        });
        Assert.NotNull(oldDataList.Result);

        List<Document> oldDocs = new();

        await foreach (var page in oldDataList.Result.DocumentPager)
        {
            foreach (var doc in page)
            {
                oldDocs.Add(doc);
            }
        }

        var cleanups = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            Config = config,
            DeleteDocument = oldDocs
        });
        cleanups.ThrowIfError();
    }

    [Fact]
    public async void TransformAppendMissingElementsGetAndDeleteModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(TransformAppendMissingElementsGetAndDeleteModelTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Document<ArrayModel> writeTest1Model1 = model1Reference.Create(new ArrayModel()
        {
            Val1 = new int[] { 1, 2, 3, 4, 5 }
        });

        Assert.NotNull(writeTest1Model1.Model);

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = writeTest1Model1,
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<ArrayModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        var transformTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .AppendMissingElements<ArrayModel>(new object[] { 6, 7 }, nameof(ArrayModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest1.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<ArrayModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 7 }, writeTest1Model1.Model.Val1);

        var transformTest2 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .AppendMissingElements<ArrayModel>(new object[] { 7, 8 }, nameof(ArrayModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest2.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<ArrayModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, writeTest1Model1.Model.Val1);

        var transformTest3 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .AppendMissingElements<ArrayModel>(new object[] { 1, 2 }, nameof(ArrayModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest3.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<ArrayModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, writeTest1Model1.Model.Val1);

        await Cleanup(config, testCollectionReference);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create<NestedType>(),
        });
        Assert.NotNull(getTest2.Result);

        Assert.Empty(getTest2.Result.Found);
        Assert.NotEmpty(getTest2.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void TransformIncrementGetAndDeleteModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(TransformIncrementGetAndDeleteModelTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Document<NumberModel> writeTest1Model1 = model1Reference.Create(new NumberModel()
        {
            Val1 = 1,
            Val2 = 1,
        });

        Assert.NotNull(writeTest1Model1.Model);

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = writeTest1Model1,
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        var transformTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Increment<NumberModel>(1, nameof(NumberModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest1.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(2, writeTest1Model1.Model.Val1);

        var transformTest2 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Increment<NumberModel>(1.5, nameof(NumberModel.Val2))
                .DocumentTransform(model1Reference)
        });
        transformTest2.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(2.5, writeTest1Model1.Model.Val2);

        var transformTest3 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Increment<NumberModel>(1, nameof(NumberModel.Val1))
                .Increment<NumberModel>(0.5, nameof(NumberModel.Val2))
                .DocumentTransform(model1Reference)
        });
        transformTest3.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(3, writeTest1Model1.Model.Val1);
        Assert.Equal(3, writeTest1Model1.Model.Val2);

        var transformTest4 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Increment<NumberModel>(-0.5, nameof(NumberModel.Val2))
                .DocumentTransform(model1Reference)
        });
        transformTest4.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(2.5, writeTest1Model1.Model.Val2);

        await Cleanup(config, testCollectionReference);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create<NestedType>(),
        });
        Assert.NotNull(getTest2.Result);

        Assert.Empty(getTest2.Result.Found);
        Assert.NotEmpty(getTest2.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void TransformMaximumGetAndDeleteModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(TransformMaximumGetAndDeleteModelTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Document<NumberModel> writeTest1Model1 = model1Reference.Create(new NumberModel()
        {
            Val1 = 1,
            Val2 = 1,
        });

        Assert.NotNull(writeTest1Model1.Model);

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = writeTest1Model1,
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        var transformTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Maximum<NumberModel>(2, nameof(NumberModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest1.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(2, writeTest1Model1.Model.Val1);

        var transformTest2 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Maximum<NumberModel>(1.5, nameof(NumberModel.Val2))
                .DocumentTransform(model1Reference)
        });
        transformTest2.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(1.5, writeTest1Model1.Model.Val2);

        var transformTest3 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Maximum<NumberModel>(3, nameof(NumberModel.Val1))
                .Maximum<NumberModel>(3, nameof(NumberModel.Val2))
                .DocumentTransform(model1Reference)
        });
        transformTest3.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(3, writeTest1Model1.Model.Val1);
        Assert.Equal(3, writeTest1Model1.Model.Val2);

        var transformTest4 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Maximum<NumberModel>(2, nameof(NumberModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest4.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(3, writeTest1Model1.Model.Val1);

        await Cleanup(config, testCollectionReference);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create<NestedType>(),
        });
        Assert.NotNull(getTest2.Result);

        Assert.Empty(getTest2.Result.Found);
        Assert.NotEmpty(getTest2.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void TransformMinimumGetAndDeleteModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(TransformMinimumGetAndDeleteModelTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Document<NumberModel> writeTest1Model1 = model1Reference.Create(new NumberModel()
        {
            Val1 = 3,
            Val2 = 3,
        });

        Assert.NotNull(writeTest1Model1.Model);

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = writeTest1Model1,
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        var transformTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Minimum<NumberModel>(2, nameof(NumberModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest1.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(2, writeTest1Model1.Model.Val1);

        var transformTest2 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Minimum<NumberModel>(1.5, nameof(NumberModel.Val2))
                .DocumentTransform(model1Reference)
        });
        transformTest2.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(1.5, writeTest1Model1.Model.Val2);

        var transformTest3 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Minimum<NumberModel>(1, nameof(NumberModel.Val1))
                .Minimum<NumberModel>(1, nameof(NumberModel.Val2))
                .DocumentTransform(model1Reference)
        });
        transformTest3.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(1, writeTest1Model1.Model.Val1);
        Assert.Equal(1, writeTest1Model1.Model.Val2);

        var transformTest4 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .Minimum<NumberModel>(3, nameof(NumberModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest4.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(1, writeTest1Model1.Model.Val1);

        await Cleanup(config, testCollectionReference);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create<NestedType>(),
        });
        Assert.NotNull(getTest2.Result);

        Assert.Empty(getTest2.Result.Found);
        Assert.NotEmpty(getTest2.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void TransformRemoveAllFromArrayGetAndDeleteModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(TransformRemoveAllFromArrayGetAndDeleteModelTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Document<ArrayModel> writeTest1Model1 = model1Reference.Create(new ArrayModel()
        {
            Val1 = new int[] { 1, 2, 3, 4, 5 }
        });

        Assert.NotNull(writeTest1Model1.Model);

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = writeTest1Model1,
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<ArrayModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        var transformTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .RemoveAllFromArray<ArrayModel>(new object[] { 4, 5 }, nameof(ArrayModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest1.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<ArrayModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(new int[] { 1, 2, 3 }, writeTest1Model1.Model.Val1);

        var transformTest2 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .RemoveAllFromArray<ArrayModel>(new object[] { 3, 4 }, nameof(ArrayModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest2.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<ArrayModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(new int[] { 1, 2 }, writeTest1Model1.Model.Val1);

        var transformTest3 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .RemoveAllFromArray<ArrayModel>(new object[] { 5, 6 }, nameof(ArrayModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest3.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<ArrayModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Equal(new int[] { 1, 2 }, writeTest1Model1.Model.Val1);

        var transformTest4 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = FieldTransform
                .RemoveAllFromArray<ArrayModel>(new object[] { 1, 2 }, nameof(ArrayModel.Val1))
                .DocumentTransform(model1Reference)
        });
        transformTest4.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<ArrayModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        Assert.Null(writeTest1Model1.Model.Val1);

        await Cleanup(config, testCollectionReference);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create<NestedType>(),
        });
        Assert.NotNull(getTest2.Result);

        Assert.Empty(getTest2.Result.Found);
        Assert.NotEmpty(getTest2.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void TransformSetToServerValueGetAndDeleteModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(TransformSetToServerValueGetAndDeleteModelTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");
        DocumentReference model2Reference = testCollectionReference.Document("model2");

        Document<TimestampModel> writeTest1Model1 = model1Reference.Create(new TimestampModel());
        Document<TimestampModel> writeTest1Model2 = model2Reference.Create(new TimestampModel());
        Document<TimestampModel> writeTest1Model3 = model2Reference.Create(new TimestampModel());

        Assert.NotNull(writeTest1Model1.Model);
        Assert.NotNull(writeTest1Model2.Model);
        Assert.NotNull(writeTest1Model3.Model);

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = Document
                .Add(writeTest1Model1)
                .Add(writeTest1Model2),
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<TimestampModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = Document<TimestampModel>
                .Add(writeTest1Model1)
                .Add(writeTest1Model2),
        });

        var transformTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            TransformDocument = DocumentTransform
                .Add(model1Reference, FieldTransform
                    .SetToServerRequestTime<TimestampModel>(nameof(TimestampModel.Val1))
                    .SetToServerRequestTime<TimestampModel>(nameof(TimestampModel.Val2)))
                .Add(model2Reference, FieldTransform
                    .SetToServerRequestTime<TimestampModel>(nameof(TimestampModel.Val1))
                    .SetToServerRequestTime<TimestampModel>(nameof(TimestampModel.Val2)))
        });
        transformTest1.ThrowIfError();
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<TimestampModel>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = Document<TimestampModel>
                .Add(writeTest1Model1)
                .Add(writeTest1Model2),
        });

        Assert.Equal(writeTest1Model1.Model.Val1, writeTest1Model1.Model.Val2);
        Assert.Equal(writeTest1Model2.Model.Val1, writeTest1Model2.Model.Val2);
        Assert.NotEqual(writeTest1Model1.Model.Val1, writeTest1Model3.Model.Val1);
        Assert.NotEqual(writeTest1Model1.Model.Val2, writeTest1Model3.Model.Val2);
        Assert.NotEqual(writeTest1Model2.Model.Val1, writeTest1Model3.Model.Val1);
        Assert.NotEqual(writeTest1Model2.Model.Val2, writeTest1Model3.Model.Val2);

        await Cleanup(config, testCollectionReference);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create<NestedType>(),
        });
        Assert.NotNull(getTest2.Result);

        Assert.Empty(getTest2.Result.Found);
        Assert.NotEmpty(getTest2.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void RunQueryTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(RunQueryTest));

        await Cleanup(config, testCollectionReference);

        Document<NumberModel>[] writeDocuments = testCollectionReference.CreateDocuments<NumberModel>(
            ($"test1", new()
            {
                Val1 = 1,
                Val2 = 3.3
            }),
            ($"test2", new()
            {
                Val1 = 1,
                Val2 = 4.4
            }),
            ($"test3", new()
            {
                Val1 = 2,
                Val2 = 5.5
            }),
            ($"test4", new()
            {
                Val1 = 2,
                Val2 = 6.6
            }),
            ($"test5", new()
            {
                Val1 = 2,
                Val2 = 7.7
            }),
            ($"test6", new()
            {
                Val1 = 2,
                Val2 = 8.8
            }),
            ($"test7", new()
            {
                Val1 = 2,
                Val2 = 9.9
            }),
            ($"test8", new()
            {
                Val1 = 2,
                Val2 = 10.1
            }),
            ($"test9", new()
            {
                Val1 = 2,
                Val2 = 10.11
            }),
            ($"test10", new()
            {
                Val1 = 2,
                Val2 = 10.12
            }));

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            PatchDocument = writeDocuments
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = writeDocuments
        });

        var runQueryTest1 = await Api.FirestoreDatabase.RunQuery(new RunQueryRequest<NumberModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            From = testCollectionReference,
            Where = FilterQuery
                .Field(nameof(NumberModel.Val1), FieldOperator.Equal, 1),
        });
        Assert.NotNull(runQueryTest1.Result);

        Assert.Equal(2, runQueryTest1.Result.Found.Count);
        Assert.Equivalent(writeDocuments[0], runQueryTest1.Result.Found[0].Document);
        Assert.Equivalent(writeDocuments[1], runQueryTest1.Result.Found[1].Document);

        var runQueryTest2 = await Api.FirestoreDatabase.RunQuery(new RunQueryRequest<NumberModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            From = testCollectionReference,
            Where = FilterQuery
                .Field(nameof(NumberModel.Val1), FieldOperator.Equal, 2),
            OrderBy = OrderByQuery
                .Descending(nameof(NumberModel.Val2)),
        });
        Assert.NotNull(runQueryTest2.Result);

        Assert.Equal(8, runQueryTest2.Result.Found.Count);
        Assert.Equivalent(writeDocuments[9], runQueryTest2.Result.Found[0].Document);
        Assert.Equivalent(writeDocuments[8], runQueryTest2.Result.Found[1].Document);
        Assert.Equivalent(writeDocuments[7], runQueryTest2.Result.Found[2].Document);
        Assert.Equivalent(writeDocuments[6], runQueryTest2.Result.Found[3].Document);
        Assert.Equivalent(writeDocuments[5], runQueryTest2.Result.Found[4].Document);
        Assert.Equivalent(writeDocuments[4], runQueryTest2.Result.Found[5].Document);
        Assert.Equivalent(writeDocuments[3], runQueryTest2.Result.Found[6].Document);
        Assert.Equivalent(writeDocuments[2], runQueryTest2.Result.Found[7].Document);

        var runQueryTest3 = await Api.FirestoreDatabase.RunQuery(new RunQueryRequest<NumberModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            From = testCollectionReference,
            Where = FilterQuery
                .Field(nameof(NumberModel.Val1), FieldOperator.Equal, 2),
            OrderBy = OrderByQuery
                .Descending(nameof(NumberModel.Val2)),
            Offset = 1,
            Limit = 5
        });
        Assert.NotNull(runQueryTest3.Result);

        Assert.Equal(5, runQueryTest3.Result.Found.Count);
        Assert.Equivalent(writeDocuments[8], runQueryTest3.Result.Found[0].Document);
        Assert.Equivalent(writeDocuments[7], runQueryTest3.Result.Found[1].Document);
        Assert.Equivalent(writeDocuments[6], runQueryTest3.Result.Found[2].Document);
        Assert.Equivalent(writeDocuments[5], runQueryTest3.Result.Found[3].Document);
        Assert.Equivalent(writeDocuments[4], runQueryTest3.Result.Found[4].Document);

        var runQueryTest4 = await Api.FirestoreDatabase.RunQuery(new RunQueryRequest<NumberModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            From = testCollectionReference,
            Select = SelectQuery.DocumentNameOnly(),
            Where = FilterQuery
                .Field(nameof(NumberModel.Val1), FieldOperator.Equal, 2),
            OrderBy = OrderByQuery
                .Descending(nameof(NumberModel.Val2)),
            Offset = 1,
            Limit = 2
        });
        Assert.NotNull(runQueryTest4.Result);

        Assert.Equal(2, runQueryTest4.Result.Found.Count);
        Assert.Null(runQueryTest4.Result.Found[0].Document.Model);
        Assert.Null(runQueryTest4.Result.Found[1].Document.Model);

        var runQueryTest5 = await Api.FirestoreDatabase.RunQuery(new RunQueryRequest<NumberModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            From = testCollectionReference,
            Select = SelectQuery.Add(nameof(NumberModel.Val2)),
            Where = FilterQuery
                .Field(nameof(NumberModel.Val1), FieldOperator.Equal, 2),
            OrderBy = OrderByQuery
                .Descending(nameof(NumberModel.Val2)),
            Offset = 1,
            Limit = 2
        });
        Assert.NotNull(runQueryTest5.Result);

        Assert.Equal(2, runQueryTest5.Result.Found.Count);
        Assert.Equivalent(0, runQueryTest5.Result.Found[0].Document.Model?.Val1);
        Assert.Equivalent(writeDocuments[8].Model?.Val2, runQueryTest5.Result.Found[0].Document.Model?.Val2);
        Assert.Equivalent(0, runQueryTest5.Result.Found[1].Document.Model?.Val1);
        Assert.Equivalent(writeDocuments[7].Model?.Val2, runQueryTest5.Result.Found[1].Document.Model?.Val2);

        await Cleanup(config, testCollectionReference);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NumberModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = writeDocuments,
        });

        Assert.NotNull(getTest3.Result);
        Assert.Empty(getTest3.Result.Found.Select(i => i.Document));
        Assert.NotEmpty(getTest3.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void PatchGetAndDeleteDocumentMVVMModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(PatchGetAndDeleteDocumentMVVMModelTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Document<MVVMModelWithIncludeOnlyAttribute> writeTest1Model1 = model1Reference.Create(new MVVMModelWithIncludeOnlyAttribute()
        {
            Val1 = "test val 1",
            Val2 = "test val 2",
            Val3 = "test val 3",
        });
        Document<MVVMModelWithIncludeOnlyAttribute> writeTest1Model2 = model1Reference.Create(new MVVMModelWithIncludeOnlyAttribute());

        Assert.NotNull(writeTest1Model1.Model);
        Assert.NotNull(writeTest1Model2.Model);

        List<string?> modelPropertyChangedNames = new();
        writeTest1Model2.Model.PropertyChanged += (s, e) =>
        {
            modelPropertyChangedNames.Add(e.PropertyName);
        };

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = writeTest1Model1,
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model2,
        });
        Assert.NotNull(getTest1.Result);

        Assert.Contains(nameof(MVVMModelWithIncludeOnlyAttribute.Val1), modelPropertyChangedNames);
        Assert.Contains(nameof(MVVMModelWithIncludeOnlyAttribute.Val2), modelPropertyChangedNames);
        Assert.DoesNotContain(nameof(MVVMModelWithIncludeOnlyAttribute.Val3), modelPropertyChangedNames);
        Assert.NotEqual(writeTest1Model1.Model.Val3, getTest1.Result.Found[0].Document.Model?.Val3);
        Assert.NotNull(getTest1.Result.Found[0].Document.Model);
        if (getTest1.Result.Found[0].Document.Model is MVVMModelWithIncludeOnlyAttribute model)
        {
            model.Val3 = writeTest1Model1.Model.Val3;
        }
        Assert.Equivalent(writeTest1.Request.PatchDocument?.Documents[0], getTest1.Result.Found[0].Document);

        await Cleanup(config, testCollectionReference);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create<NestedType>(),
        });
        Assert.NotNull(getTest2.Result);

        Assert.Empty(getTest2.Result.Found);
        Assert.NotEmpty(getTest2.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void PatchGetAndDeleteDocumentMVVMDocumentTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(PatchGetAndDeleteDocumentMVVMDocumentTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Document<MVVMModelWithIncludeOnlyAttribute> writeTest1Model1 = model1Reference.Create(new MVVMModelWithIncludeOnlyAttribute()
        {
            Val1 = "test val 1",
            Val2 = "test val 2",
            Val3 = "test val 3",
        });

        // Remove residual files
        await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            Config = config,
            DeleteDocument = model1Reference
        });

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = writeTest1Model1,
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });
        Assert.NotNull(writeTest1.Request.PatchDocument);

        List<string?> documentPropertyChangedNames = new();
        writeTest1Model1.PropertyChanged += (s, e) =>
        {
            documentPropertyChangedNames.Add(e.PropertyName);
        };

        await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = model1Reference.Create(new MVVMModelWithIncludeOnlyAttribute()
            {
                Val1 = "another test val 1",
                Val2 = "another test val 2",
                Val3 = "another test val 3",
            }),
        });

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1Model1,
        });
        Assert.NotNull(getTest1.Result);

        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.Model), documentPropertyChangedNames);
        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.Reference), documentPropertyChangedNames);
        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.CreateTime), documentPropertyChangedNames);
        Assert.Contains(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.UpdateTime), documentPropertyChangedNames);
        Assert.Equivalent(writeTest1.Request.PatchDocument?.Documents[0], getTest1.Result.Found[0].Document);

        await Cleanup(config, testCollectionReference);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create<MVVMModelWithIncludeOnlyAttribute>(),
        });
        Assert.NotNull(getTest2.Result);

        Assert.Empty(getTest2.Result.Found);
        Assert.NotEmpty(getTest2.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void PatchGetAndDeleteDocumentModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(PatchGetAndDeleteDocumentModelTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Document<NestedType> writeTest1ModelNull = model1Reference.Create<NestedType>();
        Document<NestedType> writeTest1ModelEmpty = model1Reference.Create(NestedType.Empty());
        Document<NestedType> writeTest1ModelFilled1 = model1Reference.Create(NestedType.Filled1());
        Document<NestedType> writeTest1ModelFilled2 = model1Reference.Create(NestedType.Filled2());

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = writeTest1ModelFilled1,
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1ModelEmpty,
        });

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1ModelNull,
        });
        Assert.NotNull(getTest1.Result);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1ModelEmpty,
        });
        Assert.NotNull(getTest2.Result);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1ModelFilled2,
        });
        Assert.NotNull(getTest3.Result);

        Assert.Equivalent(writeTest1ModelEmpty, getTest1.Result.Found[0].Document);
        Assert.Equivalent(writeTest1ModelEmpty, getTest2.Result.Found[0].Document);
        Assert.Equivalent(writeTest1ModelEmpty, getTest3.Result.Found[0].Document);

        await Cleanup(config, testCollectionReference);

        var writeTest1Get4 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create<NestedType>(),
        });
        Assert.NotNull(writeTest1Get4.Result);

        Assert.Empty(writeTest1Get4.Result.Found);
        Assert.NotEmpty(writeTest1Get4.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void PatchGetAndDeleteDocumentDictionaryTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(PatchGetAndDeleteDocumentDictionaryTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Document<Dictionary<string, NestedType>> dictionaryNull = model1Reference.Create<Dictionary<string, NestedType>>();
        Document<Dictionary<string, NestedType>> dictionaryEmpty = model1Reference.Create(new Dictionary<string, NestedType>());
        Document<Dictionary<string, NestedType>> dictionary1 = model1Reference.Create(new Dictionary<string, NestedType>()
        {
            { "item1", NestedType.Filled1() },
            { "item2", NestedType.Filled2() },
            { "item3", NestedType.Empty() },
        });
        Document<Dictionary<string, NestedType>> dictionary2 = model1Reference.Create(new Dictionary<string, NestedType>()
        {
            { "anotherItem1", NestedType.Filled2() },
            { "anotherItem2", NestedType.Filled1() },
        });

        // Remove residual files from last test
        await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            Config = config,
            DeleteDocument = model1Reference
        });

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = dictionary1,
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = dictionary1,
        });

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = dictionaryNull,
        });
        Assert.NotNull(getTest1.Result);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = dictionaryEmpty,
        });
        Assert.NotNull(getTest2.Result);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = dictionary2,
        });
        Assert.NotNull(getTest3.Result);

        Assert.Equivalent(dictionary1, getTest1.Result.Found[0].Document);
        Assert.Equivalent(dictionary1, getTest2.Result.Found[0].Document);
        Assert.Equivalent(dictionary1, getTest3.Result.Found[0].Document);

        await Cleanup(config, testCollectionReference);

        var getTest4 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Document = model1Reference.Create<Dictionary<string, NestedType>>(),
        });
        Assert.NotNull(getTest4.Result);

        Assert.Empty(getTest4.Result.Found);
        Assert.NotEmpty(getTest4.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void ListDocumentsTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(ListDocumentsTest));

        await Cleanup(config, testCollectionReference);

        Document<NormalMVVMModel>[] writeDocuments = testCollectionReference.CreateDocuments<NormalMVVMModel>(
            ($"{nameof(FirestoreDatabaseTest)}{nameof(ListDocumentsTest)}test1", new() // a_c
            {
                Val1 = "a",
                Val2 = "c"
            }),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(ListDocumentsTest)}test2", new() // a_d
            {
                Val1 = "a",
                Val2 = "d"
            }),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(ListDocumentsTest)}test3", new() // b_e
            {
                Val1 = "b",
                Val2 = "e"
            }),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(ListDocumentsTest)}test4", new() // b_f
            {
                Val1 = "b",
                Val2 = "f"
            }),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(ListDocumentsTest)}test5", new() // b_g
            {
                Val1 = "b",
                Val2 = "g"
            }));

        // Order for query
        // a_d
        // a_c
        // b_g
        // b_f
        // b_e

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            PatchDocument = writeDocuments
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = writeDocuments
        });
        var listDocumentTest1 = await Api.FirestoreDatabase.ListDocuments(new ListDocumentsRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            CollectionReference = testCollectionReference,
            PageSize = 2,
            OrderBy = OrderByQuery
                .Ascending(nameof(NormalMVVMModel.Val1))
                .Descending(nameof(NormalMVVMModel.Val2))
        });
        Assert.NotNull(listDocumentTest1.Result);

        List<Document<NormalMVVMModel>> ordered = new();
        int pages = 0;
        await foreach (var page in listDocumentTest1.Result.DocumentPager)
        {
            pages++;
            foreach (var doc in page)
            {
                ordered.Add(doc);
            }
        }

        Assert.Equal(3, pages);
        Assert.Equal(5, ordered.Count);
        Assert.Equivalent(writeDocuments.ElementAt(0), ordered[1]);
        Assert.Equivalent(writeDocuments.ElementAt(1), ordered[0]);
        Assert.Equivalent(writeDocuments.ElementAt(2), ordered[4]);
        Assert.Equivalent(writeDocuments.ElementAt(3), ordered[3]);
        Assert.Equivalent(writeDocuments.ElementAt(4), ordered[2]);

        await Cleanup(config, testCollectionReference);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = writeDocuments,
        });

        Assert.NotNull(getTest3.Result);
        Assert.Empty(getTest3.Result.Found.Select(i => i.Document));
        Assert.NotEmpty(getTest3.Result.Missing);
        Assert.Equivalent(writeDocuments.Select(i => i.Reference), getTest3.Result.Missing.Select(i => i.Reference));
    }

    [Fact]
    public async void CreateGetDeleteTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(CreateGetDeleteTest));

        await Cleanup(config, testCollectionReference);

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

        var createTest1 = await Api.FirestoreDatabase.CreateDocument(new CreateDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            CollectionReference = testCollectionReference,
            Model = model1
        });
        Assert.NotNull(createTest1.Result);

        var createTest2 = await Api.FirestoreDatabase.CreateDocument(new CreateDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            CollectionReference = testCollectionReference,
            Model = model2,
            DocumentId = $"{nameof(FirestoreDatabaseTest)}{nameof(CreateGetDeleteTest)}documentIdSample"
        });
        Assert.NotNull(createTest2.Result);

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = createTest1.Result.Reference
        });
        Assert.NotNull(getTest1.Result);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = createTest2.Result.Reference
        });
        Assert.NotNull(getTest2.Result);

        Assert.Equal(model1, createTest1.Result.Model);
        Assert.Equal(model2, createTest2.Result.Model);
        Assert.Equivalent(createTest1.Result, getTest1.Result.Found[0].Document);
        Assert.Equivalent(createTest2.Result, getTest2.Result.Found[0].Document);

        await Cleanup(config, testCollectionReference);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = new Document<NormalMVVMModel>[] { getTest1.Result.Found[0].Document, getTest2.Result.Found[0].Document }
        });

        Assert.NotNull(getTest3.Result);
        Assert.Empty(getTest3.Result.Found.Select(i => i.Document));
        Assert.NotEmpty(getTest3.Result.Missing);

        Assert.True(true);
    }

    [Fact]
    public async void BatchWriteGetDeleteTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(ListDocumentsTest));

        await Cleanup(config, testCollectionReference);

        Document<NormalMVVMModel>[] writeDocuments = testCollectionReference.CreateDocuments<NormalMVVMModel>(
            ("test1", new()
            {
                Val1 = "1 test 1",
                Val2 = "1 test 2"
            }),
            ("test2", new()
            {
                Val1 = "2 test 1",
                Val2 = "2 test 2"
            }),
            ("test3", new()
            {
                Val1 = "3 test 1",
                Val2 = "3 test 2"
            }),
            ("test4", new()
            {
                Val1 = "4 test 1",
                Val2 = "4 test 2"
            }),
            ("test5", new()
            {
                Val1 = "5 test 1",
                Val2 = "5 test 2"
            }));

        Document<NormalMVVMModel>[] emptyPropsDocuments = testCollectionReference.CreateDocuments<NormalMVVMModel>(
            ("test1", null),
            ("test2", null),
            ("test3", null),
            ("test4", null),
            ("test5", null));

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            PatchDocument = writeDocuments
        });
        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = writeDocuments
        });
        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = emptyPropsDocuments
        });

        Assert.NotNull(getTest1.Result);
        Assert.NotNull(getTest2.Result);
        Assert.Equivalent(writeDocuments, emptyPropsDocuments);
        Assert.Equivalent(writeDocuments, getTest1.Result.Found.Select(i => i.Document));
        Assert.Equivalent(writeDocuments, getTest2.Result.Found.Select(i => i.Document));

        await Cleanup(config, testCollectionReference);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = writeDocuments,
        });

        Assert.NotNull(getTest3.Result);
        Assert.Empty(getTest3.Result.Found.Select(i => i.Document));
        Assert.NotEmpty(getTest3.Result.Missing);
        Assert.Equivalent(emptyPropsDocuments.Select(i => i.Reference), getTest3.Result.Missing.Select(i => i.Reference));

        Assert.True(true);
    }
}
