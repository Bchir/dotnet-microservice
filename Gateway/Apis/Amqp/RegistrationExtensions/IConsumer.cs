namespace Gateway.Apis.Amqp.RegistrationExtensions;

public interface IConsumer<T>
{
    Task RunAsync(T messgage, CancellationToken cancellationToken);
}
