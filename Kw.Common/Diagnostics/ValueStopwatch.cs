using System;
using System.Diagnostics;

namespace Kw.Common.Diagnostics
{
    /// <summary> Тот же самый <see cref="Stopwatch"/> только структура. Не выделяет память в управляемой куче. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Не нужно для этой структуры")]
    public readonly struct ValueStopwatch
    {
        static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        readonly long _startTimestamp;

        /// <summary> Защита от неправильного использования, признак корректной инициализации структуры. </summary>
        public bool IsActive => _startTimestamp != 0;

        ValueStopwatch(long startTimestamp)
        {
            _startTimestamp = startTimestamp;
        }

        /// <summary> Получить новый экземпляр <see cref="ValueStopwatch"/> для замера времени. </summary>
        /// <returns> Объект типа <see cref="ValueStopwatch"/>. </returns>
        public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

        /// <summary> Получить время, прошедшее с момента получения этого экземпляра <see cref="ValueStopwatch"/>. </summary>
        /// <returns> Объект типа <see cref="TimeSpan"/>. </returns>
        public TimeSpan GetElapsedTime()
        {
            // Start timestamp can't be zero in an initialized ValueStopwatch. It would have to be literally the first thing executed when the machine boots to be 0.
            // So it being 0 is a clear indication of default(ValueStopwatch)
            if (!IsActive)
                throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");

            long end = Stopwatch.GetTimestamp();
            long timestampDelta = end - _startTimestamp;
            long ticks = (long)(TimestampToTicks * timestampDelta);
            return new TimeSpan(ticks);
        }
    }
}
