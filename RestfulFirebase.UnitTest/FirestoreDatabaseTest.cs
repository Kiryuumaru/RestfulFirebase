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
    public async void PatchGetAndDeleteDocumentTest()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        DocumentReference documentReference = Api.FirestoreDatabase.Database()
            .Collection("public")
            .Document($"{nameof(FirestoreDatabaseTest)}{nameof(PatchGetAndDeleteDocumentTest)}");

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        jsonSerializerOptions.Converters.Add(CustomSerializerModel1Type.Converter.Instance);

        // Remove residual files from last test
        try
        {
            await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
            {
                Config = config,
                Reference = documentReference
            });
        }
        catch { }

        Document<NestedType>? patchTest1 = await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<NestedType>()
        {
            Config = config,
            Model = NestedType.Filled1(),
            Reference = documentReference
        }, jsonSerializerOptions);

        Document<NestedType>? getTest1 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            Config = config,
            Reference = documentReference
        }, jsonSerializerOptions);

        Document<NestedType>? getTest2 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            Config = config,
            Model = NestedType.Empty(),
            Reference = documentReference
        }, jsonSerializerOptions);

        Document<NestedType>? getTest3 = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            Config = config,
            Model = NestedType.Filled2(),
            Reference = documentReference
        }, jsonSerializerOptions);

        await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        {
            Config = config,
            Reference = documentReference
        });

        await Assert.ThrowsAsync<FirestoreDatabaseNotFoundException>(async () => await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NestedType>()
        {
            Config = config,
            Reference = documentReference
        }, jsonSerializerOptions));

        Assert.Equivalent(patchTest1, getTest1);
        Assert.Equivalent(patchTest1, getTest2);
        Assert.Equivalent(patchTest1, getTest3);

        Assert.True(true);
    }
}
