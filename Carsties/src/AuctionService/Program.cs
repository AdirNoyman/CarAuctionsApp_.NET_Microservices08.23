using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Add DB service to the container
builder.Services.AddDbContext<AuctionDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Register the ampping we created for the DTOs to the container
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Register the MassTransit service (for RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    // Handle a situation where the MessageBroker(RabbitMQ Service bus) is down, by trying to check every 10 seconds if the there is an event still waiting in the outbox queue to be sent to the broker. If yes, then try to send it again.
    x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromMilliseconds(10);
        o.UsePostgres();
        o.UseBusOutbox();

    });

    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false)); //

    // Connect MassTransit to RabbitMQ over localhost
    x.UsingRabbitMq((context, config) =>
    {
        config.ConfigureEndpoints(context);
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline. Here we will add Middleware (for inbound or outbound requests)

app.UseAuthorization();
// Map the routes to the controllers they belong to
app.MapControllers();

// Seed the database (if needed)
try
{

    Dbinitializer.InitDb(app);

}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}

app.Run();
