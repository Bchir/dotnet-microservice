using Gateway.Apis.Amqp.Orders;
using RabbitMQ.Client;
using Saunter;

namespace Gateway.Apis.Amqp.RegistrationExtensions;

public static class AsyncApiRegistration
{
    public static readonly VersionState VersionOne = new(new(1.0), true);

    public static WebApplicationBuilder AddAsyncApiHost(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("messagebroker");
        var rabbitMqUri = new Uri(connectionString!);
        var rabbitMqFactory = new ConnectionFactory { Uri = rabbitMqUri };

        builder.Services.AddQueueConsumer<CreateOrderAsync, CreateOrderHandler>(VersionOne);
        builder.Services.AddHostedService<ConsumerHost>();
        builder.Services.AddSingleton<IConnectionFactory>(rabbitMqFactory);
        builder.Services.AddHealthChecks().AddRabbitMQ(rabbitMqUri, tags: []);

        builder.Services.AddAsyncApiSchemaGeneration(options =>
        {
            AsyncApiDocumentationGenerationBag.Apply(options);
        });

        return builder;
    }
}
