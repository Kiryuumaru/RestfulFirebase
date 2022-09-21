﻿using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Requests;
using RestfulFirebase.FirestoreDatabase.Exceptions;
using System.Collections.Generic;
using Xunit;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Queries;
using System.Threading.Tasks;

namespace RestfulFirebase.UnitTest;

public class FirestoreDatabaseTest
{
    private async Task Cleanup(FirebaseConfig config, CollectionReference collectionReference)
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
            DeleteDocuments = oldDocs
        });
        cleanups.ThrowIfError();
    }

    [Fact]
    public async void WriteGetAndDeleteDocumentMVVMModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(WriteGetAndDeleteDocumentMVVMModelTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        MVVMModelWithIncludeOnlyAttribute writeTest1Model1 = new()
        {
            Val1 = "test val 1",
            Val2 = "test val 2",
            Val3 = "test val 3",
        };
        MVVMModelWithIncludeOnlyAttribute writeTest1Model2 = new();
        List<string?> modelPropertyChangedNames = new();
        writeTest1Model2.PropertyChanged += (s, e) =>
        {
            modelPropertyChangedNames.Add(e.PropertyName);
        };

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = model1Reference.Create(writeTest1Model1),
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1.Request.PatchDocument,
        });

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create(writeTest1Model2),
        });
        Assert.NotNull(getTest1.Result);

        Assert.Contains(nameof(MVVMModelWithIncludeOnlyAttribute.Val1), modelPropertyChangedNames);
        Assert.Contains(nameof(MVVMModelWithIncludeOnlyAttribute.Val2), modelPropertyChangedNames);
        Assert.DoesNotContain(nameof(MVVMModelWithIncludeOnlyAttribute.Val3), modelPropertyChangedNames);
        Assert.NotEqual(writeTest1.Request.PatchDocument?.Model?.Val3, getTest1.Result.Found[0].Document.Model?.Val3);
        Assert.NotNull(getTest1.Result.Found[0].Document.Model);
        if (getTest1.Result.Found[0].Document.Model is MVVMModelWithIncludeOnlyAttribute model)
        {
            model.Val3 = writeTest1.Request.PatchDocument?.Model?.Val3;
        }
        Assert.Equivalent(writeTest1.Request.PatchDocument, getTest1.Result.Found[0].Document);

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
    public async void WriteGetAndDeleteDocumentMVVMDocumentTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(WriteGetAndDeleteDocumentMVVMDocumentTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        // Remove residual files
        await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            Config = config,
            DeleteDocumentReference = model1Reference
        });

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = model1Reference.Create(new MVVMModelWithIncludeOnlyAttribute()
            {
                Val1 = "test val 1",
                Val2 = "test val 2",
                Val3 = "test val 3",
            }),
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1.Request.PatchDocument,
        });
        Assert.NotNull(writeTest1.Request.PatchDocument);

        List<string?> documentPropertyChangedNames = new();
        writeTest1.Request.PatchDocument.PropertyChanged += (s, e) =>
        {
            documentPropertyChangedNames.Add(e.PropertyName);
        };

        await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
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
            Document = writeTest1.Request.PatchDocument,
        });
        Assert.NotNull(getTest1.Result);

        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.Model), documentPropertyChangedNames);
        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.Reference), documentPropertyChangedNames);
        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.CreateTime), documentPropertyChangedNames);
        Assert.Contains(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.UpdateTime), documentPropertyChangedNames);
        Assert.Equivalent(writeTest1.Request.PatchDocument, getTest1.Result.Found[0].Document);

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
    public async void WriteGetAndDeleteDocumentModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(WriteGetAndDeleteDocumentModelTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = model1Reference.Create(NestedType.Filled1()),
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1.Request.PatchDocument,
        });

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create<NestedType>(),
        });
        Assert.NotNull(getTest1.Result);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create(NestedType.Empty()),
        });
        Assert.NotNull(getTest2.Result);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create(NestedType.Filled2()),
        });
        Assert.NotNull(getTest3.Result);

        Assert.Equivalent(writeTest1.Request.PatchDocument, getTest1.Result.Found[0].Document);
        Assert.Equivalent(writeTest1.Request.PatchDocument, getTest2.Result.Found[0].Document);
        Assert.Equivalent(writeTest1.Request.PatchDocument, getTest3.Result.Found[0].Document);

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
    public async void WriteGetAndDeleteDocumentDictionaryTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(WriteGetAndDeleteDocumentDictionaryTest));

        await Cleanup(config, testCollectionReference);

        DocumentReference model1Reference = testCollectionReference.Document("model1");

        Dictionary<string, NestedType> dictionary1 = new()
        {
            { "item1", NestedType.Filled1() },
            { "item2", NestedType.Filled2() },
            { "item3", NestedType.Empty() },
        };

        Dictionary<string, NestedType> dictionary2 = new()
        {
            { "anotherItem1", NestedType.Filled2() },
            { "anotherItem2", NestedType.Filled1() },
        };

        // Remove residual files from last test
        await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            Config = config,
            DeleteDocumentReference = model1Reference
        });

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            PatchDocument = model1Reference.Create(dictionary1),
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1.Request.PatchDocument,
        });

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create<Dictionary<string, NestedType>>(),
        });
        Assert.NotNull(getTest1.Result);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create(new Dictionary<string, NestedType>()),
        });
        Assert.NotNull(getTest2.Result);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = model1Reference.Create(dictionary2),
        });
        Assert.NotNull(getTest3.Result);

        Assert.Equivalent(writeTest1.Request.PatchDocument, getTest1.Result.Found[0].Document);
        Assert.Equivalent(writeTest1.Request.PatchDocument, getTest2.Result.Found[0].Document);
        Assert.Equivalent(writeTest1.Request.PatchDocument, getTest3.Result.Found[0].Document);

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

        IEnumerable<Document<NormalMVVMModel>> writeDocuments = testCollectionReference.CreateDocuments<NormalMVVMModel>(
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

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            PatchDocuments = writeDocuments
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Documents = writeDocuments
        });
        var listDocumentTest1 = await Api.FirestoreDatabase.ListDocuments(new ListDocumentsRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            CollectionReference = testCollectionReference,
            PageSize = 2,
            OrderBy = OrderBy.Create(
                (nameof(NormalMVVMModel.Val1), OrderDirection.Ascending),
                (nameof(NormalMVVMModel.Val2), OrderDirection.Descending))
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
            Documents = writeDocuments,
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
            DocumentReference = createTest1.Result.Reference
        });
        Assert.NotNull(getTest1.Result);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            DocumentReference = createTest2.Result.Reference
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
            Documents = new Document<NormalMVVMModel>[] { getTest1.Result.Found[0].Document, getTest2.Result.Found[0].Document }
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

        IEnumerable<Document<NormalMVVMModel>> writeDocuments = testCollectionReference.CreateDocuments<NormalMVVMModel>(
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

        IEnumerable<Document<NormalMVVMModel>> emptyPropsDocuments = testCollectionReference.CreateDocuments<NormalMVVMModel>(
            ("test1", null),
            ("test2", null),
            ("test3", null),
            ("test4", null),
            ("test5", null));

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            PatchDocuments = writeDocuments
        });
        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Documents = writeDocuments
        });
        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Documents = emptyPropsDocuments
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
            Documents = writeDocuments,
        });

        Assert.NotNull(getTest3.Result);
        Assert.Empty(getTest3.Result.Found.Select(i => i.Document));
        Assert.NotEmpty(getTest3.Result.Missing);
        Assert.Equivalent(emptyPropsDocuments.Select(i => i.Reference), getTest3.Result.Missing.Select(i => i.Reference));

        Assert.True(true);
    }
}
