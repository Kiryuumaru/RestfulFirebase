using ConsoleAppExample;
using RestfulFirebase;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Requests;
using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.Common.Requests;
using RestfulFirebase.FirestoreDatabase;

FirebaseConfig config = Credentials.Config(); // Your config

FirebaseUser? user;

var loginRequest = await RestfulFirebase.Api.Authentication.SignInWithEmailAndPassword(new SignInWithEmailAndPasswordRequest()
{
    Config = config,
    Email = "test@mail.com",
    Password = "123123",
});

if (loginRequest.HasResponse)
{
    user = loginRequest.Response;
}
else
{
    var signupRequest = await RestfulFirebase.Api.Authentication.CreateUserWithEmailAndPassword(new CreateUserWithEmailAndPasswordRequest()
    {
        Config = config,
        Email = "test@mail.com",
        Password = "123123",
    });

    signupRequest.ThrowIfErrorOrEmptyResponse();

    user = signupRequest.Response;
}

var personResponse = await RestfulFirebase.Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Person>()
{
    Config = config,
    Reference = RestfulFirebase.Api.FirestoreDatabase.Database()
        .Collection("public")
        .Document("sample")
});

personResponse.ThrowIfErrorOrEmptyResponse();

await RestfulFirebase.Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<Person>()
{
    Config = config,
    Model = personResponse.Response.Model,
    Reference = RestfulFirebase.Api.FirestoreDatabase.Database()
        .Collection("public")
        .Document("sample1")
});

await RestfulFirebase.Api.Authentication.DeleteUser(new AuthenticatedCommonRequest()
{
    Config = config,
    FirebaseUser = user,
});

Console.WriteLine(user.Email);
Console.WriteLine(user.LocalId);