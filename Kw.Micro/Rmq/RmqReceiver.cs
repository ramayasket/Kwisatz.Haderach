using Kw.Micro.Logging;
using Kw.Micro.Communications;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kw.Micro.Rmq
{
    /// <summary>
    /// Creates a RabbitMQ queue and listens to it.
    /// </summary>
    internal class RmqReceiver
    {
        readonly Type _messageType;
        readonly Func<CommunicatorMessage, Task<bool>> _handler;
        readonly ILogger _logger;
        readonly CommunicatorNode _node;
        readonly string _address;

        IModel? _channel;

        public RmqReceiver(CommunicatorNode node, string? address, Type messageType, Func<CommunicatorMessage, Task<bool>> handler)
        {
            _node = node;
            _address = address ?? $"{_node.Name}.{InstanceId}";

            _logger = CreateLogger<RmqReceiver>()!;
            _logger.Write(LL.I, $"RMQ reading from address '{_address}', type: '{messageType.Name}'");

            _messageType = messageType;
            _handler = handler;
        }

        internal void ChannelAndRead(IConnection connection)
        {
            _channel?.Dispose(); // if there is an old one

            IModel? channel;

            try
            {
                channel = connection.CreateModel();
            }
            catch
            {
                return;
            }

            string exchangeType = _node.Solicited ? ExchangeType.Direct : ExchangeType.Fanout;

            _channel = channel;
            _channel.ExchangeDeclare(_node.Name, exchangeType, durable: true);

            string queue = _channel.QueueDeclare(_address, exclusive: false).QueueName;
            string key = _node.Solicited ? _address : "";

            _channel.QueueBind(queue, _node.Name, key);

            EventingBasicConsumer consumer = new(_channel);
            consumer.Received += Receive;

            _channel.BasicConsume(queue, autoAck: true, consumer);
        }

        /// <remarks> NO LOGGING HERE! </remarks>
        async void Receive(object? _, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            var s = Encoding.UTF8.GetString(body);

            var message = (CommunicatorMessage)JsonSerializer.Deserialize(s, _messageType)!;

            await _handler(message);
        }
    }
}
