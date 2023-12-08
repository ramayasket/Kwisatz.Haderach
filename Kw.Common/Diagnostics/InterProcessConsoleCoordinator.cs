using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kw.Common.Diagnostics
{
    /// <summary> Координатор для межпроцессного взаимодействия нескольких консольных приложений. Позволяет синхронизировать запуск/остановку работы в них. </summary>
    public sealed class InterProcessConsoleCoordinator : IDisposable
    {
        EventWaitHandle _pauseResumeCoorninationHandle;
        EventWaitHandle _exitCoordinationHandle;
        Thread _controlThread;
        bool _disposed;

        /// <summary> Инициализирует новый экземпляр класса <see cref="InterProcessConsoleCoordinator"/> с заданным уникальным именем. </summary>
        /// <param name="coordinationName"> Уникальное имя. Все приложения, создавшие координатора с таким именем, смогут синхронизироваться между собой. </param>
        public InterProcessConsoleCoordinator(string coordinationName)
        {
            _pauseResumeCoorninationHandle = new EventWaitHandle(false, EventResetMode.ManualReset, coordinationName);
            _exitCoordinationHandle = new EventWaitHandle(false, EventResetMode.ManualReset, coordinationName + "_exit");
        }

        /// <summary> Можно ли выполнять полезную нагрузку. </summary>
        bool CanExecutePayload => _pauseResumeCoorninationHandle.WaitOne(0);

        /// <summary> Запрошена остановка координатора. </summary>
        bool IsCancellationRequested => _exitCoordinationHandle.WaitOne(0);

        /// <summary> Текст приглашения к продолжению работы (запуск полезной нагрузки). </summary>
        public string HelpText => $"Ожидание ввода команды: '{ResumeKey}' для продолжения, '{PauseKey}' для приостановки, '{ExitKey}' для выхода, '{SpawnKey}' для создания нового окна консоли...";

        /// <summary> Клавиша для продолжения работы. Координатор разрешит выполнение полезной нагрузки. </summary>
        public ConsoleKey ResumeKey { get; set; } = ConsoleKey.Enter;

        /// <summary> Клавиша для приостановки работы. </summary>
        public ConsoleKey PauseKey { get; set; } = ConsoleKey.Spacebar;

        /// <summary> Клавиша для остановки координатора. </summary>
        public ConsoleKey ExitKey { get; set; } = ConsoleKey.Escape;

        /// <summary> Клавиша для запуска нового процесса, попадающего под управление координатора. </summary>
        public ConsoleKey SpawnKey { get; set; } = ConsoleKey.F3;

        /// <summary> Запуск полезной нагрузки. </summary>
        /// <param name="payload"> Делегат с полезной нагрузкой. </param>
        /// <returns> Task for await. </returns>
        public async Task RunAsync(Func<Task> payload)
        {
            GuardDisposed();

            _controlThread = new Thread(() =>
            {
                while (!IsCancellationRequested)
                    CoordinateInput(100);
            });

            _controlThread.Start();

            while (!IsCancellationRequested)
            {
                if (CanExecutePayload)
                {
                    if (payload != null)
                        await payload().ConfigureAwait(false);
                }
                else
                {
                    Console.WriteLine(HelpText);

                    while (!IsCancellationRequested && !CanExecutePayload)
                    {
                        CoordinateInput(50);
                    }
                }
            }
        }

        void CoordinateInput(int delayMsec)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true); // не выводить нажатую клавишу на консоль
                if (key.Key == ResumeKey)
                    _pauseResumeCoorninationHandle.Set();
                else if (key.Key == ExitKey)
                    _exitCoordinationHandle.Set();
                else if (key.Key == SpawnKey)
                    Spawn();
                else if (key.Key == PauseKey)
                    _pauseResumeCoorninationHandle.Reset();
                else
                    Console.WriteLine("Неизвестная команда. " + HelpText);
            }
            else
            {
                Thread.Sleep(delayMsec);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "By design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable.", Justification = "By design")]
        static void Spawn()
        {
            try
            {
                ProcessInformation.Spawn();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Console.WriteLine(ex.Message);
            }
        }

        void GuardDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(InterProcessConsoleCoordinator));
        }

        /// <summary> Уничтожить координатор и освободить его ресурсы. </summary>
        public void Dispose()
        {
            if (_disposed) return;

            if (_controlThread != null)
            {
                _controlThread.Join();
                _controlThread = null;
            }

            if (_pauseResumeCoorninationHandle != null)
            {
                _pauseResumeCoorninationHandle.Dispose();
                _pauseResumeCoorninationHandle = null;
            }

            if (_exitCoordinationHandle != null)
            {
                _exitCoordinationHandle.Dispose();
                _exitCoordinationHandle = null;
            }

            _disposed = true;
        }
    }
}
