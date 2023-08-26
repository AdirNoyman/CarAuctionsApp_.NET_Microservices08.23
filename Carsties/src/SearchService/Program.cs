using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Data;
using SearchService.Models;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Register services
builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseAuthorization();

app.MapControllers();

// Initialize the MongoDB database
try
{

    await DbInitilaizer.InitDb(app);

}
catch (Exception e)
{
    Console.WriteLine("Error initilazing the mongoDB database 😩", e);
}

app.Run();
