using RestfulFirebase;

FirebaseConfig config;
RestfulFirebaseApp app;

config = new FirebaseConfig("<Your project ID>", "<Your API key>");

app = new RestfulFirebaseApp(config);