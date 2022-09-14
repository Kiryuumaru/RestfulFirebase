using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Requests;
using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.Common.Requests;
using Xunit;

namespace RestfulFirebase.UnitTest
{
    public class AuthenticationTest
    {
        [Fact]
        public async void Test1()
        {
            FirebaseConfig config = Helpers.GetFirebaseConfig();

            FirebaseUser user;

            var loginRequest = await Api.Authentication.SignInWithEmailAndPassword(new SignInWithEmailAndPasswordRequest()
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
                var signupRequest = await Api.Authentication.CreateUserWithEmailAndPassword(new CreateUserWithEmailAndPasswordRequest()
                {
                    Config = config,
                    Email = "test@mail.com",
                    Password = "123123",
                });

                signupRequest.ThrowIfErrorOrEmptyResponse();

                user = signupRequest.Response;
            }

            await Api.Authentication.DeleteUser(new AuthenticatedCommonRequest()
            {
                Config = config,
                FirebaseUser = user,
            });

            Assert.True(true);
        }
    }
}