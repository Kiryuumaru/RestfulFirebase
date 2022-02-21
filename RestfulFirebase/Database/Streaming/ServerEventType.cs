namespace RestfulFirebase.Database.Streaming;

internal enum ServerEventType
{
    Put,

    Patch,

    KeepAlive,

    Cancel,

    AuthRevoked
}
