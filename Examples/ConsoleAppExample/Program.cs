using ConsoleAppExample;
using RestfulFirebase;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Requests;

FirebaseConfig config = Credentials.Config(); // Your config

FirebaseUser user;

try
{
    user = await FirebaseAuthentication.SignInWithEmailAndPassword(new SignInWithEmailAndPasswordRequest()
    {
        Config = config,
        Email = "t@st.com",
        Password = "123123",
    });
}
catch (AuthEmailNotFoundException)
{
    user = await FirebaseAuthentication.CreateUserWithEmailAndPassword(new CreateUserWithEmailAndPasswordRequest()
    {
        Config = config,
        Email = "t@st.com",
        Password = "123123",
    });
}

FirebaseUser user2 = await FirebaseAuthentication.SignInAnonymously(new AuthenticationRequest()
{
    Config = config,
});

await FirebaseAuthentication.DeleteUser(new AuthenticatedRequest()
{
    Config = config,
    FirebaseUser = user,
});
await FirebaseAuthentication.DeleteUser(new AuthenticatedRequest()
{
    Config = config,
    FirebaseUser = user2,
});

Console.WriteLine(user2.Email);
Console.WriteLine(user2.LocalId);