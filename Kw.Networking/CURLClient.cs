using Kw.Common;
using Kw.Common.Threading;
using Kw.WinAPI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Kw.Networking
{
    /// <summary>
    /// Внепроцессный Web-клиент на основе CURL.
    /// </summary>
    public class CURLClient : INetworkResourceWebClient
    {
        public static bool ShellExecute { get; set; }

        public string ProxyUri
        {
            get { return null; }
            set { }
        }

        private const string TEMPORARY_FILE_FORMAT = ".curl-{0}.tmp";

        private readonly TemporaryFile OutputFile;
        private readonly TemporaryFile InputFile;

        /// <summary>
        /// Задача выполнения CURL с возможностью отмены.
        /// Входные данные: CURLDialogue.
        /// Выходные данные: код возврата CURL.EXE.
        /// </summary>
        public class CURLInvoker : ExecutionThread<CURLDialogue, int>
        {
            /// <summary>
            /// Отменяет задачу (прерывает процесс).
            /// </summary>
            public void Cancel()
            {
                var assignment = Parameter;

                if (0 != assignment.ProcessId)
                {
                    using(var handle = new Handle(Kernel.OpenProcess(ProcessAccessFlags.Terminate, false, assignment.ProcessId)))
                    {
                        if (IntPtr.Zero != handle)
                        {
                            if (!assignment.Exited)
                            {
                                Kernel.TerminateProcess(handle, 0xc000013a);
                            }
                        }
                        //
                        //    else: must have exited
                    }
                }
            }

            internal CURLInvoker() : base(Invoke, null)
            {
            }

            private static int InvokationCount = 0;

            private static int Evade7(CURLDialogue dialogue)
            {
                try
                {
                    Interlocked.Increment(ref InvokationCount);
                    var commandline = dialogue.CommandLineArguments;

                    ProcessStartInfo command;

                    if (ShellExecute)
                    {
                        command = new ProcessStartInfo("curl.exe", commandline)
                        {
                            UseShellExecute = true,
                        };
                    }
                    else
                    {
                        command = new ProcessStartInfo("curl.exe", commandline)
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        };
                    }


                    var process = new Process
                    {
                        StartInfo = command,
                    };

                    process.Start();

                    dialogue.ProcessId = process.Id;

                    process.WaitForExit();

                    return process.ExitCode;
                }
                catch (Exception x)
                {
                    return -1;
                }
                finally
                {
                    Interlocked.Decrement(ref InvokationCount);
                }
            }

            private static int Invoke(CURLDialogue dialogue)
            {
                try
                {
                    var evaded = Evade7(dialogue);

                    if (7 == evaded)
                    {
                        Thread.Sleep(new Random(DateTime.Now.Millisecond).Next(500, 3000));
                        evaded = Evade7(dialogue);
                    }

                    dialogue.ExitCode = evaded;
                    dialogue.Exited = true;

                    if (0 == evaded)
                    {
                        var output = dialogue[CURLOptionKey.output].Value;

                        try
                        {
                            dialogue.OutputBytes = File.ReadAllBytes(output);
                        }
                        catch (Exception x)
                        {
                            return -1;
                        }
                    }

                    return evaded;
                }
                catch(Exception x)
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Задача выполнения CURL с возможностью отмены.
        /// </summary>
        public CURLInvoker Invoker { get; private set; }

        /// <summary>
        /// Управление запуском CURL.
        /// </summary>
        public CURLDialogue Dialogue { get; private set; }

        /// <summary>
        /// Инициализирует экземпляр <see cref="CURLClient"/>.
        /// </summary>
        /// <param name="dialogue">Управление запуском.</param>
        public CURLClient(CURLDialogue dialogue = null)
        {
            OutputFile = new TemporaryFile(TEMPORARY_FILE_FORMAT);
            InputFile = new TemporaryFile(TEMPORARY_FILE_FORMAT);

            Dialogue = dialogue ?? CURLDialogue.Standard;
        }

        /// <summary>
        /// Начинает асинхронный прием данных.
        /// </summary>
        /// <param name="url">Адрес.</param>
        /// <param name="timeout">Тайм-аут.</param>
        /// <param name="stumbling">Количество повторов.</param>
        /// <returns>Задача выполнения CURL.</returns>
        public CURLInvoker DownloadBytesAsync(string url, int timeout = 0, int stumbling = 0)
        {
            Dialogue.Set(CURLOptionKey.output, OutputFile.Path);
            Dialogue.Set(CURLOptionKey.TARGET, url);
            Dialogue.Set(CURLOptionKey.fail);

            if (0 != timeout)
            {
                var stimeout = timeout / 1000;

                if(0 != stimeout)
                {
                    Dialogue.Set(CURLOptionKey.max_time, stimeout.ToString("{0}"));
                }
            }

            if (0 != stumbling)
            {
                Dialogue.Set(CURLOptionKey.retry, stumbling.ToString("{0}"));
            }

            Invoker = new CURLInvoker { Parameter = Dialogue };
            Invoker.Start();

            return Invoker;
        }

        /// <summary>
        /// Синхронный прием данных.
        /// </summary>
        /// <param name="url">Адрес.</param>
        /// <param name="timeout">Тайм-аут.</param>
        /// <param name="stumbling">Количество повторов.</param>
        /// <returns>Данные.</returns>
        public byte[] DownloadBytes(string url, int timeout = 0, int stumbling = 0)
        {
            var task = DownloadBytesAsync(url, timeout, stumbling);

            task.WaitOne();

            if(0 == task.Result)
            {
                var bytes = task.Parameter.OutputBytes;
                return bytes;
            }

            throw new Win32Exception(task.Result);
        }

        public void Dispose()
        {
            Thread.Sleep(10);

            var outPath = OutputFile.Path;
            var inPath = InputFile.Path;

            OutputFile.Dispose();
            InputFile.Dispose();

            if(File.Exists(outPath))
                throw new IncorrectOperationException();

            if (File.Exists(inPath))
                throw new IncorrectOperationException();
        }
    }
}

