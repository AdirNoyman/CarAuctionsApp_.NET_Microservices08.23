using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{

    public class AuctionServiceHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

        }

        public async Task<List<Item>> GetItemsForSearchDb()
        {
            // Get the most recent update date of an item in the database
            var lastUpdated = await DB.Find<Item, string>().Sort(x => x.Descending(x => x.UpdatedAt)).Project(x => x.UpdatedAt.ToString()).ExecuteFirstAsync();

            // Send a HTTP get request to the AuctionService, to get all auctions that are created or updated till the last update date (inclusive)     
            return await _httpClient.GetFromJsonAsync<List<Item>>($"{_configuration["AuctionServiceUrl"]}/api/auctions?date={lastUpdated}");
        }
    }
}