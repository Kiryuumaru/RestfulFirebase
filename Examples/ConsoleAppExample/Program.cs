﻿using ConsoleAppExample;
using RestfulFirebase;
using RestfulFirebase.Common.Transactions;
using RestfulFirebase.Common.Models;

FirebaseConfig config = Credentials.Config(); // Your config

FirebaseUser? user;

var loginRequest = await RestfulFirebase.Api.Authentication.SignInWithEmailAndPassword(new SignInWithEmailAndPasswordRequest()
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
    var signupRequest = await RestfulFirebase.Api.Authentication.CreateUserWithEmailAndPassword(new CreateUserWithEmailAndPasswordRequest()
    {
        Config = config,
        Email = "test@mail.com",
        Password = "123123",
    });

    signupRequest.ThrowIfErrorOrEmptyResult();

    user = signupRequest.Result;
}

Console.WriteLine(user.Email);
Console.WriteLine(user.LocalId);