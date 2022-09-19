using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Exceptions;
using RestfulFirebase.Common.Transactions;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.FirestoreDatabase.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Enums;

namespace RestfulFirebase.UnitTest;

public class FirestoreDatabaseTest
{
    [Fact]
    public async void WriteGetAndDeleteDocumentModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        DocumentReference documentReferenceTest1 = Api.FirestoreDatabase
            .Collection("public")
            .Document($"{nameof(FirestoreDatabaseTest)}{nameof(WriteGetAndDeleteDocumentModelTest)}");

        // Remove residual files from last test
        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            DocumentReference = documentReferenceTest1
        });

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create(NestedType.Filled1()),
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1.Request.Document,
        });

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create<NestedType>(),
        });
        Assert.NotNull(getTest1.Result);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create(NestedType.Empty()),
        });
        Assert.NotNull(getTest2.Result);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create(NestedType.Filled2()),
        });
        Assert.NotNull(getTest3.Result);

        Assert.Equivalent(writeTest1.Request.Document, getTest1.Result);
        Assert.Equivalent(writeTest1.Request.Document, getTest2.Result);
        Assert.Equivalent(writeTest1.Request.Document, getTest3.Result);

        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            DocumentReference = documentReferenceTest1
        });

        var writeTest1Get4 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create<NestedType>(),
        });

        Assert.Throws<FirestoreDatabaseNotFoundException>(writeTest1Get4.ThrowIfErrorOrEmptyResult);

        Assert.True(true);
    }

    [Fact]
    public async void WriteGetAndDeleteDocumentDictionaryTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        DocumentReference documentReferenceTest1 = Api.FirestoreDatabase
            .Collection("public")
            .Document($"{nameof(FirestoreDatabaseTest)}{nameof(WriteGetAndDeleteDocumentDictionaryTest)}");

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
        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            DocumentReference = documentReferenceTest1
        });

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create(dictionary1),
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1.Request.Document,
        });

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create<Dictionary<string, NestedType>>(),
        });
        Assert.NotNull(getTest1.Result);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create(new Dictionary<string, NestedType>()),
        });
        Assert.NotNull(getTest2.Result);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create(dictionary2),
        });
        Assert.NotNull(getTest3.Result);

        Assert.Equivalent(writeTest1.Request.Document, getTest1.Result);
        Assert.Equivalent(writeTest1.Request.Document, getTest2.Result);
        Assert.Equivalent(writeTest1.Request.Document, getTest3.Result);

        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            DocumentReference = documentReferenceTest1
        });

        var getTest4 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Document = documentReferenceTest1.Create<Dictionary<string, NestedType>>(),
        });

        Assert.Throws<FirestoreDatabaseNotFoundException>(getTest4.ThrowIfErrorOrEmptyResult);

        Assert.True(true);
    }

    [Fact]
    public async void WriteGetAndDeleteDocumentMVVMModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        DocumentReference documentReferenceTest1 = Api.FirestoreDatabase
            .Collection("public")
            .Document($"{nameof(FirestoreDatabaseTest)}{nameof(WriteGetAndDeleteDocumentMVVMModelTest)}");

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            DocumentReference = documentReferenceTest1
        });

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
            Document = documentReferenceTest1.Create(writeTest1Model1),
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = writeTest1.Request.Document,
        });

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create(writeTest1Model2),
        });
        Assert.NotNull(getTest1.Result);

        Assert.Contains(nameof(MVVMModelWithIncludeOnlyAttribute.Val1), modelPropertyChangedNames);
        Assert.Contains(nameof(MVVMModelWithIncludeOnlyAttribute.Val2), modelPropertyChangedNames);
        Assert.DoesNotContain(nameof(MVVMModelWithIncludeOnlyAttribute.Val3), modelPropertyChangedNames);
        Assert.NotEqual(writeTest1.Request.Document?.Model?.Val3, getTest1.Result.Model?.Val3);
        Assert.NotNull(getTest1.Result.Model);
        getTest1.Result.Model.Val3 = writeTest1.Request.Document?.Model?.Val3;
        Assert.Equivalent(writeTest1.Request.Document, getTest1.Result);

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            DocumentReference = documentReferenceTest1
        });

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create<NestedType>(),
        });

        Assert.Throws<FirestoreDatabaseNotFoundException>(getTest2.ThrowIfErrorOrEmptyResult);

        Assert.True(true);
    }

    [Fact]
    public async void WriteGetAndDeleteDocumentMVVMDocumentTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        DocumentReference documentReferenceTest1 = Api.FirestoreDatabase
            .Collection("public")
            .Document($"{nameof(FirestoreDatabaseTest)}{nameof(WriteGetAndDeleteDocumentMVVMDocumentTest)}");

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            DocumentReference = documentReferenceTest1
        });

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create(new MVVMModelWithIncludeOnlyAttribute()
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
            Document = writeTest1.Request.Document,
        });
        Assert.NotNull(writeTest1.Request.Document);

        List<string?> documentPropertyChangedNames = new();
        writeTest1.Request.Document.PropertyChanged += (s, e) =>
        {
            documentPropertyChangedNames.Add(e.PropertyName);
        };

        await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create(new MVVMModelWithIncludeOnlyAttribute()
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
            Document = writeTest1.Request.Document,
        });
        Assert.NotNull(getTest1.Result);

        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.Model), documentPropertyChangedNames);
        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.Reference), documentPropertyChangedNames);
        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.CreateTime), documentPropertyChangedNames);
        Assert.Contains(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.UpdateTime), documentPropertyChangedNames);
        Assert.Equivalent(writeTest1.Request.Document, getTest1.Result);

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            DocumentReference = documentReferenceTest1
        });

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = documentReferenceTest1.Create<MVVMModelWithIncludeOnlyAttribute>(),
        });

        Assert.Throws<FirestoreDatabaseNotFoundException>(getTest2.ThrowIfErrorOrEmptyResult);

        Assert.True(true);
    }

    [Fact]
    public async void BatchWriteGetDeleteTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference documentReferenceTest = Api.FirestoreDatabase
            .Collection("public");

        IEnumerable<Document<NormalMVVMModel>> writeDocuments = documentReferenceTest.CreateDocuments<NormalMVVMModel>(
            ($"{nameof(FirestoreDatabaseTest)}{nameof(BatchWriteGetDeleteTest)}test1", new()
            {
                Val1 = "1 test 1",
                Val2 = "1 test 2"
            }),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(BatchWriteGetDeleteTest)}test2", new()
            {
                Val1 = "2 test 1",
                Val2 = "2 test 2"
            }),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(BatchWriteGetDeleteTest)}test3", new()
            {
                Val1 = "3 test 1",
                Val2 = "3 test 2"
            }),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(BatchWriteGetDeleteTest)}test4", new()
            {
                Val1 = "4 test 1",
                Val2 = "4 test 2"
            }),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(BatchWriteGetDeleteTest)}test5", new()
            {
                Val1 = "5 test 1",
                Val2 = "5 test 2"
            }));

        IEnumerable<Document<NormalMVVMModel>> emptyPropsDocuments = documentReferenceTest.CreateDocuments<NormalMVVMModel>(
            ($"{nameof(FirestoreDatabaseTest)}{nameof(BatchWriteGetDeleteTest)}test1", null),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(BatchWriteGetDeleteTest)}test2", null),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(BatchWriteGetDeleteTest)}test3", null),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(BatchWriteGetDeleteTest)}test4", null),
            ($"{nameof(FirestoreDatabaseTest)}{nameof(BatchWriteGetDeleteTest)}test5", null));

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocuments(new DeleteDocumentsRequest()
        {
            Config = config,
            Documents = emptyPropsDocuments
        });

        var writeTest1 = await Api.FirestoreDatabase.WriteDocuments(new WriteDocumentsRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Documents = writeDocuments
        });
        var getTest1 = await Api.FirestoreDatabase.GetDocuments(new GetDocumentsRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Documents = writeDocuments
        });
        var getTest2 = await Api.FirestoreDatabase.GetDocuments(new GetDocumentsRequest<NormalMVVMModel>()
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

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocuments(new DeleteDocumentsRequest()
        {
            Config = config,
            Documents = writeDocuments
        });

        var getTest3 = await Api.FirestoreDatabase.GetDocuments(new GetDocumentsRequest<NormalMVVMModel>()
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


    [Fact]
    public async void ListDocumentsTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference documentReferenceTest = Api.FirestoreDatabase
            .Collection("public");

        IEnumerable<Document<NormalMVVMModel>> writeDocuments = documentReferenceTest.CreateDocuments<NormalMVVMModel>(
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

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocuments(new DeleteDocumentsRequest()
        {
            Config = config,
            Documents = writeDocuments
        });

        var writeTest1 = await Api.FirestoreDatabase.WriteDocuments(new WriteDocumentsRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Documents = writeDocuments
        });
        await Api.FirestoreDatabase.GetDocuments(new GetDocumentsRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Documents = writeDocuments
        });
        var listDocumentTest1 = await Api.FirestoreDatabase.ListDocuments(new ListDocumentsRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            CollectionReference = documentReferenceTest,
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

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocuments(new DeleteDocumentsRequest()
        {
            Config = config,
            Documents = writeDocuments
        });

        var getTest3 = await Api.FirestoreDatabase.GetDocuments(new GetDocumentsRequest<NormalMVVMModel>()
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
}
