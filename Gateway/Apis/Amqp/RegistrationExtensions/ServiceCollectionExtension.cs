using Gateway.Apis.Amqp.Tooling;

namespace Gateway.Apis.Amqp.RegistrationExtensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddQueueConsumer<TMessage, THandler>(
                this IServiceCollection services,
                VersionState versionState
            ) where THandler: class, IConsumer<TMessage>
        {
            services.AddScoped<IConsumer<TMessage>, THandler>();
            AsyncApiConsumerFactoryBag.AddQueueConsumer<TMessage>(versionState);
            return services;
        }
    }
}
