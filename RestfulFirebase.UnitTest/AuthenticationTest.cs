using RestfulFirebase.Authentication;
using Xunit;

namespace RestfulFirebase.UnitTest
{
    public class AuthenticationTest
    {
        [Fact]
        public async void Test1()
        {
            FirebaseApp app = new(Helpers.GetFirebaseConfig());

            FirebaseUser user;

            var loginRequest = await app.Authentication.SignInWithEmailAndPassword("test@mail.com", "123123");

            var ss1 = await loginRequest.GetResponseContentAsString();

            if (loginRequest.IsSuccess)
            {
                user = loginRequest.Result;
            }
            else
            {
                var signupRequest = await app.Authentication.CreateUserWithEmailAndPassword("test@mail.com", "123123", false);

                var ss2 = await signupRequest.GetResponseContentAsString();

                signupRequest.ThrowIfError();

                user = signupRequest.Result;
            }

            string encrypted = user.Encrypt(1, 2, 3, 4, 5, 6);

            FirebaseUser decrypted = FirebaseUser.Decrypt(app, encrypted, 1, 2, 3, 4, 5, 6);

            Assert.Equivalent(user, decrypted);

            await user.DeleteUser();

            Assert.True(true);
        }
    }
}