using RestfulFirebase.Authentication;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestfulFirebase.UnitTest
{
    public class AuthenticationTest
    {
        [Fact]
        public async void Test1()
        {
            FirebaseApp app = Helpers.GetFirebaseApp();

            FirebaseUser user;

            var loginRequest = await app.Authentication.SignInWithEmailAndPassword("test@mail.com", "123123");

            List<(string?, string?, string?)> logins = new();
            List<(string?, string?, string?)> creates = new();

            foreach (var transac in loginRequest.HttpTransactions)
            {
                logins.Add((transac.RequestUrl, await transac.GetRequestContentAsString(), await transac.GetResponseContentAsString()));
            }

            if (loginRequest.IsSuccess)
            {
                user = loginRequest.Result;
            }
            else
            {
                var signupRequest = await app.Authentication.CreateUserWithEmailAndPassword("test@mail.com", "123123", false);

                foreach (var transac in signupRequest.HttpTransactions)
                {
                    creates.Add((transac.RequestUrl, await transac.GetRequestContentAsString(), await transac.GetResponseContentAsString()));
                }

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
