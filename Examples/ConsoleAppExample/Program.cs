﻿using ConsoleAppExample;
using RestfulFirebase;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Requests;

FirebaseConfig config = Credentials.Config(); // Your config

//FirebaseUser newUser = await FirebaseAuthentication.CreateUserWithEmailAndPassword(new CreateUserWithEmailAndPasswordRequest()
//{
//    Config = config,
//    Email = "t@st.com",
//    Password = "123123",
//});

//Console.WriteLine(newUser.Email);
//Console.WriteLine(newUser.LocalId);

FirebaseUser user1 = await FirebaseAuthentication.SignInWithEmailAndPassword(new SignInWithEmailAndPasswordRequest()
{
    Config = config,
    Email = "t@st.com",
    Password = "123123",
});

Console.WriteLine(user1.Email);
Console.WriteLine(user1.LocalId);

FirebaseUser user2 = await FirebaseAuthentication.SignInAnonymously(new AuthenticationRequest()
{
    Config = config,
});

Console.WriteLine(user2.Email);
Console.WriteLine(user2.LocalId);