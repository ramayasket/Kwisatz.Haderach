using System;
using System.Diagnostics;

namespace Kw.Common.Diagnostics
{
    /// <summary> Вспомогательный класс для получения сведений о доступной памяти. </summary>
    public sealed class MemoryStatus
    {
        private readonly bool _collect;
        private long _prev;

        /// <summary> Инициализирует новый экземпляр класса <see cref="MemoryStatus"/>. </summary>
        /// <param name="collect"> Нужно ли выполнять полную сборку мусора перед получением счётчика. </param>
        public MemoryStatus(bool collect = true)
        {
            _collect = collect;
            if (_collect)
                Collect();

            using var p = Process.GetCurrentProcess();
            _prev = p.PrivateMemorySize64;
        }

        private static void Collect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary> Снять показания счётчика. </summary>
        /// <returns> Строка для отладки. </returns>
        public string Sample()
        {
            if (_collect)
                Collect();

            using var p = Process.GetCurrentProcess();
            long current = p.PrivateMemorySize64;
            string sample = $"Текущее: {current}, предыдущее: {_prev}, разница: {current - _prev}";
            _prev = current;
            return sample;
        }
    }
}
