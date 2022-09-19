using ObservableHelpers.ComponentModel;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestfulFirebase.Common.Attributes;

namespace RestfulFirebase.UnitTest;

public class NormalModel
{
    public string? Val1 { get; set; }

    public string? Val2 { get; set; }
}

[ObservableObject]
public partial class NormalMVVMModel
{
    [ObservableProperty]
    string? val1;

    [ObservableProperty]
    string? val2;
}

[ObservableObject]
[FirebaseValueOnly]
public partial class MVVMModelWithIncludeOnlyAttribute
{
    [ObservableProperty]
    [FirebaseValue]
    string? val1;

    [ObservableProperty]
    [FirebaseValue(Name = "value2")]
    string? val2;

    [ObservableProperty]
    string? val3;
}

public class ModelWithCustomSerializer
{
    public double? Val1 { get; set; }

    public double? Val2 { get; set; }

    public class Converter : JsonConverter<ModelWithCustomSerializer>
    {
        public static Converter Instance { get; } = new();

        public override ModelWithCustomSerializer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!JsonDocument.TryParseValue(ref reader, out JsonDocument? document))
            {
                return null;
            }

            JsonProperty jsonProperty = document.RootElement.EnumerateObject().FirstOrDefault();

            string[]? jsonParts = jsonProperty.Value.GetString()?.Split(',');

            if (jsonParts == null || jsonParts.Length != 2)
            {
                return null;
            }

            double? val1 = null;
            double? val2 = null;

            try
            {
                val1 = double.Parse(jsonParts[0]);
            }
            catch { }
            try
            {
                val2 = double.Parse(jsonParts[1]);
            }
            catch { }

