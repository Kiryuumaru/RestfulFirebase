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
        try
        {
            await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
            {
                Config = config,
                Reference = documentReferenceTest1
            });
        }
        catch { }

        Document<NestedType>? patchTest1 = await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Model = NestedType.Filled1(),
            Reference = documentReferenceTest1
        });

        Document<NestedType>? getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Reference = documentReferenceTest1
        });

        Document<NestedType>? getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Model = NestedType.Empty(),
            Reference = documentReferenceTest1
        });

        Document<NestedType>? getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Model = NestedType.Filled2(),
            Reference = documentReferenceTest1
        });

        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            Reference = documentReferenceTest1
        });

        await Assert.ThrowsAsync<FirestoreDatabaseNotFoundException>(async () => await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Config = config,
            Reference = documentReferenceTest1
        }));

        Assert.Equivalent(patchTest1, getTest1);
        Assert.Equivalent(patchTest1, getTest2);
        Assert.Equivalent(patchTest1, getTest3);

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
        try
        {
            await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
            {
                Config = config,
                Reference = documentReferenceTest1
            });
        }
        catch { }

        Document<Dictionary<string, NestedType>>? patchTest1 = await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Model = dictionary1,
            Reference = documentReferenceTest1
        });

        Document<Dictionary<string, NestedType>>? getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Reference = documentReferenceTest1
        });

        Document<Dictionary<string, NestedType>>? getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Model = new Dictionary<string, NestedType>(),
            Reference = documentReferenceTest1
        });

        Document<Dictionary<string, NestedType>>? getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Model = dictionary2,
            Reference = documentReferenceTest1
        });

        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            Reference = documentReferenceTest1
        });

        await Assert.ThrowsAsync<FirestoreDatabaseNotFoundException>(async () => await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Dictionary<string, NestedType>>()
        {
            Config = config,
            Reference = documentReferenceTest1
        }));

        Assert.Equivalent(patchTest1, getTest1);
        Assert.Equivalent(patchTest1, getTest2);
        Assert.Equivalent(patchTest1, getTest3);

        Assert.True(true);
    }
}
