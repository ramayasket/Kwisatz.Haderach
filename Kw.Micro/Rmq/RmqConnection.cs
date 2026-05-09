using RabbitMQ.Client;

namespace Kw.Micro.Rmq
{
    public class RmqConnection
    {
        public RmqConnection(IConnection connection)
        {
            Underlying = connection;
        }

        public IConnection Underlying { get; }

        ~RmqConnection()
        {
            Underlying.Dispose();
        }
    }
}
