using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.CloudFirestore.Models.Fields;

namespace RestfulFirebase.CloudFirestore.Models
{
    public abstract class Field
    {
        public abstract Type Type { get; }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        public bool TryCast<FieldType>([MaybeNullWhen(false)] out FieldType? field)
#else
        public bool TryCast<FieldType>(out FieldType? field)
#endif
            where FieldType : Field
        {
            if (this is FieldType f)
            {
                field = f;
                return true;
            }
            field = default;
            return false;
        }

        public static Field Create(string type, string value)
        {
            return type switch
            {
                "stringValue" => new StringField(value),
                "integerValue" => new IntegerField(value),
                "doubleValue" => new DoubleField(value),
                _ => throw new NotSupportedException("The type \"" + type + "\" is not supported."),
            };
        }
    }

    public abstract class Field<T> : Field
    {
        public override Type Type { get => typeof(T); }

        protected T? Value { get; set; }

        public T? GetValue() => Value;

        public T GetValue(T defaultValue)
        {
            if (Value is null)
            {
                return defaultValue;
            }
            return Value;
        }
    }
}
