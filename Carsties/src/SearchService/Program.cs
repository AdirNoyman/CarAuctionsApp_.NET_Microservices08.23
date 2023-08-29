using System.Net;
using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Models;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Register services
builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetRetryPolicy());
// Register the MassTransit service (for RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    // Connect MassTransit to RabbitMQ over localhost
    x.UsingRabbitMq((context, config) =>
    {
        config.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseAuthorization();

app.MapControllers();

// Start and run the Search app even if the Auction service is not available at that time (of starting).
app.Lifetime.ApplicationStarted.Register(async () =>
{
    // Initialize the MongoDB database
    try
    {

        await DbInitilaizer.InitDb(app);

    }
    catch (Exception e)
    {
        Console.WriteLine("Error initilazing the mongoDB database 😩", e);
    }

});

app.Run();

// Handle http request retry to get the data from the Auction service
// Keep trying every 3 seconds until we get an ok response
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() => HttpPolicyExtensions.HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
