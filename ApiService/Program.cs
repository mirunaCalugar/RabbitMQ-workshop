using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Adăugăm suport pentru controllere
builder.Services.AddControllers();

// Adăugăm OpenAPI (disponibil în .NET 8+)
// builder.Services.AddOpenApi();

var app = builder.Build();

// Activăm documentația OpenAPI
// app.MapOpenApi();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "ApiService is running!");


app.Run();
