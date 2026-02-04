using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Shared.Services.BuildingBlocks.EventBus.Base;
using Shared.Services.BuildingBlocks.EventBus.Base.Events;
using Shared.Services.BuildingBlocks.Exceptions;

namespace EventBus.RabbitMQ;

    public class EventBusRabbitMQ : BaseEventBus
    {
        readonly RabbitMQPersistenceConnection rabbitMQPersistenceConnection;
        readonly IConnectionFactory connectionFactory;
        readonly IChannel channel;
        readonly Uri Uri;

        public EventBusConfig Config { get; }

        public EventBusRabbitMQ(IServiceProvider serviceProvider, EventBusConfig config) : base(serviceProvider, config)
        {
            Uri = new Uri(config.EventBusConnectionString);
           
            connectionFactory = new ConnectionFactory()
            {
                Uri = Uri
            };

            rabbitMQPersistenceConnection = new RabbitMQPersistenceConnection(connectionFactory, config.ConnectionRetryCount);
            channel = CreateConsumerChannel().GetAwaiter().GetResult();
            SubscriptionManager.OnEventRemoved += SubscriptionManager_OnEventRemoved;
            Config = config;
        }

        private async void SubscriptionManager_OnEventRemoved(object? sender, string eventName)
        {
            var _eventName = ProcessEventName(eventName);

            if (!rabbitMQPersistenceConnection.IsConnected)
            {
                rabbitMQPersistenceConnection.TryConnect();
            }

            await channel.QueueUnbindAsync(queue: eventName,
                                         exchange: EventBusConfig.DefaultTopicName,
                                         routingKey: eventName);

            if (SubscriptionManager.IsEmpty)
            {
                await channel.CloseAsync();
            }
        }

        public override async Task Publish(IntegrationEvent @event)
        {
            if (!rabbitMQPersistenceConnection.IsConnected)
                rabbitMQPersistenceConnection.TryConnect();

            var policy = Policy.Handle<BrokerUnreachableException>()
                             .Or<SocketException>()
                             .WaitAndRetry(EventBusConfig.ConnectionRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                             {
                                 // retry callback intentionally left empty – logging delegated to host application
                             });

            var eventName = @event.GetType().Name;
            eventName = ProcessEventName(eventName);

            await channel.ExchangeDeclareAsync(exchange: EventBusConfig.DefaultTopicName,
                                            type: ExchangeType.Direct);

            var message = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            var body = Encoding.UTF8.GetBytes(message);

            try
            {
                await policy.Execute(async () =>
                {
                    BasicProperties properties = new BasicProperties
                    {
                        DeliveryMode = DeliveryModes.Persistent
                    };

                    await channel.BasicPublishAsync(
                        exchange: Config.DefaultTopicName,
                        routingKey: eventName,
                        mandatory: true,
                        body: body);
                });
            }
            catch (Exception ex)
            {
                throw new EventBusConnectionException(
                    $"Failed to publish event '{eventName}' after {Config.ConnectionRetryCount} retries.",
                    ex
                );
            }
        }

        public override async Task Subscribe<T, THandle>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);

            if (!SubscriptionManager.HasSubscriptionForEvent(eventName))
            {
                if (!rabbitMQPersistenceConnection.IsConnected)
                {
                    rabbitMQPersistenceConnection.TryConnect();
                }

                await channel.QueueDeclareAsync(queue: GetSubName(eventName),
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false);

                await channel.QueueBindAsync(queue: GetSubName(eventName),
                                   exchange: Config.DefaultTopicName,
                                   routingKey: eventName);

            }
            SubscriptionManager.AddSubscription<T, THandle>();
            await StartBasicConsume(eventName);
        }

        public override async Task UnSubscribe<T, THandle>()
        {
            SubscriptionManager.RemoveSubscription<T, THandle>();
        }
        private async Task<IChannel> CreateConsumerChannel()
        {
            if (!rabbitMQPersistenceConnection.IsConnected)
            {
                rabbitMQPersistenceConnection.TryConnect();
            }

            var channel = await rabbitMQPersistenceConnection.CreateChannel();
            await channel.ExchangeDeclareAsync(
                exchange: EventBusConfig.DefaultTopicName,
                type: ExchangeType.Direct);

            return channel;
        }
        private async Task StartBasicConsume(string eventName)
        {
            if (channel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(channel);

                await channel.BasicConsumeAsync(queue: GetSubName(eventName),
                                     autoAck: false,
                                     consumer: consumer);

                consumer.ReceivedAsync += async (sender, e) =>
                {
                    var _eventName = e.RoutingKey;
                    _eventName = ProcessEventName(_eventName);
                    var message = Encoding.UTF8.GetString(e.Body.Span);

                    try
                    {
                        await ProcessEvent(_eventName, message);
                        await channel.BasicAckAsync(e.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        throw new EventBusPublishException(
                            $"Failed to publish event '{eventName}'.",
                            ex
                        );
                    }
                };
            }
        }
    }