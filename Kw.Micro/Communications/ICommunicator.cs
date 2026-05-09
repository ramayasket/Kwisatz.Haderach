using System;
using System.Threading.Tasks;

namespace Kw.Micro.Communications
{
    public interface ICommunicator
    {
        void Read(CommunicatorNode node, string? address, Type messageType, Func<CommunicatorMessage, Task<bool>> handler);
        bool Write(CommunicatorNode node, string? address, CommunicatorMessage message);
    }

    public interface IQueuedCommunicator : ICommunicator
    {
        bool TestQueue(CommunicatorNode node, string address);
    }
}