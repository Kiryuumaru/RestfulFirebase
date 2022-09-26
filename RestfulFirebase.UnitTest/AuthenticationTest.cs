using Xunit;
using RestfulFirebase.Authentication.Requests;
using RestfulFirebase.Authentication.Models;

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

            if (loginRequest.HasResult)
            {
                user = loginRequest.Result;
            }
            else
            {
                var signupRequest = await Api.Authentication.CreateUserWithEmailAndPassword(new CreateUserWithEmailAndPasswordRequest()
                {
                    Config = config,
                    Email = "test@mail.com",
                    Password = "123123",
                });

                signupRequest.ThrowIfErrorOrEmptyResult();

                user = signupRequest.Result;
            }

            string encrypted = user.Encrypt(1, 2, 3, 4, 5, 6);

            FirebaseUser decrypted = FirebaseUser.Decrypt(encrypted, 1, 2, 3, 4, 5, 6);

            Assert.Equivalent(user, decrypted);

            await Api.Authentication.DeleteUser(new DeleteUserRequest()
            {
                Config = config,
                Authorization = user,
            });

            Assert.True(true);
        }
    }
}