using System.Threading.Tasks;
using Kw.Micro.Communications;

namespace Kw.Micro
{
    public class DefaultCommunicator : IQueuedCommunicator
    {
        public void Read(CommunicatorNode node, string? address, Type messageType, Func<CommunicatorMessage, Task<bool>> handler) { }
        public bool Write(CommunicatorNode node, string? address, CommunicatorMessage message) { return true; }
        public bool TestQueue(CommunicatorNode node, string address) => true;
    }
}
