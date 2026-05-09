using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kw.Common
{
    public class TaskBasedEvent
    {
        volatile TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task WaitAsync(CancellationToken cancellationToken = default) =>
            WaitAsync(TimeSpan.MaxValue, cancellationToken);

        public Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (!cancellationToken.CanBeCanceled)
                return _tcs.Task;

            return _tcs.Task.WaitAsync(timeout, cancellationToken);
        }

        public void Set()
        {
            _tcs.TrySetResult(true);
        }

        public void Reset()
        {
            while (true)
            {
                var tcs = _tcs;

                if (!tcs.Task.IsCompleted)
                    return;

                var newTcs = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

                if (Interlocked.CompareExchange(ref _tcs, newTcs, tcs) == tcs)
                    return;
            }
        }
    }}