            return new()
            {
                Val1 = val1,
                Val2 = val2,
            };
        }

        public override void Write(Utf8JsonWriter writer, ModelWithCustomSerializer value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("stringValue");
            writer.WriteStringValue($"{(value.Val1.HasValue ? value.Val1 : "null")},{(value.Val2.HasValue ? value.Val2 : "null")}");
            writer.WriteEndObject();
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

    public NormalModel[] ArrayValue5 { get; set; }

    public NormalModel?[] ArrayValue6 { get; set; }

    public string[]? ArrayValue1Null { get; set; }

    public string?[]? ArrayValue2Null { get; set; }

    public int[]? ArrayValue3Null { get; set; }

    public int?[]? ArrayValue4Null { get; set; }

    public NormalModel[]? ArrayValue5Null { get; set; }

    public NormalModel?[]? ArrayValue6Null { get; set; }

    public string[]? ArrayValue7Null { get; set; }

    public string?[]? ArrayValue8Null { get; set; }

    public int[]? ArrayValue9Null { get; set; }

    public int?[]? ArrayValue10Null { get; set; }

    public NormalModel[]? ArrayValue11Null { get; set; }

    public NormalModel?[]? ArrayValue12Null { get; set; }

    public Dictionary<string, string?> MapValue1 { get; set; }

    public Dictionary<string, NormalModel?> MapValue2 { get; set; }

    public Dictionary<int, string?> MapValue3 { get; set; }

    public NormalModel MapValue4 { get; set; }

    public Dictionary<string, string?>? MapValue1Null { get; set; }

    public Dictionary<string, NormalModel?>? MapValue2Null { get; set; }

    public Dictionary<int, string?>? MapValue3Null { get; set; }

    public Dictionary<string, string?>? MapValue4Null { get; set; }

    public Dictionary<string, NormalModel?>? MapValue5Null { get; set; }

    public Dictionary<int, string?>? MapValue7Null { get; set; }

    public NormalModel? MapValue8Null { get; set; }

    public NormalModel NormalModel1 { get; set; }

    public NormalModel NormalModel2 { get; set; }

    public NormalModel? NormalModel3 { get; set; }

    public NormalMVVMModel NormalMVVMModel1 { get; set; }

    public NormalMVVMModel NormalMVVMModel2 { get; set; }

    public NormalMVVMModel? NormalMVVMModel3 { get; set; }

    public MVVMModelWithIncludeOnlyAttribute MVVMModelWithIncludeOnlyAttribute1 { get; set; }

    public MVVMModelWithIncludeOnlyAttribute MVVMModelWithIncludeOnlyAttribute2 { get; set; }

    public MVVMModelWithIncludeOnlyAttribute? MVVMModelWithIncludeOnlyAttribute3 { get; set; }

    public ModelWithCustomSerializer ModelWithCustomSerializer1 { get; set; }

    public ModelWithCustomSerializer ModelWithCustomSerializer2 { get; set; }

    public ModelWithCustomSerializer? ModelWithCustomSerializer3 { get; set; }

    public static AllType Empty()
    {
        return new AllType()
        {
            NullValue1 = null,
            NullValue2 = null,
            NullValue3 = null,
            BooleanValue1 = false,
            BooleanValue2 = false,
            BooleanValue1Null = false,
            BooleanValue2Null = false,
            BooleanValue3Null = false,
            IntegerValue1 = 0,
            IntegerValue2 = 0,
            IntegerValue3 = 0,
            IntegerValue4 = 0,
            IntegerValue5 = 0,
            IntegerValue6 = 0,
            IntegerValue7 = 0,
            IntegerValue8 = 0,
            IntegerValue1Null = 0,
            IntegerValue2Null = 0,
            IntegerValue3Null = 0,
            IntegerValue4Null = 0,
            IntegerValue5Null = 0,
            IntegerValue6Null = 0,
            IntegerValue7Null = 0,
            IntegerValue8Null = 0,
            IntegerValue9Null = 0,
            IntegerValue10Null = 0,
            IntegerValue11Null = 0,
            IntegerValue12Null = 0,
            IntegerValue13Null = 0,
            IntegerValue14Null = 0,
            IntegerValue15Null = 0,
            IntegerValue16Null = 0,
            DoubleValue1 = 0,
            DoubleValue2 = 0,
            DoubleValue1Null = 0,
            DoubleValue2Null = 0,
            DoubleValue3Null = 0,
            DoubleValue4Null = 0,
            DecimalValue1 = 0,
            DecimalValue1Null = 0,
            DecimalValue2Null = 0,
            TimestampValue1 = new DateTime(),
            TimestampValue2 = new DateTimeOffset(),
            TimestampValue1Null = new DateTime(),
            TimestampValue2Null = new DateTimeOffset(),
            TimestampValue3Null = new DateTime(),
            TimestampValue4Null = new DateTimeOffset(),
            StringValue1 = "",
            StringValue1Null = "",
            StringValue2Null = "",
            BytesValue1 = Array.Empty<byte>(),
            BytesValue1Null = Array.Empty<byte>(),
            BytesValue2Null = Array.Empty<byte>(),
            ReferenceValue1 = Api.FirestoreDatabase.Collection("a").Document("b"),
            ReferenceValue1Null = Api.FirestoreDatabase.Collection("a").Document("b"),
            ReferenceValue2Null = Api.FirestoreDatabase.Collection("a").Document("b"),
            GeoPointValue1 = new Coordinates(),
            GeoPointValue1Null = new Coordinates(),
            GeoPointValue2Null = new Coordinates(),
            ArrayValue1 = Array.Empty<string>(),
            ArrayValue2 = Array.Empty<string>(),
            ArrayValue3 = Array.Empty<int>(),
            ArrayValue4 = Array.Empty<int?>(),
            ArrayValue5 = Array.Empty<NormalModel>(),
            ArrayValue6 = Array.Empty<NormalModel?>(),
            ArrayValue1Null = Array.Empty<string>(),
            ArrayValue2Null = Array.Empty<string>(),
            ArrayValue3Null = Array.Empty<int>(),
            ArrayValue4Null = Array.Empty<int?>(),
            ArrayValue5Null = Array.Empty<NormalModel>(),
            ArrayValue6Null = Array.Empty<NormalModel?>(),
            ArrayValue7Null = Array.Empty<string>(),
            ArrayValue8Null = Array.Empty<string?>(),
            ArrayValue9Null = Array.Empty<int>(),
            ArrayValue10Null = Array.Empty<int?>(),
            ArrayValue11Null = Array.Empty<NormalModel>(),
            ArrayValue12Null = Array.Empty<NormalModel?>(),
            MapValue1 = new Dictionary<string, string?>(),
            MapValue2 = new Dictionary<string, NormalModel?>(),
            MapValue3 = new Dictionary<int, string?>(),
            MapValue1Null = new Dictionary<string, string?>(),
            MapValue2Null = new Dictionary<string, NormalModel?>(),
            MapValue3Null = new Dictionary<int, string?>(),
            MapValue4Null = new Dictionary<string, string?>(),
            MapValue5Null = new Dictionary<string, NormalModel?>(),
            NormalModel1 = new NormalModel(),
            NormalModel2 = new NormalModel(),
            NormalModel3 = null,
            NormalMVVMModel1 = new NormalMVVMModel(),
            NormalMVVMModel2 = new NormalMVVMModel(),
            NormalMVVMModel3 = null,
            MVVMModelWithIncludeOnlyAttribute1 = new MVVMModelWithIncludeOnlyAttribute(),
            MVVMModelWithIncludeOnlyAttribute2 = new MVVMModelWithIncludeOnlyAttribute(),
            MVVMModelWithIncludeOnlyAttribute3 = new MVVMModelWithIncludeOnlyAttribute(),
            ModelWithCustomSerializer1 = new ModelWithCustomSerializer(),
            ModelWithCustomSerializer2 = new ModelWithCustomSerializer(),
            ModelWithCustomSerializer3 = new ModelWithCustomSerializer(),
        };
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
            ReferenceValue1 = Api.FirestoreDatabase
                .Collection("collectionTest1")
                .Document("documentTest1")
                .Collection("collectionTest2")
                .Document("documentTest2"),
            ReferenceValue1Null = Api.FirestoreDatabase
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
            ArrayValue5 = new NormalModel[3]
            {
                new NormalModel() { Val1 = "51Test11Val1", Val2 = "51Test11Val2" },
                new NormalModel() { Val1 = null, Val2 = "52Test11Val2" },
                new NormalModel() { Val1 = "53Test11Val1", Val2 = null }
            },
            ArrayValue6 = new NormalModel?[3]
            {
                new NormalModel() { Val1 = "61Test11Val1", Val2 = "61Test11Val2" },
                new NormalModel() { Val1 = "62Test11Val1", Val2 = null },
                new NormalModel() { Val1 = null, Val2 = "63Test11Val2" }
            },
            ArrayValue1Null = new string[4] { "this11", "is11", "a11", "test11" },
            ArrayValue2Null = new string[4] { "this22", "is22", "a22", "test22" },
            ArrayValue3Null = new int[4] { 3, 4, 5, 6 },
            ArrayValue4Null = new int?[4] { 1, 2, null, 4 },
            ArrayValue5Null = new NormalModel[3]
            {
                new NormalModel() { Val1 = "51Test11Val11", Val2 = "51Test11Val22" },
                new NormalModel() { Val1 = null, Val2 = "52Test11Val22" },
                new NormalModel() { Val1 = "53Test11Val11", Val2 = null }
            },
            ArrayValue6Null = new NormalModel?[3]
            {
                new NormalModel() { Val1 = "61Test11Val11", Val2 = "61Test11Val22" },
                new NormalModel() { Val1 = "62Test11Val11", Val2 = null },
                new NormalModel() { Val1 = null, Val2 = "63Test11Val22" }
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
            MapValue2 = new Dictionary<string, NormalModel?>()
            {
                { "key1", new NormalModel() { Val1 = "val 1", Val2 = "val 2" } },
                { "key2", new NormalModel() { Val1 = "val 3", Val2 = null } },
                { "key3", new NormalModel() { Val1 = null, Val2 = "val 6" } },
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
            MapValue2Null = new Dictionary<string, NormalModel?>()
            {
                { "key11", new NormalModel() { Val1 = "val 11", Val2 = "val 22" } },
                { "key22", new NormalModel() { Val1 = null, Val2 = "val 33" } },
                { "key33", new NormalModel() { Val1 = "val 66", Val2 = null } },
            },
            MapValue3Null = new Dictionary<int, string?>()
            {
                { 2, "value2" },
                { 3, "value3" },
                { 4, "value4" }
            },
            MapValue4Null = null,
            MapValue5Null = null,
            NormalModel1 = new NormalModel()
            {
                Val1 = "normal val 11",
                Val2 = "normal val 12",
            },
            NormalModel2 = new NormalModel()
            {
                Val1 = "normal val 21",
                Val2 = null,
            },
            NormalModel3 = null,
            NormalMVVMModel1 = new NormalMVVMModel()
            {
                Val1 = "normal mvvm val 11",
                Val2 = "normal mvvm val 12",
            },
            NormalMVVMModel2 = new NormalMVVMModel()
            {
                Val1 = null,
                Val2 = "normal mvvm val 22",
            },
            NormalMVVMModel3 = null,
            MVVMModelWithIncludeOnlyAttribute1 = new MVVMModelWithIncludeOnlyAttribute()
            {
                Val1 = "normal mvvm val include only 11",
                Val2 = "normal mvvm val include only 12",
                Val3 = "normal mvvm val include only 13"
            },
            MVVMModelWithIncludeOnlyAttribute2 = new MVVMModelWithIncludeOnlyAttribute()
            {
                Val1 = null,
                Val2 = "normal mvvm val include only 22",
                Val3 = "normal mvvm val include only 23"
            },
            MVVMModelWithIncludeOnlyAttribute3 = null,
            ModelWithCustomSerializer1 = new ModelWithCustomSerializer()
            {
                Val1 = 123.321,
                Val2 = 345.543,
            },
            ModelWithCustomSerializer2 = new ModelWithCustomSerializer()
            {
                Val1 = null,
                Val2 = 345.543,
            },
            ModelWithCustomSerializer3 = null,
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
            ReferenceValue1 = Api.FirestoreDatabase
                .Collection("collectionTest5")
                .Document("documentTest5")
                .Collection("collectionTest6")
                .Document("documentTest6"),
            ReferenceValue1Null = Api.FirestoreDatabase
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
            ArrayValue5 = new NormalModel[3]
            {
                new NormalModel() { Val1 = "51AnotherTest11Val1", Val2 = "51AnotherTest11Val2" },
                new NormalModel() { Val1 = null, Val2 = "52AnotherTest11Val2" },
                new NormalModel() { Val1 = "53AnotherTest11Val1", Val2 = null }
            },
            ArrayValue6 = new NormalModel?[3]
            {
                new NormalModel() { Val1 = "61AnotherTest11Val1", Val2 = "61AnotherTest11Val2" },
                new NormalModel() { Val1 = "62AnotherTest11Val1", Val2 = null },
                new NormalModel() { Val1 = null, Val2 = "63AnotherTest11Val2" }
            },
            ArrayValue1Null = new string[5] { "and11", "this11", "is11", "another11", "test11" },
            ArrayValue2Null = new string[5] { "and22", "this22", "is22", "another22", "test22" },
            ArrayValue3Null = new int[6] { 3, 4, 5, 6, 5, 6 },
            ArrayValue4Null = new int?[6] { 1, 2, null, 4, null, 4 },
            ArrayValue5Null = new NormalModel[3]
            {
                new NormalModel() { Val1 = "51AnotherTest11Val11", Val2 = "51AnotherTest11Val22" },
                new NormalModel() { Val1 = null, Val2 = "52AnotherTest11Val22" },
                new NormalModel() { Val1 = "53AnotherTest11Val11", Val2 = null }
            },
            ArrayValue6Null = new NormalModel?[3]
            {
                new NormalModel() { Val1 = "61AnotherTest11Val11", Val2 = "61AnotherTest11Val22" },
                new NormalModel() { Val1 = "62AnotherTest11Val11", Val2 = null },
                new NormalModel() { Val1 = null, Val2 = "63AnotherTest11Val22" }
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
            MapValue2 = new Dictionary<string, NormalModel?>()
            {
                { "anotherKey1", new NormalModel() { Val1 = "another val 1", Val2 = "another val 2" } },
                { "anotherKey2", new NormalModel() { Val1 = "another val 3", Val2 = null } },
                { "anotherKey3", new NormalModel() { Val1 = null, Val2 = "another val 6" } },
                { "anotherKey4", new NormalModel() { Val1 = null, Val2 = null } },
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
            MapValue2Null = new Dictionary<string, NormalModel?>()
            {
                { "anotherKey11", new NormalModel() { Val1 = "another val 11", Val2 = "another val 22" } },
                { "anotherKey22", new NormalModel() { Val1 = "another val 33", Val2 = null } },
                { "anotherKey33", new NormalModel() { Val1 = null, Val2 = "another val 66" } },
                { "anotherKey44", new NormalModel() { Val1 = null, Val2 = null } },
            },
            MapValue3Null = new Dictionary<int, string?>()
            {
                { 5, "anotherValue5" },
                { 4, "anotherValue4" },
                { 3, "anotherValue3" }
            },
            MapValue4Null = null,
            MapValue5Null = null,
            NormalModel1 = new NormalModel()
            {
                Val1 = "another normal val 11",
                Val2 = "another normal val 12",
            },
            NormalModel2 = new NormalModel()
            {
                Val1 = null,
                Val2 = "another normal val 22",
            },
            NormalModel3 = null,
            NormalMVVMModel1 = new NormalMVVMModel()
            {
                Val1 = "another normal mvvm val 11",
                Val2 = "another normal mvvm val 12",
            },
            NormalMVVMModel2 = new NormalMVVMModel()
            {
                Val1 = "another normal mvvm val 21",
                Val2 = null,
            },
            NormalMVVMModel3 = null,
            MVVMModelWithIncludeOnlyAttribute1 = new MVVMModelWithIncludeOnlyAttribute()
            {
                Val1 = "another normal mvvm val include only 11",
                Val2 = "another normal mvvm val include only 12",
                Val3 = "another normal mvvm val include only 13"
            },
            MVVMModelWithIncludeOnlyAttribute2 = new MVVMModelWithIncludeOnlyAttribute()
            {
                Val1 = "another normal mvvm val include only 21",
                Val2 = null,
                Val3 = "another normal mvvm val include only 23"
            },
            MVVMModelWithIncludeOnlyAttribute3 = null,
            ModelWithCustomSerializer1 = new ModelWithCustomSerializer()
            {
                Val1 = 789.987,
                Val2 = 567.765,
            },
            ModelWithCustomSerializer2 = new ModelWithCustomSerializer()
            {
                Val1 = 12.21,
                Val2 = null,
            },
            ModelWithCustomSerializer3 = null,
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
        return new NestedType()
        {
            AllType1 = AllType.Empty(),
            ArrayAllType1 = Array.Empty<AllType>(),
            ArrayAllType2 = Array.Empty<AllType>(),
            ArrayAllType3 = new List<AllType>(),
            ArrayAllType4 = new List<AllType?>(),
            DictionaryAllType1 = new Dictionary<string, AllType>(),
            DictionaryAllType2 = new Dictionary<string, AllType?>(),
            DictionaryAllType3 = new Dictionary<int, AllType?>()
        };
    }

    public static NestedType Filled1()
    {
        return new NestedType()
        {
            AllType1 = AllType.Filled1(),
            ArrayAllType1 = new AllType[3]
            {
                AllType.Empty(),
                AllType.Filled1(),
                AllType.Filled2(),
            },
            ArrayAllType2 = new AllType[3]
            {
                AllType.Filled2(),
                AllType.Empty(),
                AllType.Filled1(),
            },
            ArrayAllType3 = new List<AllType>()
            {
                AllType.Filled1(),
                AllType.Filled2(),
                AllType.Empty(),
            },
            ArrayAllType4 = new List<AllType?>()
            {
                AllType.Empty(),
                AllType.Filled2(),
                AllType.Filled1(),
            },
            DictionaryAllType1 = new Dictionary<string, AllType>()
            {
                { "allTypeKey1",  AllType.Filled1() },
                { "allTypeKey2",  AllType.Empty() },
                { "allTypeKey3",  AllType.Filled2() },
            },
            DictionaryAllType2 = new Dictionary<string, AllType?>()
            {
                { "allTypeKey11",  AllType.Filled2() },
                { "allTypeKey22",  AllType.Filled1() },
                { "allTypeKey33",  AllType.Empty() },
            },
            DictionaryAllType3 = new Dictionary<int, AllType?>()
            {
                { 1,  AllType.Empty() },
                { 2,  AllType.Filled1() },
                { 3,  AllType.Filled2() },
            }
        };
    }

    public static NestedType Filled2()
    {
        return new NestedType()
        {
            AllType1 = AllType.Filled2(),
            ArrayAllType1 = new AllType[3]
            {
                AllType.Filled2(),
                AllType.Filled1(),
                AllType.Empty(),
            },
            ArrayAllType2 = new AllType[3]
            {
                AllType.Filled1(),
                AllType.Empty(),
                AllType.Filled2(),
            },
            ArrayAllType3 = new List<AllType>()
            {
                AllType.Empty(),
                AllType.Filled2(),
                AllType.Filled1(),
            },
            ArrayAllType4 = new List<AllType?>()
            {
                AllType.Filled1(),
                AllType.Filled2(),
                AllType.Empty(),
            },
            DictionaryAllType1 = new Dictionary<string, AllType>()
            {
                { "allTypeKey1",  AllType.Filled2() },
                { "allTypeKey2",  AllType.Empty() },
                { "allTypeKey3",  AllType.Filled1() }
            },
            DictionaryAllType2 = new Dictionary<string, AllType?>()
            {
                { "allTypeKey11",  AllType.Empty() },
                { "allTypeKey22",  AllType.Filled1() },
                { "allTypeKey33",  AllType.Filled2() }
            },
            DictionaryAllType3 = new Dictionary<int, AllType?>()
            {
                { 1,  AllType.Filled2() },
                { 2,  AllType.Filled1() },
                { 3,  AllType.Empty() },
            }
        };
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

