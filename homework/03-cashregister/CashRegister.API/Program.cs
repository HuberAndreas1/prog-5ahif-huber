using CashRegister.API;
using CashRegister.Data;

var builder = WebApplication.CreateBuilder(args);

// Add logging from .NET Aspire
builder.AddServiceDefaults();

builder.AddSqliteDbContext<ApplicationDataContext>("sqlite-db");
builder.Services.AddCors();

var app = builder.Build();

app.MapCashRegisterApi();
app.UseCors(policyBuilder => policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseHttpsRedirection();

app.Run();