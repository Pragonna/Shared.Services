using System.Net.Sockets;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EventBus.RabbitMQ;

public class RabbitMQPersistenceConnection(IConnectionFactory connectionFactory, int retryCount) : IDisposable
{
    private IConnection connection;
    public bool IsConnected => connection != null && connection.IsOpen;
    private bool isDisposed;
    private object lock_object = new object();
    public async Task<IChannel> CreateChannel() => await connection.CreateChannelAsync();

    public bool TryConnect()
    {
        lock (lock_object)
        {
            var policy = Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    //
                });

            policy.Execute(() => connection = connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult());

            if (IsConnected)
            {
                connection.ConnectionShutdownAsync += (sender, e) => ReTryWhenDisconnected();
                connection.ConnectionBlockedAsync += (sender, e) => ReTryWhenDisconnected();
                connection.CallbackExceptionAsync += (sender, e) => ReTryWhenDisconnected();
                
                return true;
            }
            return false;
        }
    }

    private async Task ReTryWhenDisconnected()
    {
        if (isDisposed) return;
        TryConnect();
    }

    public void Dispose()
    {
        isDisposed = true;
        connection.Dispose();
    }
}