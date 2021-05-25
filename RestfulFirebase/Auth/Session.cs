using RestfulFirebase.Extensions;
using RestfulFirebase.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Auth
{
    public class Session
    {
        private const string AuthRoot = "auth";

        public RestfulFirebaseApp App { get; }

        public string FirebaseToken
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "tok"));
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "tok"), value);
        }

        public string RefreshToken
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "ref"));
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "ref"), value);
        }

        public int ExpiresIn
        {
            get => Serializer.Deserialize<int>(App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "exp")));
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "exp"), Serializer.Serialize(value));
        }

        public DateTime Created
        {
            get => Serializer.Deserialize<DateTime>(App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "ctd")));
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "ctd"), Serializer.Serialize(value));
        }

        public string LocalId
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "lid")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "lid"), value);
        }

        public string FederatedId
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "fid")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "fid"), value);
        }

        public string FirstName
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "fname")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "fname"), value);
        }

        public string LastName
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "lname")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "lname"), value);
        }

        public string DisplayName
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "dname")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "dname"), value);
        }

        public string Email
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "email")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "email"), value);
        }

        public bool IsEmailVerified
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "vmail")) == "1";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "vmail"), value ? "1" : "0");
        }

        public string PhotoUrl
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "purl")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "purl"), value);
        }

        public string PhoneNumber
        {
            get => App.LocalDatabase.Get(Utils.UrlCombine(AuthRoot, "pnum")) ?? "";
            private set => App.LocalDatabase.Set(Utils.UrlCombine(AuthRoot, "pnum"), value);
        }

        public bool Exist
        {
            get
            {
                return
                    !string.IsNullOrEmpty(FirebaseToken) &&
                    !string.IsNullOrEmpty(RefreshToken);
            }
        }

        public Session(RestfulFirebaseApp app)
        {
            App = app;
        }

        internal void UpdateAuth(FirebaseAuth auth)
        {
            if (!string.IsNullOrEmpty(auth.FirebaseToken)) FirebaseToken = auth.FirebaseToken;
            if (!string.IsNullOrEmpty(auth.RefreshToken)) RefreshToken = auth.RefreshToken;
            if (auth.ExpiresIn.HasValue) ExpiresIn = auth.ExpiresIn.Value;
            if (auth.Created.HasValue) Created = auth.Created.Value;
            if (auth.User != null) UpdateUserInfo(auth.User);
        }

        internal void UpdateUserInfo(User user)
        {
            LocalId = user.LocalId;
            FederatedId = user.FederatedId;
            FirstName = user.FirstName;
            LastName = user.LastName;
            DisplayName = user.DisplayName;
            Email = user.Email;
            IsEmailVerified = user.IsEmailVerified;
            PhotoUrl = user.PhoneNumber;
            PhoneNumber = user.PhoneNumber;
        }

        internal void Purge()
        {
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "ctd"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "exp"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "ref"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "tok"));

            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "lid"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "fid"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "fname"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "lname"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "dname"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "email"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "vmail"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "purl"));
            App.LocalDatabase.Delete(Utils.UrlCombine(AuthRoot, "pnum"));
        }

        public bool IsExpired()
        {
            return DateTime.Now > Created.AddSeconds(ExpiresIn - 10);
        }
    }
}
