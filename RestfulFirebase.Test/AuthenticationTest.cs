using RestfulFirebase;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Test.Utilities;
using RestfulFirebase.Test;
using Xunit;

namespace AuthenticationTest
{
    public class Login
    {
        [Fact]
        public async void Normal()
        {
            var app = new RestfulFirebaseApp(Config.YourConfig());
            await app.Auth.SignInWithEmailAndPassword("t@st.com", "123123");

            Assert.True(app.Auth.IsAuthenticated);
        }

        [Fact]
        public async void EmailNotFound()
        {
            var app = new RestfulFirebaseApp(Config.YourConfig());

            await Assert.ThrowsAsync<AuthEmailNotFoundException>(() => app.Auth.SignInWithEmailAndPassword("unknown@email.com", "123123"));
        }

        [Fact]
        public async void WrongPassword()
        {
            var app = new RestfulFirebaseApp(Config.YourConfig());

            await Assert.ThrowsAsync<AuthInvalidPasswordException>(() => app.Auth.SignInWithEmailAndPassword("t@st.com", "wrong"));
        }
    }
}