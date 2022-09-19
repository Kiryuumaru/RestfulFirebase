using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Exceptions;
using RestfulFirebase.Common.Transactions;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xunit;
using RestfulFirebase.Common.Models;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Reflection;
using RestfulFirebase.FirestoreDatabase.Enums;

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

        //List<Document<NormalMVVMModel>> orderedDocuments = new();

        //for (int i = 0; i < 300; i++)
        //{
        //    orderedDocuments.Add(documentReferenceTest.Document($"model{i}").Create(new NormalMVVMModel()
        //    {
        //        Val1 = $"{i.ToString("0000")} try 1",
        //        Val2 = $"{i.ToString("0000")} try 2",
        //    }));
        //}

        //List<Document<NormalMVVMModel>> writeDocuments = new();
        //Random random = new();
        //while (orderedDocuments.Count > 0)
        //{
        //    int index = random.Next(0, orderedDocuments.Count - 1);
        //    writeDocuments.Add(orderedDocuments[index]);
        //    orderedDocuments.RemoveAt(index);
        //}

        //await Api.FirestoreDatabase.WriteDocuments(new WriteDocumentsRequest<NormalMVVMModel>()
        //{
        //    Config = config,
        //    Documents = writeDocuments
        //});

        var awdaw = await Api.FirestoreDatabase.ListDocuments(new ListDocumentsRequest<NormalMVVMModel>()
        {
            Config = config,
            CollectionReference = documentReferenceTest,
            PageSize = 60,
            OrderBy = OrderBy.Create(
                (nameof(NormalMVVMModel.Val1), OrderDirection.Ascending),
                (nameof(NormalMVVMModel.Val2), OrderDirection.Descending))
        });
        Assert.NotNull(awdaw.Result);

        List<Document<NormalMVVMModel>> readDocuments = new();

        int calls = 0;
        await foreach (var page in awdaw.Result.DocumentPager)
        {
            calls++;
            foreach (var doc in page)
            {
                readDocuments.Add(doc);
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
