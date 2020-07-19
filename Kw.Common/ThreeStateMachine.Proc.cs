using Kw.Common.Threading;
using System.Diagnostics;
using System.Threading;

namespace Kw.Common
{
    /// <summary>
    /// Исполнитель логического процесса на основе автомата трёх состояний.
    /// </summary>
    public abstract partial class ThreeStateMachine
    {
        /// <summary>
        /// Прерывает ожидание и сигнализирует о смене состояния.
        /// </summary>
        /// <returns></returns>
        private bool Signal()
        {
            return _stateChanging != null;
        }

        protected virtual void MachineProcEnter()
        {
        }

        protected virtual void MachineProcLeave()
        {
        }

        /// <summary>
        /// Поток управления логическим процессом.
        /// Переходы между состояниями выполняются в этом потоке.
        /// </summary>
        private void MachineProc()
        {
            Debug.WriteLine($"Entering MachineProc thread id {Thread.CurrentThread.ManagedThreadId}");

            MachineProcEnter();

            while (!Kwisarath.Exiting && !_shutdown)
            {
                //
                //    Ждём в цикле 10 порциями по 100 мс.
                //    Каждые 100 мс. проверяем флаги завершения (вышке).
                //    Если флаги взведены, завершение потока.
                //
                var signal = Interruptable.Wait(1000, 100, Signal);

                //
                //    Также каждые 100 мс. проверяется необходимость перехода.
                //
                if (Interruptable.Signal.Application == signal)
                {
                    lock (this)
                    {
                        Debug.Assert(null != _stateChanging);

                        var transition = this[_state, _stateChanging.Value];

                        if (null != transition)
                        {
                            transition();
                        }
                        else
                        {
                            //
                            //    Выкидывать исключение не имеет смысла, т.к. это внутренний поток исполнителя.
                            //    Поэтому мы просто отбрасываем запрос на изменение.
                            //
                            _stateChanging = null;
                            _stateChanged.Set();
                        }
                    }
                }
            }

            Debug.WriteLine($"Leaving MachineProc thread id {Thread.CurrentThread.ManagedThreadId}");

            MachineProcLeave();
        }
    }
}
