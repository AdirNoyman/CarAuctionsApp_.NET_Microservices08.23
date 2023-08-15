using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Add DB service to the container
builder.Services.AddDbContext<AuctionDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Register the ampping we created for the DTOs to the container
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
var app = builder.Build();

// Configure the HTTP request pipeline. Here we will add Middleware (for inbound or outbound requests)

app.UseAuthorization();
// Map the routes to the controllers they belong to
app.MapControllers();

// Seed the database (if needed)
try {

    Dbinitializer.InitDb(app);

} catch (Exception e) {
    Console.WriteLine(e);
    throw;
}

app.Run();
