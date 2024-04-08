using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddSqlServer("MyDataBase");
// var broker = builder.AddRabbitMQ("messagebroker");

var gateway = builder.AddProject<Gateway>(nameof(Gateway))
    .WithReference(db);

await builder.Build().RunAsync();