namespace RestfulFirebase.FirestoreDatabase.Models.Fields
{
    public class StringField : Field<string>
    {
        public StringField(string? value)
        {
            Value = value;
        }
    }
}
