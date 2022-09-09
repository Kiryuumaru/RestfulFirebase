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

string ss = await FirebaseAuthentication.GetFreshToken(new AuthenticatedRequest()
{
    Config = config,
    FirebaseUser = user,
});

await FirebaseAuthentication.DeleteUser(new AuthenticatedRequest()
{
    Config = config,
    FirebaseUser = user,
});

Console.WriteLine(user.Email);
Console.WriteLine(user.LocalId);