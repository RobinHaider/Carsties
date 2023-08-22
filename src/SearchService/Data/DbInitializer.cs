using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;
using System.Text.Json;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitializeAsync(WebApplication app)
    {
        await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        // seed data

        // approch 1 - using json file

        //var count = await DB.CountAsync<Item>();
        //if(count == 0)
        //{
        //    Console.WriteLine("No Data available Seeding data...");

        //    var itemData = await File.ReadAllTextAsync("Data/auctions.json");
        //    var options = new JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true,
        //    };
        //    var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);

        //    await DB.SaveAsync(items);
        //}

        // approch 2 - using http client
        using var scope = app.Services.CreateScope();

        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();

        var items = await httpClient.GetAuctionsAsync();

        Console.WriteLine($"{items.Count} returned from the auction service");

        if (items.Count > 0) await DB.SaveAsync(items);
    }
}
