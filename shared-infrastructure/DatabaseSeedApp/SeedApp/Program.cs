// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

if(args.Length < 6)
{
    Console.WriteLine("Necessary arguments are missing.");
    return;
}

bool resetData = false;
string endpoint = args[0];
string accountKey = args[1];
string databaseName = args[2];
string containerName = args[3];
string partitionKeyField = args[4];
string filePath = args[5];

if(args.Length > 6)
{
    _ = bool.TryParse(args[6], out resetData);
}

CosmosClient client = new(endpoint, accountKey);
Container container = client.GetContainer(databaseName, containerName);

if (resetData)
{
    Console.WriteLine("Data reset flag was set. Dropping existing data...");
    await DeleteAllItemsAsync(container, partitionKeyField);
    Console.WriteLine("Data reset complete.");
}
else
{
    Console.WriteLine("Data reset flag was not set. Existing data will be left intact.");
}

string jsonContent = await File.ReadAllTextAsync(filePath);
var items = JsonConvert.DeserializeObject<dynamic[]>(jsonContent);

if(items == null)
{
    Console.WriteLine("Unable to deserialise seed data");
    Console.WriteLine(jsonContent);
    return;
}

Console.WriteLine($"Seeding data from file: {filePath}...");

foreach (var item in items)
{
    await container.UpsertItemAsync(item, new PartitionKey((string)item[partitionKeyField]));
}

Console.WriteLine("Data seeding completed.");

static async Task DeleteAllItemsAsync(Container container, string partitionKeyField)
{
    Console.WriteLine("Clearing existing data from target container...");

    var query = "SELECT * FROM c";
    var iterator = container.GetItemQueryIterator<dynamic>(query);
    while (iterator.HasMoreResults)
    {
        foreach (var item in await iterator.ReadNextAsync())
        {
            await container.DeleteItemAsync<dynamic>(
                (string)item.id,
                new PartitionKey((string)item[partitionKeyField])
            );
        }
    }

    Console.WriteLine("Target container cleared.");
}

[ExcludeFromCodeCoverage]
public partial class Program { }