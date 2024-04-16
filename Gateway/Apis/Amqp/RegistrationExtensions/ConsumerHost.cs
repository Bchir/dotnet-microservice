using RabbitMQ.Client;

namespace Gateway.Apis.Amqp.RegistrationExtensions;

public class ConsumerHost(IConnectionFactory connectionFactory, IServiceProvider serviceProvider)
    : IHostedService
{
    private readonly IConnectionFactory _connectionFactory = connectionFactory;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        AsyncApiConsumerFactoryBag.CreateConsumers(_channel, _serviceProvider);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _connection?.Dispose();
        _channel?.Dispose();
        return Task.CompletedTask;
    }
}