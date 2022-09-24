using Xunit;
using RestfulFirebase.FirestoreDatabase.Requests;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Authentication.Requests;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Text.Json.Serialization;
using RestfulFirebase.FirestoreDatabase.Transform;
using RestfulFirebase.FirestoreDatabase.References;

namespace RestfulFirebase.UnitTest;

public class MockTest
{
    [Fact]
    public async void Test1()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection("mock");

        Assert.True(true);
    }
}
