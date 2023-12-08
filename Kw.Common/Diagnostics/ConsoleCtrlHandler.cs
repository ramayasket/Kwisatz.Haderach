#pragma warning disable CA1060 // Move pinvokes to native methods class

using System;
using System.Runtime.InteropServices;

namespace Kw.Common.Diagnostics
{
    /// <summary> Вспомогательный класс для обработки закрытия окна консоли (по крестику либо сочетанием клавиш). </summary>
    public sealed class ConsoleCtrlHandler : IDisposable
    {
        /// <summary> Делегат для события от консоли. </summary>
        /// <param name="consoleEvent"> Возникшее событие. </param>
        delegate void ControlEventHandler(ConsoleEvent consoleEvent);

        /// <summary> Установка/снятие обработчика события от консоли. </summary>
        /// <param name="e"> Обработчик. </param>
        /// <param name="add"> true для установки обработчика, false для его удаления. </param>
        /// <returns> true, если метод отработал успешно, иначе false. Причину можно получить с помощью вызова GetLastError. </returns>
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ControlEventHandler e, bool add);

        Action<ConsoleEvent> _action;
        ControlEventHandler _handler;

        /// <summary> Инициализирует новый экземпляр класса <see cref="ConsoleCtrlHandler"/>. </summary>
        /// <param name="action"> Делегат-обработчик. </param>
        public ConsoleCtrlHandler(Action<ConsoleEvent> action)
        {
            _action = action;

            // без явного вызова new ControlEventHandler с сохранением результата в поле не работает:
            // http://stackoverflow.com/questions/4855513/callbackoncollecteddelegate-was-detected
            // http://stackoverflow.com/questions/9957544/callbackoncollecteddelegate-in-globalkeyboardhook-was-detected
            SetConsoleCtrlHandlerWithCheck(_handler = new ControlEventHandler(OnControlEvent), true);
        }

        /// <summary> Обработчик события от консоли. Обработчик должен быстро завершаться. </summary>
        /// <param name="consoleEvent"> Возникшее событие. </param>
        void OnControlEvent(ConsoleEvent consoleEvent) => _action(consoleEvent);

        /// <summary> Уничтожение объекта. </summary>
        public void Dispose()
        {
            SetConsoleCtrlHandlerWithCheck(_handler, false);
            _handler = null;
            _action = null;
        }

        static bool SetConsoleCtrlHandlerWithCheck(ControlEventHandler e, bool add)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("Windows only feature yet");

            return SetConsoleCtrlHandler(e, add);
        }
    }

    /// <summary> Тип события от консоли. </summary>
    public enum ConsoleEvent
    {
        /// <summary> Ctrl+C </summary>
        CtrlC = 0,

        /// <summary> Ctrl+Break </summary>
        CtrlBreak = 1,

        /// <summary> Закрытие консоли </summary>
        CtrlClose = 2,

        /// <summary> Выход из системы </summary>
        CtrlLogoff = 5,

        /// <summary> Выключение машины </summary>
        CtrlShutdown = 6,
    }
}
