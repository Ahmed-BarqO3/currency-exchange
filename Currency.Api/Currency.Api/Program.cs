using Currencey.Api;
using Currencey.Api.Database;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.Services.AddOpenApi();
builder.Services.AddDataBase(config.GetConnectionString("NpgsDb")!);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var dbInitializer = app.Services.GetRequiredService<DBInitializer>();
await dbInitializer.InitializeAsync();

app.UseHttpsRedirection();


app.Run();
