using Xunit;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase.Requests;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Enums;

namespace RestfulFirebase.UnitTest;

public class MockTest
{
    [Fact]
    public async void Test1()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();


        Assert.True(true);
    }
}
