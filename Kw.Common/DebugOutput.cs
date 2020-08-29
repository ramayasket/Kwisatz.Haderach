using System;
using System.Runtime.InteropServices;

namespace Kw.Common
{
    /// <summary>
    /// Обеспечивает вывод отладочной информации независимо от конфигурации проекта
    /// </summary>
    /// TODO English comments
    public static class DebugOutput
    {
        /// <summary>
        /// Функция API: вывод отладочной информации
        /// </summary>
        /// <param name="outputString">Строка для вывода</param>
        [DllImport("kernel32.dll", EntryPoint = "OutputDebugString", CharSet = CharSet.Unicode)] static extern void Output(string outputString);

        /// <summary>
        /// Вывод отладочной информации с префиксом и переводом строки
        /// </summary>
        /// <param name="message"></param>
        public static void OutputDebugString(string message)
        {
            Output(message + Environment.NewLine);
        }

        /// <summary>
        /// Вывод отладочной информации с форматированием
        /// </summary>
        /// <param name="format">Форматная строка</param>
        /// <param name="arguments">Объекты для вывода</param>
        public static void WriteLine(string format, params object[] arguments)
        {
            OutputDebugString(string.Format(format, arguments));
        }
    }
}

