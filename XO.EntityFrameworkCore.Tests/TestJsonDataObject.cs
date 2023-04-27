using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XO.EntityFrameworkCore;

internal sealed record TestJsonDataObject(
    string Name,
    int Age,
    List<string>? Aliases = null,
    string? Description = null,
    int? Dependants = null,
    [property:JsonPropertyName("DEETZ")]
    TestJsonDataAttributes? Attributes = null);

internal sealed record TestJsonDataAttributes(
    string? Occupation = null,
    string? Address = null);
