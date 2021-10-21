namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the specified serializer is not supported.
    /// </summary>
    public class SerializerInvalidValueProvided : SerializerException
    {
        internal SerializerInvalidValueProvided()
            : base("The provided value is invalid serialized data.")
        {

        }
    }
}
