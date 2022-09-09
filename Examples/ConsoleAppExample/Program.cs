using ConsoleAppExample;
using RestfulFirebase;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Requests;

FirebaseConfig config = Credentials.Config(); // Your config

FirebaseUser newUser = await FirebaseAuthentication.CreateUserWithEmailAndPassword(new CreateUserWithEmailAndPasswordRequest()
{
    Config = config,
    Email = "t@st.com",
    Password = "123123",
});

Console.WriteLine(newUser.Email);
Console.WriteLine(newUser.LocalId);

FirebaseUser user = await FirebaseAuthentication.SignInWithEmailAndPassword(new SignInWithEmailAndPasswordRequest()
{
    Config = config,
    Email = "t@st.com",
    Password = "123123",
});

Console.WriteLine(user.Email);
Console.WriteLine(user.LocalId);