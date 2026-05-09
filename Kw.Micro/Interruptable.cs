using System.Threading;

namespace Kw.Micro
{
    public enum Waitstate
    {
        Timeout,   // time ended
        Shutdown,  // wait interrupted due to a shutdown
        Completed, // waitable event occurred
    }


    public class Interruptable
    {
        public Waitstate Wait(long period, int sleep, Func<bool>? signal = null)
        {
            for (int i = 0; i < period / sleep; i++)
            {
                if(Shutdown)
                    return Waitstate.Shutdown;
                
                if (null != signal && signal())
                    return Waitstate.Completed;

                Thread.Sleep(sleep);
            }

            return Waitstate.Timeout;
        }
    }
}