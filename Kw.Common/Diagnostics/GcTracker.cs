using System;

namespace Kw.Common.Diagnostics
{
    /// <summary> Структура для отслеживания числа вызовов сборщика мусора. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "By design")]
    public readonly struct GcTracker : IDisposable
    {
        readonly Action<GcCollections> _writer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "By design")]
        static GcTracker()
        {
            if (GC.MaxGeneration != 2)
                throw new NotSupportedException();
        }

        /// <summary> Инициализирует новый экземпляр структуры <see cref="GcTracker"/>. </summary>
        /// <param name="writer"> Делегат для вывода показаний. </param>
        public GcTracker(Action<GcCollections> writer)
        {
            Initial = default;  // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs0188
            InitialInfo = null; // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs0188

            _writer = writer ?? (delta => Console.WriteLine(delta.ToString()));

            Initial = Current;
            InitialInfo = Initial.ToString();
        }

        /// <summary> Получить начальное состояние трекера. </summary>
        public GcCollections Initial { get; }

        /// <summary> Получить текущее состояние трекера. </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "By design")]
        public GcCollections Current => GcCollections.Current;

        /// <summary> Получить начальное состояние трекера. </summary>
        /// <returns> Строка состояния. </returns>
        public string InitialInfo { get; }

        /// <summary> Получить текущее состояние трекера. </summary>
        /// <returns> Строка состояния. </returns>
        public string CurrentInfo => Current.ToString();

        /// <summary> Получить разницу между текущим состоянием и начальным. </summary>
        /// <returns> Строка состояния. </returns>
        public string DeltaInfo => (Current - Initial).ToString();

        /// <summary> "Уничтожить" трекер. </summary>
        public void Dispose() => _writer?.Invoke(Current - Initial);
    }
}
