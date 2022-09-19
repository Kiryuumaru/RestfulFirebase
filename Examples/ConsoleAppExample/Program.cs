using ConsoleAppExample;
using RestfulFirebase;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Exceptions;
using RestfulFirebase.Common.Transactions;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase;
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

var personResponse = await RestfulFirebase.Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Person>()
{
    Config = config,
    Reference = RestfulFirebase.Api.FirestoreDatabase.Query()
        .Collection("public")
        .Document("sample")
});

personResponse.ThrowIfErrorOrEmptyResult();

await RestfulFirebase.Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest<Person>()
{
    Config = config,
    Model = personResponse.Result.Model,
    Reference = RestfulFirebase.Api.FirestoreDatabase.Query()
        .Collection("public")
        .Document("sample1")
});

await RestfulFirebase.Api.Authentication.DeleteUser(new DeleteUserRequest()
{
    Config = config,
    FirebaseUser = user,
});

Console.WriteLine(user.Email);
Console.WriteLine(user.LocalId);