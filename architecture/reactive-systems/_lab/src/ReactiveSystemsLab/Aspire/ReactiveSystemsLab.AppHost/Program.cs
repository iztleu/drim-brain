var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddKafka("kafka");

var rabbitMq = builder.AddRabbitMQ("rabbitmq");

var blockchainServiceDb = builder.AddPostgres("blockchain-service-db")
    .AddDatabase("BlockchainServiceDb");

var blockchainServiceSetup = builder.AddProject<Projects.BlockchainService_Setup>("blockchain-service-setup")
    .WithReference(kafka)
    .WithReference(blockchainServiceDb)
    .WithReplicas(1);

var blockchainService = builder.AddProject<Projects.BlockchainService>("blockchain-service")
    .WithReference(blockchainServiceDb)
    .WithReference(kafka)
    .WithReplicas(2);

var bankingServiceDb = builder.AddPostgres("banking-service-db")
    .AddDatabase("BankingServiceDb");

var bankingServiceSetup = builder.AddProject<Projects.BankingService_Setup>("banking-service-setup")
    .WithReference(bankingServiceDb)
    .WithReplicas(1);

var bankingService = builder.AddProject<Projects.BankingService>("banking-service")
    .WithReference(bankingServiceDb)
    .WithReference(kafka)
    .WithReference(rabbitMq)
    .WithReplicas(2);

var notificationServiceDb = builder.AddPostgres("notification-service-db")
    .AddDatabase("NotificationServiceDb");

var notificationServiceSetup = builder.AddProject<Projects.NotificationService_Setup>("notification-service-setup")
    .WithReference(notificationServiceDb)
    .WithReplicas(1);

var notificationService = builder.AddProject<Projects.NotificationService>("notification-service")
    .WithReference(notificationServiceDb)
    .WithReference(rabbitMq)
    .AsHttp2Service()
    .WithReplicas(1);

var apiGateway = builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithReference(blockchainService)
    .WithReference(bankingService)
    .WithReference(notificationService)
    .WithReplicas(2);

builder.Build().Run();
