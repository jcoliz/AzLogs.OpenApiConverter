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

foreach (var declaration in streamDeclarations!.AsObject())
{
    Console.WriteLine("***{0}***", declaration.Key);

    var columns_json = declaration.Value!["columns"];

    var columns = JsonSerializer.Deserialize<List<Column>>(columns_json!.ToJsonString());

    foreach(var column in columns)
    {
        Console.WriteLine("\t{0}/{1}/{2}", column.name, column.type, column.description);
    }
}

public record Column
{
    public string name { get; set; }
    public string type { get; set; }
    public string description { get; set; }
};
