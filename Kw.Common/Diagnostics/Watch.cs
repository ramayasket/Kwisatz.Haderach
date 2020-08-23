#pragma warning disable CA1060 // Move pinvokes to native methods class

using System;
using System.Runtime.InteropServices;

namespace Kw.Common.Diagnostics
{
    /// <summary> Таймер для замера времени работы участков кода. Похож на Stopwatch, даёт возможность использования себя в блоке using. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Не нужно для этой структуры")]
    public struct Watch : IDisposable
    {
        private readonly Action<double> _writer;
        private long _start;

        /// <summary> Инициализирует новый экземпляр структуры <see cref="Watch"/>. </summary>
        /// <param name="writer"> Необязательный собственный делегат для вывода результатов замеров. </param>
        public Watch(Action<double> writer = null)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("Windows only feature yet");

            _writer = writer ?? (seconds => Console.WriteLine(seconds.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)));
            _start = 0;

            Start();
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32", EntryPoint = "QueryPerformanceCounter", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(ref long lPerformanceCounter);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32", EntryPoint = "QueryPerformanceFrequency", SetLastError = true)]
        private static extern bool QueryPerformanceFrequency(out long frequency);

        /// <summary> Запустить таймер. </summary>
        public void Start() => QueryPerformanceCounter(ref _start);

        /// <summary> Получить число секунд, прошедших с последнего вызова метода <c>Watch.Start()</c> таймера. </summary>
        /// <returns> Число секунд как число двойной точности. </returns>
        public double GetSeconds()
        {
            long current = _start;
            return !QueryPerformanceFrequency(out long freq) || !QueryPerformanceCounter(ref current) ? -1 : (current - _start) / (double)freq;
        }

        /// <summary> Преобразовать в строку. </summary>
        /// <returns> Число секунд, прошедших с последнего вызова метода <c>Watch.Start()</c> таймера. </returns>
        public override string ToString() => GetSeconds().ToString("F4", System.Globalization.CultureInfo.InvariantCulture);

        /// <summary> Освободить ресурсы. </summary>
        /// <remarks> Здесь происходит печать результата измерения времени, прошедшего с последнего вызова метода <c>Watch.Start()</c> таймера. </remarks>
        public void Dispose() => _writer?.Invoke(GetSeconds());
    }
}
