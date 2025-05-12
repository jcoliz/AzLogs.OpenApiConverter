using System.Text.Json;
using System.Text.Json.Nodes;

var name = args[0];
var stream = File.OpenRead(name);

var node = await JsonNode.ParseAsync(stream!);

// Collect data flows for later inclusion as schema descriptions

var transformKqls = new Dictionary<string, string>();

var dataFlows = node!["properties"]!["dataFlows"];

foreach (var dataFlow in dataFlows!.AsArray())
{
    var transformKql = dataFlow!["transformKql"].AsValue().ToString();
    var streams = dataFlow!["streams"].AsArray();

    foreach (var oneStream in streams!)
    {
        var streamName = oneStream.AsValue().ToString();

        transformKqls[streamName] = transformKql;
    }
}

var streamDeclarations = node!["properties"]!["streamDeclarations"];

Console.WriteLine("openapi: 3.1.0");
Console.WriteLine("info:");
Console.WriteLine("  title: 'Generated API'");
Console.WriteLine("  version: 0.0.1");
Console.WriteLine("components:");
Console.WriteLine("  schemas:");

foreach (var declaration in streamDeclarations!.AsObject())
{
    Console.WriteLine("    {0}:", declaration.Key);
    Console.WriteLine("      type: object");
    Console.WriteLine("      additionalProperties: false");

    if (transformKqls.TryGetValue(declaration.Key, out var kqls))
    {
        Console.WriteLine("      description: {0}", kqls);        
    }

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
