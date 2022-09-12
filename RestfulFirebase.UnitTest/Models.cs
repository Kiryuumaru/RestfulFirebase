using Newtonsoft.Json.Linq;
using RestfulFirebase.CloudFirestore.Query;
using RestfulFirebase.FirestoreDatabase.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RestfulFirebase.UnitTest;

public class ModelType
{
    public string? Val1 { get; set; }

    public string? Val2 { get; set; }
}

public class CustomSerializerModel1Type
{
    public double? Val1 { get; set; }

    public double? Val2 { get; set; }

    public class Converter : JsonConverter<CustomSerializerModel1Type>
    {
        public static Converter Instance { get; } = new();

        public override CustomSerializerModel1Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            string? json = reader.GetString();

            if (json == null)
            {
                throw new JsonException();
            }

            string[] jsonParts = json.Split(',');

            if (jsonParts.Length != 2)
            {
                throw new JsonException();
            }

            double? val1 = null;
            double? val2 = null;

            try
            {
                val1 = double.Parse(jsonParts[0]);
                val1 = double.Parse(jsonParts[1]);
            }
            catch { }

            return new CustomSerializerModel1Type()
            {
                Val1 = val1,
                Val2 = val2,
            };
        }

        public override void Write(Utf8JsonWriter writer, CustomSerializerModel1Type value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value.Val1},{value.Val2}");
        }
    }
}

