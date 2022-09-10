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
    public class UnitTest1
    {
        [Fact]
        public async void Test1()
        {
            FirebaseConfig config = Helpers.GetFirebaseConfig();

            FirebaseUser user;

            try
            {
                user = await Api.Authentication.SignInWithEmailAndPassword(new SignInWithEmailAndPasswordRequest()
                {
                    Config = config,
                    Email = "test@mail.com",
                    Password = "123123",
                });
            }
            catch (AuthEmailNotFoundException)
            {
                user = await Api.Authentication.CreateUserWithEmailAndPassword(new CreateUserWithEmailAndPasswordRequest()
                {
                    Config = config,
                    Email = "test@mail.com",
                    Password = "123123",
                });
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