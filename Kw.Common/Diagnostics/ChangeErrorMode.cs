#pragma warning disable CA1060 // Move pinvokes to native methods class

using System;
using System.Runtime.InteropServices;

namespace Kw.Common.Diagnostics
{
    /// <summary> Вспомогательная структура для управления режимом обработки ошибок процессом. </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "By design")]
    public readonly struct ChangeErrorMode : IDisposable
    {
        /// <summary> https://msdn.microsoft.com/ru-ru/library/windows/desktop/ms680621%28v=vs.85%29.aspx. </summary>
        /// <param name="uMode"> The process error mode. </param>
        /// <returns> Предыдущее значение режима. </returns>
        [DllImport("kernel32.dll")]
        private static extern ErrorModes SetErrorMode(ErrorModes uMode);

        /// <summary> Предыдущий режим. </summary>
        private readonly ErrorModes oldMode;

        /// <summary> Инициализирует новый экземпляр структуры <see cref="ChangeErrorMode"/>, устанавливая режим обработки ошибок. </summary>
        /// <param name="mode"> Режим. </param>
        public ChangeErrorMode(ErrorModes mode)
        {
            oldMode = ErrorModes.Default; // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs0188
            oldMode = SetErrorModeWithCheck(mode);
        }

        /// <summary> Уничтожение на самом деле устанавливает старый режим. </summary>
        public void Dispose() => SetErrorModeWithCheck(oldMode);

        private static ErrorModes SetErrorModeWithCheck(ErrorModes uMode)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("Windows only feature yet");

            return SetErrorMode(uMode);
        }
    }

    /// <summary> Режим обработки ошибок. </summary>
    [Flags]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "WINAPI")]
    public enum ErrorModes : uint
    {
        /// <summary> Use the system default, which is to display all error dialog boxes. </summary>
        Default = 0x0,

        /// <summary> The system does not display the critical-error-handler message box. Instead, the system sends the error to the calling process. </summary>
        FailCriticalErrors = 0x1,

        /// <summary> The system does not display the Windows Error Reporting dialog. </summary>
        NoGpFaultErrorBox = 0x2,

        /// <summary> The system automatically fixes memory alignment faults and makes them invisible to the application. It does this for the calling process and any descendant processes. </summary>
        NoAlignmentFaultExcept = 0x4,

        /// <summary> The system does not display a message box when it fails to find a file. Instead, the error is returned to the calling process. </summary>
        NoOpenFileErrorBox = 0x8000,
    }
}