public class Coordinates : IGeoPoint
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }
}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class AllType
{
    public object? NullValue1 { get; set; }

    public string? NullValue2 { get; set; }

    public int? NullValue3 { get; set; }

    public bool BooleanValue1 { get; set; }

    public bool BooleanValue2 { get; set; }

    public bool? BooleanValue1Null { get; set; }

    public bool? BooleanValue2Null { get; set; }

    public bool? BooleanValue3Null { get; set; }

    public sbyte IntegerValue1 { get; set; }

    public byte IntegerValue2 { get; set; }

    public short IntegerValue3 { get; set; }

    public ushort IntegerValue4 { get; set; }

    public int IntegerValue5 { get; set; }

    public uint IntegerValue6 { get; set; }

    public long IntegerValue7 { get; set; }

    public ulong IntegerValue8 { get; set; }

    public sbyte? IntegerValue1Null { get; set; }

    public byte? IntegerValue2Null { get; set; }

    public short? IntegerValue3Null { get; set; }

    public ushort? IntegerValue4Null { get; set; }

    public int? IntegerValue5Null { get; set; }

    public uint? IntegerValue6Null { get; set; }

    public long? IntegerValue7Null { get; set; }

    public ulong? IntegerValue8Null { get; set; }

    public sbyte? IntegerValue9Null { get; set; }

    public byte? IntegerValue10Null { get; set; }

    public short? IntegerValue11Null { get; set; }

    public ushort? IntegerValue12Null { get; set; }

    public int? IntegerValue13Null { get; set; }

    public uint? IntegerValue14Null { get; set; }

    public long? IntegerValue15Null { get; set; }

    public ulong? IntegerValue16Null { get; set; }

    public float DoubleValue1 { get; set; }

    public double DoubleValue2 { get; set; }

    public float? DoubleValue1Null { get; set; }

    public double? DoubleValue2Null { get; set; }

    public float? DoubleValue3Null { get; set; }

    public double? DoubleValue4Null { get; set; }

    public decimal DecimalValue1 { get; set; }

    public decimal? DecimalValue1Null { get; set; }

    public decimal? DecimalValue2Null { get; set; }

    public DateTime TimestampValue1 { get; set; }

    public DateTimeOffset TimestampValue2 { get; set; }

    public DateTime? TimestampValue1Null { get; set; }

    public DateTimeOffset? TimestampValue2Null { get; set; }

    public DateTime? TimestampValue3Null { get; set; }

    public DateTimeOffset? TimestampValue4Null { get; set; }

    public string StringValue1 { get; set; }

    public string? StringValue1Null { get; set; }

    public string? StringValue2Null { get; set; }

    public byte[] BytesValue1 { get; set; }

    public byte[]? BytesValue1Null { get; set; }

    public byte[]? BytesValue2Null { get; set; }

    public DocumentReference ReferenceValue1 { get; set; }

    public DocumentReference? ReferenceValue1Null { get; set; }

    public DocumentReference? ReferenceValue2Null { get; set; }

    public Coordinates GeoPointValue1 { get; set; }

    public Coordinates? GeoPointValue1Null { get; set; }

    public Coordinates? GeoPointValue2Null { get; set; }

    public string[] ArrayValue1 { get; set; }

    public string?[] ArrayValue2 { get; set; }

    public int[] ArrayValue3 { get; set; }

    public int?[] ArrayValue4 { get; set; }

    public ModelType[] ArrayValue5 { get; set; }

    public ModelType?[] ArrayValue6 { get; set; }

    public string[]? ArrayValue1Null { get; set; }

    public string?[]? ArrayValue2Null { get; set; }

    public int[]? ArrayValue3Null { get; set; }

    public int?[]? ArrayValue4Null { get; set; }

    public ModelType[]? ArrayValue5Null { get; set; }

    public ModelType?[]? ArrayValue6Null { get; set; }

    public string[]? ArrayValue7Null { get; set; }

    public string?[]? ArrayValue8Null { get; set; }

    public int[]? ArrayValue9Null { get; set; }

    public int?[]? ArrayValue10Null { get; set; }

    public ModelType[]? ArrayValue11Null { get; set; }

    public ModelType?[]? ArrayValue12Null { get; set; }

    public Dictionary<string, string?> MapValue1 { get; set; }

    public Dictionary<string, ModelType?> MapValue2 { get; set; }

    public Dictionary<int, string?> MapValue3 { get; set; }

    public ModelType MapValue4 { get; set; }

    public Dictionary<string, string?>? MapValue1Null { get; set; }

    public Dictionary<string, ModelType?>? MapValue2Null { get; set; }

    public Dictionary<int, string?>? MapValue3Null { get; set; }

    public ModelType? MapValue4Null { get; set; }

    public Dictionary<string, string?>? MapValue5Null { get; set; }

    public Dictionary<string, ModelType?>? MapValue6Null { get; set; }

    public Dictionary<int, string?>? MapValue7Null { get; set; }

    public ModelType? MapValue8Null { get; set; }

    public CustomSerializerModel1Type CustomSerializerModel1Type1 { get; set; }

    public CustomSerializerModel1Type CustomSerializerModel1Type2 { get; set; }

    public CustomSerializerModel1Type? CustomSerializerModel1Type1Null { get; set; }

    public static AllType Empty()
    {
        return new AllType();
    }

    public static AllType Filled1()
    {
        return new AllType()
        {
            NullValue1 = null,
            NullValue2 = null,
            NullValue3 = null,
            BooleanValue1 = true,
            BooleanValue2 = false,
            BooleanValue1Null = false,
            BooleanValue2Null = true,
            BooleanValue3Null = null,
            IntegerValue1 = 9,
            IntegerValue2 = 19,
            IntegerValue3 = 29,
            IntegerValue4 = 39,
            IntegerValue5 = 49,
            IntegerValue6 = 59,
            IntegerValue7 = 69,
            IntegerValue8 = 79,
            IntegerValue1Null = 1,
            IntegerValue2Null = 11,
            IntegerValue3Null = 21,
            IntegerValue4Null = 31,
            IntegerValue5Null = 41,
            IntegerValue6Null = 51,
            IntegerValue7Null = 61,
            IntegerValue8Null = 71,
            IntegerValue9Null = null,
            IntegerValue10Null = null,
            IntegerValue11Null = null,
            IntegerValue12Null = null,
            IntegerValue13Null = null,
            IntegerValue14Null = null,
            IntegerValue15Null = null,
            IntegerValue16Null = null,
            DoubleValue1 = 99.9F,
            DoubleValue2 = 99.99,
            DoubleValue1Null = 11.1F,
            DoubleValue2Null = 11.11,
            DoubleValue3Null = null,
            DoubleValue4Null = null,
            DecimalValue1 = 9999.9999M,
            DecimalValue1Null = 1111.1111M,
            DecimalValue2Null = null,
            TimestampValue1 = DateTime.UtcNow + TimeSpan.FromDays(9),
            TimestampValue2 = DateTimeOffset.UtcNow + TimeSpan.FromDays(19),
            TimestampValue1Null = DateTime.UtcNow + TimeSpan.FromDays(3),
            TimestampValue2Null = DateTimeOffset.UtcNow + TimeSpan.FromDays(13),
            TimestampValue3Null = null,
            TimestampValue4Null = null,
            StringValue1 = "test value",
            StringValue1Null = "test value",
            StringValue2Null = null,
            BytesValue1 = new byte[4] { 9, 8, 7, 6 },
            BytesValue1Null = new byte[4] { 5, 4, 3, 2 },
            BytesValue2Null = null,
            ReferenceValue1 = Api.FirestoreDatabase.Database()
                .Collection("collectionTest1")
                .Document("documentTest1")
                .Collection("collectionTest2")
                .Document("documentTest2"),
            ReferenceValue1Null = Api.FirestoreDatabase.Database()
                .Collection("collectionTest3")
                .Document("documentTest3")
                .Collection("collectionTest4")
                .Document("documentTest4"),
            ReferenceValue2Null = null,
            GeoPointValue1 = new Coordinates()
            {
                Latitude = 34.43,
                Longitude = 23.32
            },
            GeoPointValue1Null = new Coordinates()
            {
                Latitude = 21.12,
                Longitude = 24.42
            },
            GeoPointValue2Null = null,
            ArrayValue1 = new string[4] { "this1", "is1", "a1", "test1" },
            ArrayValue2 = new string[4] { "this2", "is2", "a2", "test2" },
            ArrayValue3 = new int[4] { 7, 6, 5, 4 },
            ArrayValue4 = new int?[4] { 7, 6, null, 4 },
            ArrayValue5 = new ModelType[3]
            {
                new ModelType() { Val1 = "51Test11Val1", Val2 = "51Test11Val2" },
                new ModelType() { Val1 = null, Val2 = "52Test11Val2" },
                new ModelType() { Val1 = "53Test11Val1", Val2 = null }
            },
            ArrayValue6 = new ModelType?[3]
            {
                new ModelType() { Val1 = "61Test11Val1", Val2 = "61Test11Val2" },
                new ModelType() { Val1 = "62Test11Val1", Val2 = null },
                new ModelType() { Val1 = null, Val2 = "63Test11Val2" }
            },
            ArrayValue1Null = new string[4] { "this11", "is11", "a11", "test11" },
            ArrayValue2Null = new string[4] { "this22", "is22", "a22", "test22" },
            ArrayValue3Null = new int[4] { 3, 4, 5, 6 },
            ArrayValue4Null = new int?[4] { 1, 2, null, 4 },
            ArrayValue5Null = new ModelType[3]
            {
                new ModelType() { Val1 = "51Test11Val11", Val2 = "51Test11Val22" },
                new ModelType() { Val1 = null, Val2 = "52Test11Val22" },
                new ModelType() { Val1 = "53Test11Val11", Val2 = null }
            },
            ArrayValue6Null = new ModelType?[3]
            {
                new ModelType() { Val1 = "61Test11Val11", Val2 = "61Test11Val22" },
                new ModelType() { Val1 = "62Test11Val11", Val2 = null },
                new ModelType() { Val1 = null, Val2 = "63Test11Val22" }
            },
            ArrayValue7Null = null,
            ArrayValue8Null = null,
            ArrayValue9Null = null,
            ArrayValue10Null = null,
            ArrayValue11Null = null,
            ArrayValue12Null = null,
            MapValue1 = new Dictionary<string, string?>()
            {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" }
            },
            MapValue2 = new Dictionary<string, ModelType?>()
            {
                { "key1", new ModelType() { Val1 = "val 1", Val2 = "val 2" } },
                { "key2", new ModelType() { Val1 = "val 3", Val2 = null } },
                { "key3", new ModelType() { Val1 = null, Val2 = "val 6" } },
            },
            MapValue3 = new Dictionary<int, string?>()
            {
                { 9, "value9" },
                { 8, "value8" },
                { 7, "value7" }
            },
            MapValue1Null = new Dictionary<string, string?>()
            {
                { "key11", "value11" },
                { "key22", "value22" },
                { "key33", "value33" }
            },
            MapValue2Null = new Dictionary<string, ModelType?>()
            {
                { "key11", new ModelType() { Val1 = "val 11", Val2 = "val 22" } },
                { "key22", new ModelType() { Val1 = null, Val2 = "val 33" } },
                { "key33", new ModelType() { Val1 = "val 66", Val2 = null } },
            },
            MapValue3Null = new Dictionary<int, string?>()
            {
                { 2, "value2" },
                { 3, "value3" },
                { 4, "value4" }
            },
            MapValue4Null = null,
            MapValue5Null = null,
            MapValue6Null = null,
            CustomSerializerModel1Type1 = new CustomSerializerModel1Type()
            {
                Val1 = 123.321,
                Val2 = 345.543,
            },
            CustomSerializerModel1Type2 = new CustomSerializerModel1Type()
            {
                Val1 = null,
                Val2 = 345.543,
            },
            CustomSerializerModel1Type1Null = null,
        };
    }

    public static AllType Filled2()
    {
        return new AllType()
        {
            NullValue1 = null,
            NullValue2 = null,
            NullValue3 = null,
            BooleanValue1 = false,
            BooleanValue2 = true,
            BooleanValue1Null = true,
            BooleanValue2Null = false,
            BooleanValue3Null = null,
            IntegerValue1 = 5,
            IntegerValue2 = 15,
            IntegerValue3 = 25,
            IntegerValue4 = 35,
            IntegerValue5 = 45,
            IntegerValue6 = 55,
            IntegerValue7 = 65,
            IntegerValue8 = 75,
            IntegerValue1Null = 7,
            IntegerValue2Null = 17,
            IntegerValue3Null = 27,
            IntegerValue4Null = 37,
            IntegerValue5Null = 47,
            IntegerValue6Null = 57,
            IntegerValue7Null = 67,
            IntegerValue8Null = 77,
            IntegerValue9Null = null,
            IntegerValue10Null = null,
            IntegerValue11Null = null,
            IntegerValue12Null = null,
            IntegerValue13Null = null,
            IntegerValue14Null = null,
            IntegerValue15Null = null,
            IntegerValue16Null = null,
            DoubleValue1 = 55.5F,
            DoubleValue2 = 55.55,
            DoubleValue1Null = 17.7F,
            DoubleValue2Null = 17.17,
            DoubleValue3Null = null,
            DoubleValue4Null = null,
            DecimalValue1 = 5555.5555M,
            DecimalValue1Null = 1177.1177M,
            DecimalValue2Null = null,
            TimestampValue1 = DateTime.UtcNow + TimeSpan.FromDays(5),
            TimestampValue2 = DateTimeOffset.UtcNow + TimeSpan.FromDays(15),
            TimestampValue1Null = DateTime.UtcNow + TimeSpan.FromDays(7),
            TimestampValue2Null = DateTimeOffset.UtcNow + TimeSpan.FromDays(17),
            TimestampValue3Null = null,
            TimestampValue4Null = null,
            StringValue1 = "test value",
            StringValue1Null = "another test value",
            StringValue2Null = null,
            BytesValue1 = new byte[6] { 4, 5, 6, 7, 8, 9 },
            BytesValue1Null = new byte[5] { 0, 9, 8, 7, 6 },
            BytesValue2Null = null,
            ReferenceValue1 = Api.FirestoreDatabase.Database()
                .Collection("collectionTest5")
                .Document("documentTest5")
                .Collection("collectionTest6")
                .Document("documentTest6"),
            ReferenceValue1Null = Api.FirestoreDatabase.Database()
                .Collection("collectionTest7")
                .Document("documentTest7")
                .Collection("collectionTest8")
                .Document("documentTest8"),
            ReferenceValue2Null = null,
            GeoPointValue1 = new Coordinates()
            {
                Latitude = 12.13,
                Longitude = 14.15
            },
            GeoPointValue1Null = new Coordinates()
            {
                Latitude = 33.3,
                Longitude = 34.4
            },
            GeoPointValue2Null = null,
            ArrayValue1 = new string[5] { "and1", "this1", "is1", "another1", "test1" },
            ArrayValue2 = new string[5] { "and2", "this2", "is2", "another2", "test2" },
            ArrayValue3 = new int[3] { 5, 4, 3 },
            ArrayValue4 = new int?[3] { null, 2, 1 },
            ArrayValue5 = new ModelType[3]
            {
                new ModelType() { Val1 = "51AnotherTest11Val1", Val2 = "51AnotherTest11Val2" },
                new ModelType() { Val1 = null, Val2 = "52AnotherTest11Val2" },
                new ModelType() { Val1 = "53AnotherTest11Val1", Val2 = null }
            },
            ArrayValue6 = new ModelType?[3]
            {
                new ModelType() { Val1 = "61AnotherTest11Val1", Val2 = "61AnotherTest11Val2" },
                new ModelType() { Val1 = "62AnotherTest11Val1", Val2 = null },
                new ModelType() { Val1 = null, Val2 = "63AnotherTest11Val2" }
            },
            ArrayValue1Null = new string[5] { "and11", "this11", "is11", "another11", "test11" },
            ArrayValue2Null = new string[5] { "and22", "this22", "is22", "another22", "test22" },
            ArrayValue3Null = new int[6] { 3, 4, 5, 6, 5, 6 },
            ArrayValue4Null = new int?[6] { 1, 2, null, 4, null, 4 },
            ArrayValue5Null = new ModelType[3]
            {
                new ModelType() { Val1 = "51AnotherTest11Val11", Val2 = "51AnotherTest11Val22" },
                new ModelType() { Val1 = null, Val2 = "52AnotherTest11Val22" },
                new ModelType() { Val1 = "53AnotherTest11Val11", Val2 = null }
            },
            ArrayValue6Null = new ModelType?[3]
            {
                new ModelType() { Val1 = "61AnotherTest11Val11", Val2 = "61AnotherTest11Val22" },
                new ModelType() { Val1 = "62AnotherTest11Val11", Val2 = null },
                new ModelType() { Val1 = null, Val2 = "63AnotherTest11Val22" }
            },
            ArrayValue7Null = null,
            ArrayValue8Null = null,
            ArrayValue9Null = null,
            ArrayValue10Null = null,
            ArrayValue11Null = null,
            ArrayValue12Null = null,
            MapValue1 = new Dictionary<string, string?>()
            {
                { "anotherKey1", "anotherValue1" },
                { "anotherKey2", "anotherValue2" },
                { "anotherKey3", "anotherValue3" },
                { "anotherKey4", "anotherValue4" }
            },
            MapValue2 = new Dictionary<string, ModelType?>()
            {
                { "anotherKey1", new ModelType() { Val1 = "another val 1", Val2 = "another val 2" } },
                { "anotherKey2", new ModelType() { Val1 = "another val 3", Val2 = null } },
                { "anotherKey3", new ModelType() { Val1 = null, Val2 = "another val 6" } },
                { "anotherKey4", new ModelType() { Val1 = null, Val2 = null } },
            },
            MapValue3 = new Dictionary<int, string?>()
            {
                { 2, "anotherValue2" },
                { 3, "anotherValue3" },
                { 4, "anotherValue4" }
            },
            MapValue1Null = new Dictionary<string, string?>()
            {
                { "anotherKey11", "anotherValue11" },
                { "anotherKey22", "anotherValue22" },
                { "anotherKey33", "anotherValue33" },
                { "anotherKey44", "anotherValue44" }
            },
            MapValue2Null = new Dictionary<string, ModelType?>()
            {
                { "anotherKey11", new ModelType() { Val1 = "another val 11", Val2 = "another val 22" } },
                { "anotherKey22", new ModelType() { Val1 = "another val 33", Val2 = null } },
                { "anotherKey33", new ModelType() { Val1 = null, Val2 = "another val 66" } },
                { "anotherKey44", new ModelType() { Val1 = null, Val2 = null } },
            },
            MapValue3Null = new Dictionary<int, string?>()
            {
                { 5, "anotherValue5" },
                { 4, "anotherValue4" },
                { 3, "anotherValue3" }
            },
            MapValue4Null = null,
            MapValue5Null = null,
            MapValue6Null = null,
            CustomSerializerModel1Type1 = new CustomSerializerModel1Type()
            {
                Val1 = 789.987,
                Val2 = 567.765,
            },
            CustomSerializerModel1Type2 = new CustomSerializerModel1Type()
            {
                Val1 = 12.21,
                Val2 = null,
            },
            CustomSerializerModel1Type1Null = null,
        };
    }
}

