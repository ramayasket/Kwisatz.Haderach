using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kw.Common;
using Kw.Micro.Logging;

namespace Kw.Micro
{
    public class Parallel
    {
        static readonly HashSet<Parallel> _parallels = new();

        static Parallel? GetFirst()
        {
            lock (_parallels)
                return _parallels.FirstOrDefault();
        }

        public static void WaitTerminate()
        {
            Parallel? next = GetFirst();

            while (null != next)
            {
                next._ended.WaitOne();
                next = GetFirst();
            }
        }

        protected Thread _thread;
        protected bool _needServer;

        static readonly AsyncLocal<string> _asyncName = new();
        static readonly ThreadLocal<string> _threadName = new();

        /// <summary>
        /// The name of currently executing Parallel class, if any.
        /// </summary>
        public static string? CurrentName => _threadName.Value ?? _asyncName.Value;

        protected virtual string Name { get; set; }

        protected readonly ManualResetEvent _started = new(false);
        protected readonly ManualResetEvent _ended = new(false);

        readonly Action _main;

        public static Parallel? Next()
        {
            lock (_parallels)
                return _parallels.FirstOrDefault();
        }

        public Parallel(bool needServer, bool start, Action main)
        {
            _main = main;
            _needServer = needServer;

            Name = main.Method.Name;

            if (start)
                Start();
        }

        protected Parallel(bool needServer, bool start)
        {
            _main = Main;
            _needServer = needServer;

            Name = GetType().AsString();

            if (start)
                Start();
        }

        public void Start()
        {
            if (null != _thread) return;

            (_thread = new(ThreadProc) { IsBackground = false }).Start();

            _started.WaitOne();
        }

        public virtual void OnError(Exception x) { }

        public virtual void OnEnd() { }

        void ThreadProc()
        {
            _threadName.Value = _asyncName.Value = Name;

            lock (_parallels)
                _parallels.Add(this);

            _started.Set();

            if (_needServer)
                Started.WaitOne();

            var logger = CreateLogger<Parallel>();

            logger.Write(LL.D, $"{Name} started");

            if (!Shutdown) // just in case
                try
                {
                    _main();
                }
                catch (Exception x)
                {
                    try { OnError(x); } catch { }
                    logger.Write(x, $"{Name} has crashed");
                }

            OnEnd();
            
            logger.Write(LL.D, $"{Name} ended");

            lock (_parallels)
                _parallels.Remove(this);

            _ended.Set();
        }

        public virtual void Main() { }

        public virtual Parallel Clone(bool start)
        {
            Parallel x = new(_needServer, start, _main);
            return x;
        }
    }

    public class ParallelCycle : Parallel
    {
        //
        // null = regular cycle with waiting
        // true = next cycle without waiting
        // false = quit
        //
        protected readonly Func<bool?> _cycle;

        protected readonly Func<bool> _signal;
        protected readonly Action _initialize;
        protected readonly uint _wait;

        public ParallelCycle(bool needServer, bool start, uint wait) : base(needServer, false)
        {
            _wait = wait;

            _initialize = Initialize;
            _signal = Signal;
            _cycle = Iteration;

            if (start)
                Start();
        }

        public ParallelCycle(bool needServer, bool start,
            Action initialize,
            Func<bool?> cycle,
            Func<bool> signal,
            uint wait) : base(needServer, false)
        {
            _wait = wait;

            _initialize = initialize;
            _signal = signal;
            _cycle = cycle;

            Name = _cycle.Method.Name;

            if (start)
                Start();
        }

        public override void Main()
        {
            _initialize();

            while (!Shutdown)
            {
                bool? control = null;

                try
                {
                    control = _cycle();
                }
                catch
                {
                    // ignored
                }

                if (control is false)
                    break;

                if (control is true)
                    continue;

                ServiceEnvironment.Interruptable.Wait(_wait, 10, _signal);
            }
        }

        public virtual void Initialize() { }
        public virtual bool? Iteration() => null;
        public virtual bool Signal() => false;
    }

    public class ParallelWorker<T, TWorker> : ParallelCycle where T : class where TWorker: class
    {
        static readonly Queue<T> _incoming = new();
        protected readonly Func<T,Task> _process;

        public ParallelWorker(bool start, uint wait) : base(true, start, wait)
        {
            _process = Process;
        }

        public ParallelWorker(bool start, Func<T,Task> process, uint wait) : base(true, start, wait)
        {
            _process = process;

            Name = process.Method.Name;
        }

        public static void Load(T load)
        {
            lock (_incoming)
                _incoming.Enqueue(load);
        }

        public override bool Signal()
        {
            lock(_incoming)
                return _incoming.Any();
        }

        public override bool? Iteration()
        {
            T? x = null;

            lock (_incoming)
                if (_incoming.Any())
                    x = _incoming.Dequeue();

            if (null == x) return null; // default = wait

            try
            {
                _process(x).Wait();
            }
            catch
            {
            }

            return true; // immediate repeat
        }

        public virtual Task Process(T x) => Task.CompletedTask;
    }
}
