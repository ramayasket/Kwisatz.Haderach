using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kw.Common.Threading
{
    /// <summary>
    /// Поддержка операций ожидания.
    /// </summary>
    public static class Waitable
    {
        public const int MAX_WAIT_WIDTH = 64;

        /// <summary>
        /// Ожидает получение сигнала всеми элементами массива.
        /// </summary>
        /// <param name="handles">Массив объектов WaitHandle.</param>
        /// <param name="waitTimeout">Тайм-аут ожидания.</param>
        /// <remarks>Метод снимает ограничения на параметр handles. Допустимы массивы размером более 64 элементов и повторяющиеся элементы.</remarks>
        public static bool WaitAll(this WaitHandle[] handles, int waitTimeout = -1)
        {
            var all = handles.Distinct().ToList();

            var ix = 0;

            while (true)
            {
                var portion = all.Skip(ix++ * MAX_WAIT_WIDTH).Take(MAX_WAIT_WIDTH).ToArray();

                if (0 == portion.Length)
                    break;

                if (!WaitHandle.WaitAll(portion, waitTimeout))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Ожидает готовности массива подготавливаемабельных объектов.
        /// </summary>
        /// <param name="source">Исходный массив.</param>
        /// <param name="waitTimeout">Тайм-аут ожидания.</param>
        public static bool WaitAll(this IEnumerable<IPreparable> source, int waitTimeout = -1)
        {
            var handles = source.ToArray().NonEmpty().Select(s => s.Ready).ToArray();

            return WaitAll(handles);
        }

        /// <summary>
        /// Единичный всплеск события
        /// </summary>
        /// <param name="evt">ManualResetEvent</param>
        public static void Pulse(this ManualResetEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));

            evt.Set();
            evt.Reset();
        }

        public static bool WaitPulse(this ManualResetEvent evt, int timeout = -1)
        {
            if (evt == null) throw new ArgumentNullException("evt");

            var wait = evt.WaitOne(timeout);

            if (wait)
            {
                evt.Reset();
            }

            return wait;
        }
    }
}