public class NestedType
{
    public AllType AllType1 { get; set; }

    public AllType[] ArrayAllType1 { get; set; }

    public AllType?[] ArrayAllType2 { get; set; }

    public List<AllType> ArrayAllType3 { get; set; }

    public List<AllType?> ArrayAllType4 { get; set; }

    public Dictionary<string, AllType> DictionaryAllType1 { get; set; }

    public Dictionary<string, AllType?> DictionaryAllType2 { get; set; }

    public Dictionary<int, AllType?> DictionaryAllType3 { get; set; }

    public static NestedType Empty()
    {
        return new NestedType();
    }

    public static NestedType Filled1()
    {
        return new NestedType()
        {
            AllType1 = AllType.Filled1(),
            ArrayAllType1 = new AllType[2]
            {
                AllType.Filled1(),
                AllType.Filled2(),
            },
            ArrayAllType2 = new AllType[2]
            {
                AllType.Filled2(),
                AllType.Filled1(),
            },
            ArrayAllType3 = new List<AllType>()
            {
                AllType.Filled1(),
                AllType.Filled2(),
            },
            ArrayAllType4 = new List<AllType?>()
            {
                AllType.Filled2(),
                AllType.Filled1(),
            },
            DictionaryAllType1 = new Dictionary<string, AllType>()
            {
                { "allTypeKey1",  AllType.Filled1() },
                { "allTypeKey2",  AllType.Filled2() }
            },
            DictionaryAllType2 = new Dictionary<string, AllType?>()
            {
                { "allTypeKey22",  AllType.Filled2() },
                { "allTypeKey11",  AllType.Filled1() }
            },
            DictionaryAllType3 = new Dictionary<int, AllType?>()
            {
                { 1,  AllType.Filled1() },
                { 2,  AllType.Filled2() }
            }
        };
    }

    public static NestedType Filled2()
    {
        return new NestedType()
        {
            AllType1 = AllType.Filled2(),
            ArrayAllType1 = new AllType[2]
            {
                AllType.Filled2(),
                AllType.Filled1(),
            },
            ArrayAllType2 = new AllType[2]
            {
                AllType.Filled1(),
                AllType.Filled2(),
            },
            ArrayAllType3 = new List<AllType>()
            {
                AllType.Filled2(),
                AllType.Filled1(),
            },
            ArrayAllType4 = new List<AllType?>()
            {
                AllType.Filled1(),
                AllType.Filled2(),
            },
            DictionaryAllType1 = new Dictionary<string, AllType>()
            {
                { "allTypeKey2",  AllType.Filled2() },
                { "allTypeKey1",  AllType.Filled1() }
            },
            DictionaryAllType2 = new Dictionary<string, AllType?>()
            {
                { "allTypeKey11",  AllType.Filled1() },
                { "allTypeKey22",  AllType.Filled2() }
            },
            DictionaryAllType3 = new Dictionary<int, AllType?>()
            {
                { 2,  AllType.Filled2() },
                { 1,  AllType.Filled1() }
            }
        };
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

