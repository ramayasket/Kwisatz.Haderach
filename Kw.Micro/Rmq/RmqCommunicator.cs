using System.Collections.Generic;
using Kw.Micro.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Kw.Micro.Communications;
using Microsoft.Extensions.Logging;

namespace Kw.Micro.Rmq
{
    public class RmqCommunicator : ParallelCycle, IQueuedCommunicator
    {
        const int RECONNECT_INTERVAL = 5 * SECOND;

        public readonly string Host;
        public readonly string Username;
        public readonly string Password;
        public readonly int Port;
        public readonly string Protocol;
        public readonly Uri Uri;

        readonly ILogger _logger;
        readonly List<RmqReceiver> _receivers = new();

        readonly object _this = new();

        RmqConnection? _connection;

        public RmqCommunicator(RmqConfig config)
            : this(config.Protocol, config.Host, config.Port, config.Username, config.Password) { }

        public RmqCommunicator(string protocol, string host, int port, string user, string password) : base(false, true, RECONNECT_INTERVAL)
        {
            Protocol = protocol;
            Host = host;
            Port = port;
            Username = user;
            Password = password;
            Uri = new($"{Protocol}://{Host}:{Port}");

            _logger = CreateLogger<RmqCommunicator>()!;
        }

        public override bool? Iteration()
        {
            RmqConnection? old = _connection;

            if (null == _connection)
                _connection = CreateConnection();

            if (null != _connection &&
                !Write(_connection.Underlying, new AmqFanoutNode(), null, new AmqFanoutMessage()))
            {
                _connection = null;
                _logger.Write(LL.I, $"Disconnected from Rmq cluster {Uri}");
            }

            if (null == old && null != _connection)
                lock (_this)
                    foreach (RmqReceiver receiver in _receivers)
                        receiver.ChannelAndRead(_connection.Underlying);

            return null;
        }

        RmqConnection? CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = Host,
                UserName = Username,
                Password = Password,
                Port = Port,
                Uri = Uri,
            };

            IConnection? connection;

            try
            {
                connection = factory.CreateConnection();
                _logger.Write(LL.I, $"Connected to Rmq cluster {Uri}");
            }
            catch
            {
                connection = null;
            }

            if (null == connection)
                return null;

            return new(connection);
        }

        public void Read(CommunicatorNode node, string? address, Type messageType, Func<CommunicatorMessage, Task<bool>> receiveAction)
        {
            RmqReceiver receiver = new(node, address, messageType, receiveAction);

            RmqConnection? connection = _connection;

            if (null != connection)
                receiver.ChannelAndRead(connection.Underlying);

            lock (_this)
                _receivers.Add(receiver);
        }

        public bool Write(CommunicatorNode node, string? address, CommunicatorMessage message)
        {
            RmqConnection? connection = _connection;

            if (null != connection)
            {
                //
                // no logging here!
                //
                return Write(connection.Underlying, node, address, message);
            }

            return false;
        }

        bool Write(IConnection connection, CommunicatorNode node, string? address, CommunicatorMessage message)
        {
            if (node.Solicited && null == address)
                throw new InvalidOperationException("Need to provide an address for a solicited exchange");

            bool failure = false;

            try
            {
                using (var channel = connection.CreateModel())
                {
                    string exchangeType = node.Solicited ? ExchangeType.Direct : ExchangeType.Fanout;

                    channel.ConfirmSelect();
                    channel.ExchangeDeclare(node.Name, exchangeType, durable: true);

                    if (node.Solicited && !TestQueue(address!, channel))
                        throw new KeyNotFoundException("Target queue does not exist.");

                    string json = JsonSerializer.Serialize(message, message.GetType(), JsonOptions());
                    byte[] body = Encoding.UTF8.GetBytes(json);

                    string key = node.Solicited ? address! : "";

                    channel.BasicPublish(node.Name, key, basicProperties: null, body);

                    channel.WaitForConfirms();

                    return true;
                }
            }
            catch
            {
                failure = true;
            }

            return !failure;
        }

        public bool TestQueue(CommunicatorNode node, string address)
        {
            RmqConnection? connection = _connection;

            if (null == connection)
                return false;

            bool failure = false;

            try // check that target queue exists
            {
                using (var channel = connection.Underlying.CreateModel())
                {
                    channel.ConfirmSelect();
                    channel.ExchangeDeclare(node.Name, type: ExchangeType.Direct, durable: true);

                    bool test = TestQueue(address, channel);

                    return test;
                }
            }
            catch
            {
                failure = true;
            }

            return !failure;
        }

        bool TestQueue(string address, IModel channel)
        {
            try
            {
                channel.QueueDeclarePassive(address);
                return true;
            }
            catch (Exception x)
            {
                bool status404 = x.Message.Contains("=404");

                return !status404;
            }
        }
    }
}
