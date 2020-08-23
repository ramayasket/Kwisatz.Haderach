using System;
using System.Threading;

namespace Kw.Common.Diagnostics
{
    /// <summary> Счётчик созданных объектов. Предполагается использовать как приватное поле целевого класса. </summary>
    /// <typeparam name="T"> Целевой тип, для которого работает счётчик. </typeparam>
    /// <remarks> Специально struct, чтобы избежать ненужной аллокации в куче. Шаблонный параметр нужен для разграничения хранения _globalIndex для счётчиков разных типов объектов. </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "By design")]
    public readonly struct InstanceCounter<T>
    {
        private readonly long _currentIndex;
        private static long _globalIndex;

        /// <summary> Инициализирует новый экземпляр структуры <see cref="InstanceCounter{T}"/>. </summary>
        /// <param name="unused"> Неиспользуемый параметр, введённый из-за невозможности иметь конструкторы по умолчанию для структур. </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "By design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "By design")]
        public InstanceCounter(bool unused = false)
        {
            _currentIndex = Interlocked.Increment(ref _globalIndex);
        }

        /// <summary> Преобразовать к строке. </summary>
        /// <returns> Строка с индексом для отладки. </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "By design")]
        public override string ToString()
        {
            if (_currentIndex == 0)
                throw new InvalidOperationException("Был по ошибке вызван конструктор по умолчанию, нужно использовать параметризованный конструктор.");

            return $"{typeof(T).Name} #{_currentIndex}";
        }
    }
}
