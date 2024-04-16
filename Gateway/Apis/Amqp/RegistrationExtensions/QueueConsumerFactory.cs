using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Gateway.Apis.Amqp.RegistrationExtensions
{
    public static class QueueConsumerFactory
    {
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
        private static readonly ActivitySource ActivitySource = new(nameof(QueueConsumerFactory));

        public static void CreateQueueConsumer<TMessage>(
            IServiceProvider service,
            IModel channel,
            string queueName
        )
        {
            channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                using (Activity? activity = CreateActivity(queueName, ea))
                {
                    using var serviceScope = service.CreateAsyncScope();
                    var jsonOptions = serviceScope
                        .ServiceProvider
                        .GetRequiredService<IOptions<JsonOptions>>();
                    var messageHandler = serviceScope
                        .ServiceProvider
                        .GetRequiredService<IConsumer<TMessage>>();
                    var body = ea.Body.ToArray();
                    var message = JsonSerializer.Deserialize<TMessage>(
                        ea.Body.ToArray(),
                        jsonOptions.Value.SerializerOptions
                    );
                    await messageHandler.RunAsync(message!, CancellationToken.None);
                }
            };

            channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
        }

        private static Activity? CreateActivity(string queueName, BasicDeliverEventArgs ea)
        {
            var parentContext = Propagator.Extract(
                default,
                ea.BasicProperties,
                ExtractTraceContextFromBasicProperties
            );
            Baggage.Current = parentContext.Baggage;
            var activityName = $"{ea.RoutingKey} receive";
            var activity = ActivitySource.StartActivity(
                activityName,
                ActivityKind.Consumer,
                parentContext.ActivityContext
            );
            AddMessagingTags(activity, queueName, ea.Exchange);
            return activity;
        }

        private static IEnumerable<string> ExtractTraceContextFromBasicProperties(
            IBasicProperties props,
            string key
        )
        {
            if (props.Headers.TryGetValue(key, out var value))
            {
                var bytes = value as byte[];
                return [Encoding.UTF8.GetString(bytes!)];
            }
            return [];
        }

        private static void AddMessagingTags(
            Activity? activity,
            string queueName,
            string exchangeName
        )
        {
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.destination", queueName);
            activity?.SetTag("messaging.rabbitmq.routing_key", exchangeName);
        }
    }
}