using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddSqlServer("MyDataBase");
var broker = builder.AddRabbitMQ("messagebroker");

var gateway = builder
    .AddProject<Gateway>(nameof(Gateway))
    .WithEnvironment("ServiceMetadata__ServiceName", nameof(Gateway))
    .WithReference(db)
    .WithReference(broker);

await builder.Build().RunAsync();