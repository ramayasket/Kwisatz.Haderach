using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kw.Common.Threading
{
    /// <summary>
    /// Пул потоков для параллельного выполнения задач.
    /// </summary>
    public class ExecutionThreads
    {
        readonly Queue<ExecutionThread> _queuedTasks = new Queue<ExecutionThread>();
        readonly HashSet<ExecutionThread> _activeTasks = new HashSet<ExecutionThread>();
        readonly ManualResetEvent _empty = new ManualResetEvent(true);

        readonly List<Exception> _errors = new List<Exception>();

        /// <summary>
        /// Максимальное количество потоков.
        /// </summary>
        public int Capacity { get; private set; }
        
        /// <summary>
        /// Коллекция исключений в функциях потоков.
        /// </summary>
        public Exception[] Errors => _errors.ToArray();

        /// <summary>
        /// Рекомендуемое количество потоков.
        /// </summary>
        public static int AdviseCapacity => Environment.ProcessorCount;

        public int QueueLength => _queuedTasks.Count;

        public int ActiveTasks => _activeTasks.Count;

        public int TotalLoad => _activeTasks.Count + _queuedTasks.Count;

        /// <summary>
        /// Иницилизирует объект пула.
        /// </summary>
        /// <param name="capacity">Количество потоков.</param>
        public ExecutionThreads(int capacity = 0)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity", "Expected positive number or zero.");

            Capacity = (0 == capacity) ? AdviseCapacity : capacity;
        }

        public int Cancel()
        {
            _queuedTasks.Clear();
            var actives = _activeTasks.ToArray();

            foreach(var task in actives)
            {
                task.Thread.Abort();
            }

            return actives.Length;
        }

        public event Action<bool> Empty;

        /// <summary>
        /// Ожидает завершения всех поставленных в очередь задач.
        /// </summary>
        /// <returns>Ошибки выполнения.</returns>
        public Exception[] Join(int? timeout = null)
        {
            if (null != timeout)
            {
                _empty.WaitOne(timeout.Value);
            }
            else
            {
                _empty.WaitOne();
            }

            return Errors.Where(e => null != e).ToArray();
        }

        /// <summary>
        /// Ставит в очередь задачу.
        /// </summary>
        /// <param name="action">Метод задачи.</param>
        /// <returns>Объект ExecutionThread.</returns>
        public ExecutionThread Enqueue(Action action)
        {
            var task = new ExecutionThread(action, this);

            Synchronous(p => p.InternalEnqueue(task));

            return task;
        }

        /// <summary>
        /// Ставит в очередь задачу с входными данными.
        /// </summary>
        /// <typeparam name="T">Тип входных данных задачи.</typeparam>
        /// <param name="action">Метод задачи.</param>
        /// <param name="parameter">Входные данные задачи.</param>
        /// <returns>Объект ExecutionThread.</returns>
        public ExecutionThread Enqueue<T>(Action<T> action, T parameter) where T : class
        {
            var task = new ExecutionThread<T>(action, this) { Parameter = parameter };

            Synchronous(p => p.InternalEnqueue(task));

            return task;
        }

        /// <summary>
        /// Ставит в очередь задачу с входными и выходными данными.
        /// </summary>
        /// <typeparam name="T">Тип входных данных задачи.</typeparam>
        /// <typeparam name="R">Тип выходных данных задачи.</typeparam>
        /// <param name="action">Метод задачи.</param>
        /// <param name="parameter">Входные данные задачи.</param>
        /// <returns>Объект ExecutionThread.</returns>
        public ExecutionThread<T,R> Enqueue<T, R>(Func<T, R> action, T parameter) where T : class
        {
            var task = new ExecutionThread<T, R>(action, this) { Parameter = parameter };

            Synchronous(p => p.InternalEnqueue(task));

            return task;
        }

        public void EnqueueProcess<T>(T[] data, Action<T[]> process, int minSize = 1) where T : class
        {
            InternalEnqueueProcess(data, process, minSize);
        }

        public void EnqueueProcess<T>(T[] data, Action<T> process, int minSize = 1) where T : class
        {
            InternalEnqueueProcess(data, process, minSize);
        }

        public static Exception[] Process<T>(int capacity, T[] data, Action<T[]> process, int minSize = 1) where T : class
        {
            return InternalProcess(capacity, data, process, minSize);
        }

        public static Exception[] Process<T>(T[] data, Action<T[]> process, int minSize = 1) where T : class
        {
            return Process(0, data, process, minSize);
        }

        public static Exception[] Process<T>(int capacity, T[] data, Action<T> process, int minSize = 1) where T : class
        {
            return InternalProcess(capacity, data, process, minSize);
        }

        public static Exception[] Process<T>(T[] data, Action<T> process, int minSize = 1) where T : class
        {
            return Process(0, data, process, minSize);
        }

        internal static Exception[] InternalProcess<T>(int capacity, T[] data, Action<T> process, int minSize) where T : class
        {
            return InternalProcess(capacity, data, process.AsProcessMany(), minSize);
        }

        internal static Exception[] InternalProcess<T>(int capacity, T[] data, Action<T[]> process, int minSize) where T : class
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");

            if (data == null) throw new ArgumentNullException("data");
            
            if (process == null) throw new ArgumentNullException("process");

            if (0 == data.Length)
                return new Exception[0];

            var pool = new ExecutionThreads(capacity);
            var threads = pool.Capacity;
            var portions = Distribute(threads, data, minSize);

            foreach (var portion in portions)
            {
                pool.Enqueue(process, portion.ToArray());
            }

            return pool.Join();
        }

        internal void InternalEnqueueProcess<T>(T[] data, Action<T> process, int minSize) where T : class
        {
            InternalEnqueueProcess(data, process.AsProcessMany(), minSize);
        }

        internal void InternalEnqueueProcess<T>(T[] data, Action<T[]> process, int minSize) where T : class
        {
            if (data == null) throw new ArgumentNullException("data");

            if (process == null) throw new ArgumentNullException("process");

            if (0 == data.Length)
                return;

            var threads = Capacity;
            var portions = Distribute(threads, data, minSize);

            foreach (var portion in portions)
            {
                Enqueue(process, portion.ToArray());
            }

            Join();
        }

        static int DivideUp(int divide, int divideBy)
        {
            var division = divide / divideBy;

            if(division * divideBy < divide)
            {
                division ++;
            }

            return division;
        }

        public static List<T>[] Distribute<T>(int threads, T[] data, int minSize)
        {
            threads = Math.Min(DivideUp(data.Length, minSize), threads);

            int ix = 0;
            var portions = new List<T>[threads];

            for (int i = 0; i < portions.Length; i++)
            {
                portions[i] = new List<T>();
            }

            foreach (var item in data)
            {
                portions[ix].Add(item);
                ix++;
                if (ix == threads)
                    ix = 0;
            }

            return portions.Where(p => 0 != p.Count).ToArray();
        }

        internal void Unregister(ExecutionThread task)
        {
            Synchronous(p => p.InternalUnregister(task));
        }

        void InternalUnregister(ExecutionThread task)
        {
            _activeTasks.Remove(task);

            if (null != task.Error)
            {
                _errors.Add(task.Error);
            }

            PromoteTasks();

            if (!_queuedTasks.Concat(_activeTasks).Any())
            {
                _empty.Set();
                Empty?.Invoke(true);
            }
        }

        void InternalEnqueue(ExecutionThread task)
        {
            _empty.Reset();
            Empty?.Invoke(false);
            _queuedTasks.Enqueue(task);
            PromoteTasks();
        }

        void PromoteTasks()
        {
            var extra = Math.Min(Capacity - _activeTasks.Count, _queuedTasks.Count);

            for (int i = 0; i < extra; i++)
            {
                var next = _queuedTasks.Dequeue();
                _activeTasks.Add(next);

                next.Start();
            }
        }

        void Synchronous(Action<ExecutionThreads> deed)
        {
            lock (this)
            {
                deed(this);
            }
        }
    }
}

