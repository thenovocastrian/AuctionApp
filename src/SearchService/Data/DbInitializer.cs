using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        // Initialize the Mongo database.
        await DB.InitAsync("SearchDb", MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        // Create a database index
        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();
        
        var count = await DB.CountAsync<Item>();

        if (count == 0)
        {
            Console.WriteLine("There is no data. Attempting to seed...");
            var itemData = await File.ReadAllTextAsync("Data/auctions.json");
            
            var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
            
            var items = JsonSerializer.Deserialize<List<Item>>(itemData,options);
            
            await DB.SaveAsync(items);
        }

    }
}