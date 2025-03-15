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
Console.WriteLine("  schema:");

foreach (var declaration in streamDeclarations!.AsObject())
{
    Console.WriteLine("    {0}:", declaration.Key);
    Console.WriteLine("      type: object");
    Console.WriteLine("      additionalProperties: false");
    Console.WriteLine("      properties:");

    var columns_json = declaration.Value!["columns"];

    var columns = JsonSerializer.Deserialize<List<Column>>(columns_json!.ToJsonString());

    foreach(var column in columns!)
    {
        Console.WriteLine("        {0}:", column.name);
        Console.WriteLine("          description: {0}", column.description);

        switch (column.type)
        {
            case "real":
                if (column.description!.Contains("integer") || column.description!.Contains("number"))
                {
                    Console.WriteLine("          type: integer");
                }
                else
                {
                    Console.WriteLine("          type: number");
                    Console.WriteLine("          format: double");
                }
                break;
            case "datetime":
                Console.WriteLine("          type: string");
                Console.WriteLine("          format: date-time");
                break;
            case "string":
                Console.WriteLine("          type: string");
                if (column.description!.Contains("unique"))
                {
                    Console.WriteLine("          format: guid");
                }
                else if (column.description!.Contains("timestamp"))
                {
                    Console.WriteLine("          format: date-time");
                }
                break;
            default:
                Console.WriteLine("          type: {0}", column.type);
                break;
        }
    }
}

public record Column
{
    public string name { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
};
