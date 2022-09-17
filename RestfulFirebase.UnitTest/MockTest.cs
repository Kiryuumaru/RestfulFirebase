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

namespace RestfulFirebase.UnitTest;

public class MockTest
{
    [Fact]
    public async void Test1()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        FirebaseUser user;

        //MultipleDocumentReferences documentReferenceTest = Api.FirestoreDatabase.Query()
        //    .Collection("public")
        //    .MultipleDocuments;

        var res = await Api.FirestoreDatabase.WriteDocuments(new WriteDocumentsRequest<NormalMVVMModel>()
        {
            Config = config,
            MultipleDocuments = Database.Query()
                .Collection("public")
                .MultipleDocuments<NormalMVVMModel>()
                .AddDocument(new NormalMVVMModel()
                {
                    Val1 = "1 try 1 aawd",
                    Val2 = "1 try 2 dwd",
                }, "model1")
                .AddDocument(new NormalMVVMModel()
                {
                    Val1 = "2 try 1 ddw",
                    Val2 = "2 try 2 wd",
                }, "model2")
                .AddDocument(new NormalMVVMModel()
                {
                    Val1 = "3 try 1 d",
                    Val2 = "3 try 2  w",
                }, "model3")
        });

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
