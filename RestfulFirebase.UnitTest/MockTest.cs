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

        List<Document<NormalMVVMModel>> orderedDocuments = new();

        for (int i = 0; i < 300; i++)
        {
            orderedDocuments.Add(documentReferenceTest.Document($"model").Collection($"model{i}").Document($"model").Create(new NormalMVVMModel()
            {
                Val1 = $"{i.ToString("0000")} try 1",
                Val2 = $"{i.ToString("0000")} try 2",
            }));
        }

        List<Document<NormalMVVMModel>> writeDocuments = new();
        Random random = new();
        while (orderedDocuments.Count > 0)
        {
            int index = random.Next(0, orderedDocuments.Count - 1);
            writeDocuments.Add(orderedDocuments[index]);
            orderedDocuments.RemoveAt(index);
        }

        await Api.FirestoreDatabase.WriteDocuments(new WriteDocumentsRequest<NormalMVVMModel>()
        {
            Config = config,
            Documents = writeDocuments
        });

        var awdaw = await Api.FirestoreDatabase.ListCollections(new ListCollectionsRequest()
        {
            Config = config,
            //DocumentReference = documentReferenceTest.Document($"model"),
            PageSize = 1
        });
        Assert.NotNull(awdaw.Result);

        List<CollectionReference> cols = new();

        int calls = 0;
        await foreach (var page in awdaw.Result.CollectionPager)
        {
            calls++;
            foreach (var doc in page)
            {
                cols.Add(doc);
            }
        }

        Assert.True(true);
    }
}
