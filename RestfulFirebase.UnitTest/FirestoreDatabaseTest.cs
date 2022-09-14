using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Requests;
using RestfulFirebase.CloudFirestore.Query;
using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.Common.Requests;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Abstraction;
using RestfulFirebase.FirestoreDatabase.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace RestfulFirebase.UnitTest;

public class FirestoreDatabaseTest
{
    [Fact]
    public async void PatchGetAndDeleteDocumentModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        DocumentReference documentReferenceTest1 = Api.FirestoreDatabase.Database()
            .Collection("public")
            .Document($"{nameof(FirestoreDatabaseTest)}{nameof(PatchGetAndDeleteDocumentModelTest)}");

        // Remove residual files from last test
        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            Reference = documentReferenceTest1
        });

        var patchTest1 = await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Model = NestedType.Filled1(),
            Reference = documentReferenceTest1
        });
        Assert.NotNull(patchTest1.Response);

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Reference = documentReferenceTest1
        });
        Assert.NotNull(getTest1.Response);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Model = NestedType.Empty(),
            Reference = documentReferenceTest1
        });
        Assert.NotNull(getTest2.Response);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Model = NestedType.Filled2(),
            Reference = documentReferenceTest1
        });
        Assert.NotNull(getTest3.Response);

        Assert.Equivalent(patchTest1.Response, getTest1.Response);
        Assert.Equivalent(patchTest1.Response, getTest2.Response);
        Assert.Equivalent(patchTest1.Response, getTest3.Response);

        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            Reference = documentReferenceTest1
        });

        var patchTest1Get4 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Reference = documentReferenceTest1
        });

        Assert.Throws<FirestoreDatabaseNotFoundException>(patchTest1Get4.ThrowIfErrorOrEmptyResponse);

        Assert.True(true);
    }

    [Fact]
    public async void PatchGetAndDeleteDocumentDictionaryTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        DocumentReference documentReferenceTest1 = Api.FirestoreDatabase.Database()
            .Collection("public")
            .Document($"{nameof(FirestoreDatabaseTest)}{nameof(PatchGetAndDeleteDocumentDictionaryTest)}");

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
            Reference = documentReferenceTest1
        });

        var patchTest1 = await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Model = dictionary1,
            Reference = documentReferenceTest1
        });
        Assert.NotNull(patchTest1.Response);

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Reference = documentReferenceTest1
        });
        Assert.NotNull(getTest1.Response);

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Model = new Dictionary<string, NestedType>(),
            Reference = documentReferenceTest1
        });
        Assert.NotNull(getTest2.Response);

        var getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Model = dictionary2,
            Reference = documentReferenceTest1
        });
        Assert.NotNull(getTest3.Response);

        Assert.Equivalent(patchTest1.Response, getTest1.Response);
        Assert.Equivalent(patchTest1.Response, getTest2.Response);
        Assert.Equivalent(patchTest1.Response, getTest3.Response);

        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            Reference = documentReferenceTest1
        });

        var getTest4 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Reference = documentReferenceTest1
        });

        Assert.Throws<FirestoreDatabaseNotFoundException>(getTest4.ThrowIfErrorOrEmptyResponse);

        Assert.True(true);
    }

    [Fact]
    public async void PatchGetAndDeleteDocumentMVVMModelTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        DocumentReference documentReferenceTest1 = Api.FirestoreDatabase.Database()
            .Collection("public")
            .Document($"{nameof(FirestoreDatabaseTest)}{nameof(PatchGetAndDeleteDocumentMVVMModelTest)}");

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            Reference = documentReferenceTest1
        });

        MVVMModelWithIncludeOnlyAttribute patchTest1Model1 = new()
        {
            Val1 = "test val 1",
            Val2 = "test val 2",
            Val3 = "test val 3",
        };
        MVVMModelWithIncludeOnlyAttribute patchTest1Model2 = new();
        List<string?> modelPropertyChangedNames = new();
        patchTest1Model2.PropertyChanged += (s, e) =>
        {
            modelPropertyChangedNames.Add(e.PropertyName);
        };

        var patchTest1 = await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Model = patchTest1Model1,
            Reference = documentReferenceTest1
        });
        Assert.NotNull(patchTest1.Response);

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Model = patchTest1Model2,
            Reference = documentReferenceTest1
        });
        Assert.NotNull(getTest1.Response);

        Assert.Contains(nameof(MVVMModelWithIncludeOnlyAttribute.Val1), modelPropertyChangedNames);
        Assert.Contains(nameof(MVVMModelWithIncludeOnlyAttribute.Val2), modelPropertyChangedNames);
        Assert.DoesNotContain(nameof(MVVMModelWithIncludeOnlyAttribute.Val3), modelPropertyChangedNames);
        Assert.NotEqual(patchTest1.Response.Model.Val3, getTest1.Response.Model.Val3);
        getTest1.Response.Model.Val3 = patchTest1.Response.Model.Val3;
        Assert.Equivalent(patchTest1.Response, getTest1.Response);

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            Reference = documentReferenceTest1
        });

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Reference = documentReferenceTest1
        });

        Assert.Throws<FirestoreDatabaseNotFoundException>(getTest2.ThrowIfErrorOrEmptyResponse);

        Assert.True(true);
    }

    [Fact]
    public async void PatchGetAndDeleteDocumentMVVMDocumentTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        DocumentReference documentReferenceTest1 = Api.FirestoreDatabase.Database()
            .Collection("public")
            .Document($"{nameof(FirestoreDatabaseTest)}{nameof(PatchGetAndDeleteDocumentMVVMDocumentTest)}");

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            Reference = documentReferenceTest1
        });

        var patchTest1 = await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Model = new()
            {
                Val1 = "test val 1",
                Val2 = "test val 2",
                Val3 = "test val 3",
            },
            Reference = documentReferenceTest1
        });
        Assert.NotNull(patchTest1.Response);

        List<string?> documentPropertyChangedNames = new();
        patchTest1.Response.PropertyChanged += (s, e) =>
        {
            documentPropertyChangedNames.Add(e.PropertyName);
        };

        await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Model = new()
            {
                Val1 = "another test val 1",
                Val2 = "another test val 2",
                Val3 = "another test val 3",
            },
            Reference = documentReferenceTest1
        });

        var getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Document = patchTest1.Response,
            Reference = documentReferenceTest1
        });
        Assert.NotNull(getTest1.Response);

        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.Model), documentPropertyChangedNames);
        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.Reference), documentPropertyChangedNames);
        Assert.DoesNotContain(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.CreateTime), documentPropertyChangedNames);
        Assert.Contains(nameof(Document<MVVMModelWithIncludeOnlyAttribute>.UpdateTime), documentPropertyChangedNames);
        Assert.Equivalent(patchTest1.Response, getTest1.Response);

        // Remove residual files
        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            Reference = documentReferenceTest1
        });

        var getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<MVVMModelWithIncludeOnlyAttribute>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Reference = documentReferenceTest1
        });

        Assert.Throws<FirestoreDatabaseNotFoundException>(getTest2.ThrowIfErrorOrEmptyResponse);

        Assert.True(true);
    }
}
