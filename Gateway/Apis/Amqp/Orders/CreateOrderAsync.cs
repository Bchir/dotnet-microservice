using Gateway.Apis.Amqp.RegistrationExtensions;

namespace Gateway.Apis.Amqp.Orders;

public class CreateOrderAsync
{
    public Guid OrderId { get; set; }
}

public class CreateOrderHandler : IConsumer<CreateOrderAsync>
{
    public Task RunAsync(CreateOrderAsync messgage, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
