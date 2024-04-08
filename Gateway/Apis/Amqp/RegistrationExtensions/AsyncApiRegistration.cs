using RabbitMQ.Client;

namespace Gateway.Apis.Amqp.Tooling;

public static class AsyncApiRegistration
{
    public static WebApplicationBuilder AddAsyncApiHost(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("messagebroker");
        var rabbitMqUri = new Uri(connectionString!);
        var rabbitMqFactory = new ConnectionFactory { Uri = rabbitMqUri };
        builder.Services.AddHostedService<ConsumerHost>();
        builder.Services.AddSingleton<IConnectionFactory>(rabbitMqFactory);
        builder.Services.AddHealthChecks().AddRabbitMQ(rabbitMqUri, tags: []);
        return builder;
    }

}
