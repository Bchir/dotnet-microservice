namespace Gateway.Apis.Amqp.Tooling;

public interface IConsumer<T>
{
    Task RunAsync(T messgage, CancellationToken cancellationToken);
}
