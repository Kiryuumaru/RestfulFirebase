namespace RestfulFirebase.FirestoreDatabase.Models
{
    /// <summary>
    /// The supported value types.
    /// </summary>
    public sealed class Value
    {

        //
        // Summary:
        //     Enum of possible cases for the "value_type" oneof.
        public enum ValueTypeOneofCase
        {
            None = 0,
            NullValue = 11,
            BooleanValue = 1,
            IntegerValue = 2,
            DoubleValue = 3,
            TimestampValue = 10,
            StringValue = 17,
            BytesValue = 18,
            ReferenceValue = 5,
            GeoPointValue = 8,
            ArrayValue = 9,
            MapValue = 6
        }
    }
}
