using ConsoleAppExample;
using RestfulFirebase;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Models;

FirebaseApp app = new(Credentials.Config());// Your config

FirebaseUser? user;

// Login to firebase auth
var loginRequest = await app.Authentication.SignInWithEmailAndPassword("test@mail.com", "123123");

if (loginRequest.IsSuccess)
{
    user = loginRequest.Result;
}
else
{
    // Create firebase auth
    var signupRequest = await app.Authentication.CreateUserWithEmailAndPassword("test@mail.com", "123123");

    signupRequest.ThrowIfError();

    user = signupRequest.Result;
}

Console.WriteLine(user.Email);
Console.WriteLine(user.LocalId);