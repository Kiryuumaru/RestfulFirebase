namespace RestfulFirebase.Common.Utilities;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class ItemConverterDecorator<TItemConverter> : JsonConverterFactory
    where TItemConverter : JsonConverter, new()
{
    readonly TItemConverter itemConverter = new();

    public override bool CanConvert([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type typeToConvert)
    {
        var (itemType, _, _) = GetItemType(typeToConvert);

        if (itemType == null)
        {
            return false;
        }
            
        return itemConverter.CanConvert(itemType);
    }

    public override JsonConverter? CreateConverter([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type typeToConvert, JsonSerializerOptions options)
    {
        var (itemType, isArray, isSet) = GetItemType(typeToConvert);
        if (itemType == null)
        {
            return null;
        }
        if (isArray)
        {
            return (JsonConverter?)Activator.CreateInstance(typeof(ArrayItemConverterDecorator<>)
                .MakeGenericType(typeof(TItemConverter), itemType), new object[] { options, itemConverter });
        }
        if (!typeToConvert.IsAbstract && !typeToConvert.IsInterface && typeToConvert.GetConstructor(Type.EmptyTypes) != null)
        {
            return (JsonConverter?)Activator.CreateInstance(typeof(ConcreteCollectionItemConverterDecorator<,,>).MakeGenericType(typeof(TItemConverter), typeToConvert, typeToConvert, itemType), new object[] { options, itemConverter });
        }
        if (isSet)
        {
            var setType = typeof(HashSet<>).MakeGenericType(itemType);
            if (typeToConvert.IsAssignableFrom(setType))
            {
                return (JsonConverter?)Activator.CreateInstance(typeof(ConcreteCollectionItemConverterDecorator<,,>).MakeGenericType(typeof(TItemConverter), setType, typeToConvert, itemType), new object[] { options, itemConverter });
            }
        }
        else
        {
            var listType = typeof(List<>).MakeGenericType(itemType);
            if (typeToConvert.IsAssignableFrom(listType))
            {
                return (JsonConverter?)Activator.CreateInstance(typeof(ConcreteCollectionItemConverterDecorator<,,>).MakeGenericType(typeof(TItemConverter), listType, typeToConvert, itemType), new object[] { options, itemConverter });
            }
        }
        // OK it's not an array and we can't find a parameterless constructor for the type.  We can serialize, but not deserialize.
        return (JsonConverter?)Activator.CreateInstance(typeof(EnumerableItemConverterDecorator<,>).MakeGenericType(typeof(TItemConverter), typeToConvert, itemType), new object[] { options, itemConverter });
    }

    static (Type? Type, bool IsArray, bool isSet) GetItemType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
    {
        // Quick reject for performance
        // Dictionary is not implemented. 
        if (type.IsPrimitive || type == typeof(string) || typeof(IDictionary).IsAssignableFrom(type))
        {
            return (null, false, false);
        }
        if (type.IsArray)
        {
            return type.GetArrayRank() == 1 ? (type.GetElementType(), true, false) : (null, false, false);
        }
        Type? itemType = null;
        bool isSet = false;
        foreach (var iType in type.GetInterfacesAndSelf())
        {
            if (iType.IsGenericType)
            {
                var genType = iType.GetGenericTypeDefinition();
                if (genType == typeof(ISet<>))
                {
                    isSet = true;
                }
                else if (genType == typeof(IEnumerable<>))
                {
                    var thisItemType = iType.GetGenericArguments()[0];
                    if (itemType != null && itemType != thisItemType)
                    {
                        return (null, false, false); // type implements multiple enumerable types simultaneously.
                    }
                    itemType = thisItemType;
                }
                else if (genType == typeof(IDictionary<,>))
                {
                    return (null, false, false);
                }
            }
        }
        return (itemType, false, isSet);
    }

    abstract class CollectionItemConverterDecoratorBase<TEnumerable, TItem> : JsonConverter<TEnumerable> where TEnumerable : IEnumerable<TItem>
    {
        readonly JsonSerializerOptions modifiedOptions;

        public CollectionItemConverterDecoratorBase(JsonSerializerOptions options, TItemConverter converter)
        {
            // Clone the incoming options and insert the item converter at the beginning of the clone.
            // Then if converter is actually a JsonConverterFactory (e.g. JsonStringEnumConverter) then the correct JsonConverter<T> will be manufactured or fetched.
            modifiedOptions = new JsonSerializerOptions(options);
            modifiedOptions.Converters.Insert(0, converter);
        }

#if NET5_0_OR_GREATER
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
        protected TCollection BaseRead<TCollection>(ref Utf8JsonReader reader) where TCollection : ICollection<TItem>, new()
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException(); // Unexpected token type
            }
            var list = new TCollection();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }
#pragma warning disable CS8604 // Possible null reference argument.
                list.Add(JsonSerializer.Deserialize<TItem>(ref reader, modifiedOptions));
#pragma warning restore CS8604 // Possible null reference argument.
            }
            return list;
        }

#if NET5_0_OR_GREATER
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
#endif
        public sealed override void Write(Utf8JsonWriter writer, TEnumerable value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                JsonSerializer.Serialize(writer, item, modifiedOptions);
            }
            writer.WriteEndArray();
        }
    }

    sealed class ArrayItemConverterDecorator<TItem> : CollectionItemConverterDecoratorBase<TItem[], TItem>
    {
        public ArrayItemConverterDecorator(JsonSerializerOptions options, TItemConverter converter) : base(options, converter) { }
        
        public override TItem[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return BaseRead<List<TItem>>(ref reader)?.ToArray();
        }
    }

    sealed class ConcreteCollectionItemConverterDecorator<TCollection, TEnumerable, TItem> : CollectionItemConverterDecoratorBase<TEnumerable, TItem>
        where TCollection : ICollection<TItem>, TEnumerable, new()
        where TEnumerable : IEnumerable<TItem>
    {
        public ConcreteCollectionItemConverterDecorator(JsonSerializerOptions options, TItemConverter converter) : base(options, converter) { }
        
        public override TEnumerable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return BaseRead<TCollection>(ref reader);
        }
    }

    sealed class EnumerableItemConverterDecorator<TEnumerable, TItem> : CollectionItemConverterDecoratorBase<TEnumerable, TItem> where TEnumerable : IEnumerable<TItem>
    {
        public EnumerableItemConverterDecorator(JsonSerializerOptions options, TItemConverter converter) : base(options, converter) { }
        
        public override TEnumerable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(string.Format("Deserialization is not implemented for type {0}", typeof(TEnumerable)));
        }
    }
}

internal static class TypeExtensions
{
    public static IEnumerable<Type> GetInterfacesAndSelf([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] this Type type)
    {
        return (type ?? throw new ArgumentNullException(nameof(type))).IsInterface ?
            new[] { type }.Concat(type.GetInterfaces()) :
            type.GetInterfaces();
    }
}
