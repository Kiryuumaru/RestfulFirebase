using Xunit;
using RestfulFirebase.FirestoreDatabase.Requests;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Authentication.Requests;

namespace RestfulFirebase.UnitTest;

public class MockTest
{
    [Fact]
    public async void Test1()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        FirebaseUser? user;

        var loginRequest = await Api.Authentication.SignInWithEmailAndPassword(new SignInWithEmailAndPasswordRequest()
        {
            Config = config,
            Email = "test@mail.com",
            Password = "123123",
        });

        if (loginRequest.HasResult)
        {
            user = loginRequest.Result;
        }
        else
        {
            var signupRequest = await Api.Authentication.CreateUserWithEmailAndPassword(new CreateUserWithEmailAndPasswordRequest()
            {
                Config = config,
                Email = "test@mail.com",
                Password = "123123",
            });

            signupRequest.ThrowIfErrorOrEmptyResult();

            user = signupRequest.Result;
        }

        var transac1 = await Api.FirestoreDatabase.BeginTransaction(new BeginTransactionRequest()
        {
            Config = config,
            Authorization = user,
            Option = TransactionOption.ReadOnly()
        });

        var transac2 = await Api.FirestoreDatabase.BeginTransaction(new BeginTransactionRequest()
        {
            Config = config,
            Authorization = user,
            Option = TransactionOption.ReadWrite()
        });

        Assert.True(true);
    }
}
