using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

var assy = Assembly.GetExecutingAssembly();
var names = assy.GetManifestResourceNames();
var name = names.Where(x => x.EndsWith(".dcr.json")).FirstOrDefault();
var stream = assy.GetManifestResourceStream(name!);

var node = await JsonNode.ParseAsync(stream!);

var streamDeclarations = node!["properties"]!["streamDeclarations"];

Console.WriteLine("components:");
Console.WriteLine("\tschema:");

foreach (var declaration in streamDeclarations!.AsObject())
{
    Console.WriteLine("\t\t{0}:", declaration.Key);
    Console.WriteLine("\t\t\ttype: object");
    Console.WriteLine("\t\t\tadditionalProperties: false");
    Console.WriteLine("\t\t\tproperties:");

    var columns_json = declaration.Value!["columns"];

    var columns = JsonSerializer.Deserialize<List<Column>>(columns_json!.ToJsonString());

    foreach(var column in columns)
    {
        Console.WriteLine("\t\t\t\t{0}:", column.name);
        Console.WriteLine("\t\t\t\t\ttype: {0}", column.type);
        Console.WriteLine("\t\t\t\t\tdescription: {0}", column.description);
    }
}

public record Column
{
    public string name { get; set; }
    public string type { get; set; }
    public string description { get; set; }
};
