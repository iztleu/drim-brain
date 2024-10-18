using ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImageTag("16.4")
    .WithPgAdmin();

var webApiDb = postgres.AddDatabase(ResourceNames.WebApiDb);

var webApiSetup = builder.AddProject<Projects.WebApi_Setup>(ResourceNames.WebApiSetup)
    .WithReference(webApiDb);

var webApi = builder.AddProject<Projects.WebApi>(ResourceNames.WebApi)
    .WithReference(webApiDb);

builder.Build().Run();
