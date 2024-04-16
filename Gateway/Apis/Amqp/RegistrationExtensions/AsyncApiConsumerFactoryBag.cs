using RabbitMQ.Client;
using Saunter.Utils;


namespace Gateway.Apis.Amqp.RegistrationExtensions;

public static class AsyncApiConsumerFactoryBag
{

    private static readonly Queue<Action<IModel, IServiceProvider>> DocumentationActions = new();


    public static void AddQueueConsumer<TMessage>(
        VersionState versionState)
    {
        string queueName = $"{versionState.Version:'v'VVV}/{typeof(TMessage).Name}";
        DocumentationActions.Enqueue(
            (model, sp) =>
            {
                QueueConsumerFactory
                                .CreateQueueConsumer<TMessage>(
                                    sp,
                                    model,
                                    queueName);
            });
        AsyncApiDocumentationGenerationBag.AddDocumentation(x => {
            x.AsyncApi.Channels.AddOrAppend(
                queueName,
                new Saunter.AsyncApiSchema.v2.ChannelItem()
                {
                    Subscribe = new Saunter.AsyncApiSchema.v2.Operation()
                    {
                        
                    }
                }
                );
            });
    }

    public static void CreateConsumers(IModel model, IServiceProvider serviceProvider)
    {
        while (DocumentationActions.Count > 0)
        {
            var consumerFactory = DocumentationActions.Dequeue();
            consumerFactory(model, serviceProvider);
        }
    }

}
