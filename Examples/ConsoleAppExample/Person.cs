using System.Text.Json.Serialization;

namespace ConsoleAppExample;

public class Person
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    [JsonPropertyName("value1")]
    public string? Others { get; set; }
}
