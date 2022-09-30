using ConsoleAppExample;
using RestfulFirebase;
using RestfulFirebase.Authentication.Models;
using RestfulFirebase.Authentication.Requests;

FirebaseConfig config = Credentials.Config(); // Your config

FirebaseUser? user;

var loginRequest = await RestfulFirebase.Api.Authentication.SignInWithEmailAndPassword(new SignInWithEmailAndPasswordRequest()
{
    Config = config,
    Email = "test@mail.com",
    Password = "123123",
});

if (loginRequest.IsSuccess)
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

    signupRequest.ThrowIfError();

    user = signupRequest.Result;
}

Console.WriteLine(user.Email);
Console.WriteLine(user.LocalId);