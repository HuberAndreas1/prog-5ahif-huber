using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sqlite = builder.AddSqlite(
        "sqlite-db",
        builder.Configuration["Database:path"],
        builder.Configuration["Database:fileName"])
    .WithSqliteWeb(); // works just with docker

var webapi = builder
    .AddProject<WebApi>("webapi")
    .WithReference(sqlite);

builder.AddNpmApp("frontend", "../frontend")
    .WithReference(webapi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
