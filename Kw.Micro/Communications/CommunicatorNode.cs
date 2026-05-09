namespace Kw.Micro
{
    /// <summary>
    /// Node is a unit which supports string-based addressing within it.
    /// </summary>
    public abstract class CommunicatorNode
    {
        public virtual bool Solicited => true;
        public abstract string Name { get; }
    }
}