using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data
{
    public class DbInitilaizer
    {
        public static async Task InitDb(WebApplication app)
        {
            // Initialize the MongoDB 
            await DB.InitAsync("SearchDB", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            // Create the indexes for the search functionality
            await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

            // Check if there are already items in the database
            var count = await DB.CountAsync<Item>();

            using var scope = app.Services.CreateScope();

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();

            var items = await httpClient.GetItemsForSearchDb();

            Console.WriteLine($"{items.Count} returned from the auction service 🤘😎");

            if (items.Count > 0) await DB.SaveAsync(items);

        }
    }
}