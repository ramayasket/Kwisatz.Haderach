using System;
using System.Diagnostics;
using System.Reflection;

namespace Kw.Common.Diagnostics
{
    /// <summary> Вспомогательный класс для методов, работающих с объектами процессов. </summary>
    public static class ProcessInformation
    {
        /// <summary> Получить имя performance-инстанции процесса по его идентификатору. </summary>
        /// <param name="pid"> Идентификатор процесса. </param>
        /// <returns> Имя performance-инстанции. </returns>
        public static string GetProcessInstanceName(int pid)
        {
            var cat = new PerformanceCounterCategory("Process");

            foreach (string instance in cat.GetInstanceNames())
            {
                using (var counter = new PerformanceCounter("Process", "ID Process", instance, true))
                {
                    try
                    {
                        int val = (int)counter.RawValue;
                        if (val == pid)
                            return instance;
                    }
                    catch (InvalidOperationException)
                    {
                        /* просто игнорируем ошибку, какой-то процесс мог уже завершиться за время перебора цикла, это нормально */
                    }
                }
            }

            throw new ApplicationException("Could not find performance counter instance name for current process.");
        }

        /// <summary> Получить имя performance-инстанции указанного процесса. </summary>
        /// <param name="process"> Процесс. </param>
        /// <returns> Имя performance-инстанции. </returns>
        public static string GetProcessInstanceName(this Process process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            return GetProcessInstanceName(process.Id);
        }

        /// <summary> Весьма незамысловатый способ определения работы на .NET Core. </summary>
        static readonly bool IsNetCoreApp = typeof(bool).Assembly.GetName().Name == "System.Private.CoreLib";

        /// <summary> Порождение процесса-копии текущего процесса. </summary>
        /// <returns> Описатель порождённого процесса. </returns>
        public static Process Spawn()
        {
            var process = new Process();
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;

            if (IsNetCoreApp)
            {
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.Arguments = Assembly.GetEntryAssembly().Location;
            }
            else
            {
                process.StartInfo.FileName = Assembly.GetEntryAssembly().Location;
            }

            process.Start();
            return process;
        }
    }
}
