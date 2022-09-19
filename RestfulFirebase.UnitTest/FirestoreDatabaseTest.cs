using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Transactions;
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
    public async void BatchGetDeleteDocumentTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        //MultipleDocumentReference documentReferenceTest = Api.FirestoreDatabase.Query()
        //    .Collection("public")
        //    .Documents("mock1", "mock2", "mock3", "mock4", "mock5", "mock6");

        //var references = documentReferenceTest.GetDocumentReferences();

        //await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        //{
        //    Config = config,
        //    Reference = documentReferenceTest
        //});

        //await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<NormalMVVMModel>()
        //{
        //    JsonSerializerOptions = Helpers.JsonSerializerOptions,
        //    Config = config,
        //    Reference = references[0],
        //    Model = new NormalMVVMModel()
        //    {
        //        Val1 = "write 1 val 1",
        //        Val2 = "write 1 val 2",
        //    },
        //});

        //await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<NormalMVVMModel>()
        //{
        //    JsonSerializerOptions = Helpers.JsonSerializerOptions,
        //    Config = config,
        //    Reference = references[1],
        //    Model = new NormalMVVMModel()
        //    {
        //        Val1 = "write 2 val 1",
        //        Val2 = "write 2 val 2",
        //    },
        //});

        //await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<NormalMVVMModel>()
        //{
        //    JsonSerializerOptions = Helpers.JsonSerializerOptions,
        //    Config = config,
        //    Reference = references[2],
        //    Model = new NormalMVVMModel()
        //    {
        //        Val1 = "write 3 val 1",
        //        Val2 = "write 3 val 2",
        //    },
        //});

        //await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<NormalMVVMModel>()
        //{
        //    JsonSerializerOptions = Helpers.JsonSerializerOptions,
        //    Config = config,
        //    Reference = references[3],
        //    Model = new NormalMVVMModel()
        //    {
        //        Val1 = "write 4 val 1",
        //        Val2 = "write 4 val 2",
        //    },
        //});

        //await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<NormalMVVMModel>()
        //{
        //    JsonSerializerOptions = Helpers.JsonSerializerOptions,
        //    Config = config,
        //    Reference = references[4],
        //    Model = new NormalMVVMModel()
        //    {
        //        Val1 = "write 5 val 1",
        //        Val2 = "write 5 val 2",
        //    },
        //});

        //var batchGetResponse = await Api.FirestoreDatabase.BatchGet(new BatchGetDocumentRequest<NormalMVVMModel>
        //{
        //    JsonSerializerOptions = Helpers.JsonSerializerOptions,
        //    Config = config,
        //    Reference = documentReferenceTest
        //});


        Assert.True(true);
    }
}
