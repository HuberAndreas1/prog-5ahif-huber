using DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add logging from .NET Aspire
builder.AddServiceDefaults();

builder.AddSqliteDbContext<ApplicationDataContext>("sqlite-db");

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", async (ApplicationDataContext db) 
    => await db.Customers.ToListAsync()
    );

app.MapGet("/exception", () => {
    throw new Exception("Test exception");
});

app.Run();