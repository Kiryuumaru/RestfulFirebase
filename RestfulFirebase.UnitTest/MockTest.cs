using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Transactions;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xunit;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Reflection;

namespace RestfulFirebase.UnitTest;

public class MockTest
{
    [Fact]
    public async void Test1()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        FirebaseUser user;

        CollectionReference documentReferenceTest = Api.FirestoreDatabase
            .Collection("public");

        //List<Document<NormalMVVMModel>> documents = new();
        //for (int i = 0; i < 100; i++)
        //{
        //    documents.Add(documentReferenceTest.Document($"model{i}").Create(new NormalMVVMModel()
        //    {
        //        Val1 = $"{i} try 1",
        //        Val2 = $"{i} try 2",
        //    }));
        //}

        //await Api.FirestoreDatabase.WriteDocuments(new WriteDocumentsRequest<NormalMVVMModel>()
        //{
        //    Config = config,
        //    Documents = documents
        //});

        var awdaw = await Api.FirestoreDatabase.ListDocumentReferences(new ListDocumentReferencesRequest<NormalMVVMModel>()
        {
            Config = config,
            CollectionReference = documentReferenceTest,
            PageSize = 60
        });
        Assert.NotNull(awdaw.Result);

        List<Document<NormalMVVMModel>> documents = new();

        int calls = 0;
        await foreach (var page in awdaw.Result.DocumentPager)
        {
            calls++;
            foreach (var doc in page)
            {
                documents.Add(doc);
            }
        }

        Assert.True(true);
        // Remove residual files
        //await Api.FirestoreDatabase.DeleteDocument(new DeleteDocumentRequest()
        //{
        //    Config = config,
        //    Reference = documentReferenceTest
        //});

        //await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<NormalMVVMModel>()
        //{
        //    JsonSerializerOptions = Helpers.JsonSerializerOptions,
        //    Config = config,
        //    Reference = references[0],
        //    Model = new NormalMVVMModel()
        //    {
        //        Val1 = "patch 1 val 1",
        //        Val2 = "patch 1 val 2",
        //    },
        //});

        //await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<NormalMVVMModel>()
        //{
        //    JsonSerializerOptions = Helpers.JsonSerializerOptions,
        //    Config = config,
        //    Reference = references[1],
        //    Model = new NormalMVVMModel()
        //    {
        //        Val1 = "patch 2 val 1",
        //        Val2 = "patch 2 val 2",
        //    },
        //});

        //await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<NormalMVVMModel>()
        //{
        //    JsonSerializerOptions = Helpers.JsonSerializerOptions,
        //    Config = config,
        //    Reference = references[2],
        //    Model = new NormalMVVMModel()
        //    {
        //        Val1 = "patch 3 val 1",
        //        Val2 = "patch 3 val 2",
        //    },
        //});

        //await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<NormalMVVMModel>()
        //{
        //    JsonSerializerOptions = Helpers.JsonSerializerOptions,
        //    Config = config,
        //    Reference = references[3],
        //    Model = new NormalMVVMModel()
        //    {
        //        Val1 = "patch 4 val 1",
        //        Val2 = "patch 4 val 2",
        //    },
        //});

        //await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<NormalMVVMModel>()
        //{
        //    JsonSerializerOptions = Helpers.JsonSerializerOptions,
        //    Config = config,
        //    Reference = references[4],
        //    Model = new NormalMVVMModel()
        //    {
        //        Val1 = "patch 5 val 1",
        //        Val2 = "patch 5 val 2",
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
