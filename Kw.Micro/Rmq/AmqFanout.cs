using Kw.Micro.Communications;

namespace Kw.Micro.Rmq
{
    internal class AmqFanoutNode : CommunicatorNode
    {
        public override string Name => "amq.fanout";
        public override bool Solicited => false;
    }

    internal class AmqFanoutMessage : CommunicatorMessage { }
}
