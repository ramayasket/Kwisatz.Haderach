using System;
using System.Threading;
using Kw.Common.Threading;

namespace Kw.Common
{
    /// <summary>
    /// Три состояния логического процесса.
    /// </summary>
    public enum MachineState
    {
        STOPPED = 0,
        RUNNING = 1,
        FAILED = 2,
    }

    /// <summary>
    /// Исполнитель логического процесса на основе автомата трёх состояний.
    /// </summary>
    /// <remarks>
    /// Автомат (и процесс) может:
    /// – не работать (состояние STOPPED);
    /// – работать (состояние RUNNING);
    /// – находиться в неработоспособном состоянии вследствие необработанного исключения (состояние FAILED).
    /// См. комментарии к таблице переходов.
    /// </remarks>
    public abstract partial class ThreeStateMachine
    {
        protected ThreeStateMachine()
        {
            //
            //    Задаём матрицу переходов
            //
            CreateTransitionMatrix();

            _stateChanged = new ManualResetEvent(false);
            _runnerTask = ExecutionThread.StartNew(MachineProc);
        }

        /// <summary>
        /// Проверка состояния для управляемого перехода.
        /// </summary>
        /// <param name="state"></param>
        void ValidateControlledChange(MachineState state)
        {
            //
            //    В качестве управляемого перехода допускаются только STOPPED и RUNNING.
            //
            if (!state.In(MachineState.RUNNING, MachineState.STOPPED))
                throw new ArgumentOutOfRangeException(nameof(state));
        }

        /// <summary>
        /// Переключает процесс в другое состояние (запуск и остановка)
        /// при помощи управляемого перехода.
        /// </summary>
        /// <param name="state">Новое состояние.</param>
        /// <param name="synchronous">Ожидать смены состояния?</param>
        public void ControlledChange(MachineState state, bool synchronous = false)
        {
            ValidateControlledChange(state);

            if (_state == state)    //    нет изменения.
                return;

            lock (this)
            {
                if (null != _stateChanging)
                    throw new InvalidOperationException("State change already in progress.");

                //
                //    Даём сигнал потоку исполнителя о смене состояния.
                //
                _stateChanged.Reset();
                _stateChanging = state;
            }

            if (synchronous)
            {
                WaitOne();
            }
        }

        /// <summary>
        /// Переключает процесс в состояние FAILED при помощи неуправляемого перехода.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="synchronous">Ожидать смены состояния?</param>
        public void UncontrolledChange(Exception error, bool synchronous = false)
        {
            lock (this)
            {
                if(MachineState.RUNNING != _state)
                    throw new InvalidOperationException("State must be RUNNING for this change.");

                if (null != _stateChanging)
                    throw new InvalidOperationException("State change already in progress.");

                //
                //    Даём сигнал потоку исполнителя о смене состояния.
                //
                _stateChanged.Reset();
                _stateChanging = MachineState.FAILED;
                _error = error;
            }

            if (synchronous)
            {
                WaitOne();
            }
        }

        /// <summary>
        /// Ожидает завершения перехода.
        /// </summary>
        public bool WaitOne(int millisecondsTimeout = 0)
        {
            if (0 != millisecondsTimeout)
            {
                return _stateChanged.WaitOne(millisecondsTimeout);
            }

            return _stateChanged.WaitOne();
        }

        /// <summary>
        /// Ожидает завершения потока исполнителя.
        /// </summary>
        public bool WaitEnd(int millisecondsTimeout = 0)
        {
            if (0 != millisecondsTimeout)
            {
                return _runnerTask.WaitOne(millisecondsTimeout);
            }

            return _runnerTask.WaitOne();
        }

        /// <summary>
        /// Завершает поток управления
        /// </summary>
        public void Shutdown(bool wait = true)
        {
            _shutdown = true;

            if (wait)
            {
                _runnerTask.WaitOne();
            }
        }

        public MachineState State => _state;
        public Exception Error => _error;

        protected MachineState _state;
        protected MachineState? _stateChanging;
        protected Exception _error;

        protected bool _shutdown = false;

        protected readonly ManualResetEvent _stateChanged;
        protected readonly ExecutionThread _runnerTask;
    }
}
