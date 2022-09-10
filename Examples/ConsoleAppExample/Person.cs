using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConsoleAppExample;

public class Person
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    [JsonPropertyName("value1")]
    public string? Others { get; set; }
}
