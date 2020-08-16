using Kw.Common.Containers;
using Kw.Common.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Kw.Common
{
    /// TODO add XML comments
    public abstract class CalibratedPipe<T, K> : IDisposable where T : struct
    {
        public int Throughput { get; private set; }

        protected CalibratedPipe(int throughput)
        {
            if (throughput < 1)
                throw new ArgumentOutOfRangeException(nameof(throughput), @"Expected 1 or greater (items per second).");

            Throughput = throughput;

            _resolution = ONEK / throughput;
            _portion = 1;

            ExecutionThread.StartNew(CalibratedTransfer);
        }

        public void Dispose()
        {
            _disposed = true;
        }

        public void Put(T[] data, K id)
        {
            if (null == data) throw new ArgumentNullException(nameof(data));
            if (!data.Any()) throw new ArgumentException(@"Expected non-empty array.", nameof(data));

            lock (_input)
            {
                var pair = (data, id);
                _input.Enqueue(pair);
                _outstanding += data.Length;
            }
        }

        private const int ONEK = 1000;

        private readonly int _portion;
        private readonly int _resolution;
        private int _outstanding;
        private bool _disposed;

        private T[] _buffer;
        private K _id;
        private int _bufferPosition;

        private DateTime _next;

        private readonly Queue<(T[], K)> _input = new Queue<(T[], K)>();

        private TimeSpan Tick => TimeSpan.FromMilliseconds(_resolution);

        private void CalibratedTransfer()
        {
            _next = DateTime.Now;

            while (!_disposed)
            {
                if (DateTime.Now <= _next)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    var todo = _portion;

                    if (_outstanding > 0)
                    {
                        while (_outstanding > 0 && todo > 0)
                        {
                            lock (_input)
                            {
                                var done = Copy(todo);
                                _outstanding -= done;
                                todo -= done;
                            }
                        }

                        _next += Tick;
                    }
                    else
                    {
                        _next += TimeSpan.FromMilliseconds(1);
                    }
                }
            }
        }

        private int Copy(int size)
        {
            if (null == _buffer)
            {
                var pair = _input.Dequeue();
                _buffer = pair.Item1;
                _id = pair.Item2;
                _bufferPosition = 0;
            }

            var pending = _buffer.Length - _bufferPosition;
            var actual = (new[] { pending, size }).Min();

            if (actual == pending)
            {
                Complete(_buffer, _id);
                _buffer = null;
                return actual;
            }

            _bufferPosition += size;
            return size;
        }

        protected abstract void Complete(T[] data, K id);
    }

}

