using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Kw.Common
{
    public static class ExecutionTimings
    {
        internal static readonly Dictionary<string, ExecutionTimingInfo> _entries = new Dictionary<string, ExecutionTimingInfo>();

        static ExecutionTimingInfo EnsureToken(string s)
        {
            lock (_entries)
            {
                var entry = new ExecutionTimingInfo();

                if (_entries.ContainsKey(s))
                {
                    entry = _entries[s];
                }
                else
                {
                    _entries[s] = entry;
                }

                return entry;
            }
        }

        public static TimeSpan Get(string s)
        {
            return EnsureToken(s).Latest;
        }

        public static ExecutionTimingInfo GetInfo(string s)
        {
            return EnsureToken(s);
        }

        public static void Set(string s, TimeSpan ts)
        {
            EnsureToken(s).Set(ts);
        }

        public static void Reset(string s)
        {
            lock (_entries)
            {
                if (_entries.ContainsKey(s))
                {
                    _entries.Remove(s);
                }
            }
        }

        public static bool ReportTimings { get; set; }

        public static TimeSpan MeasuredCall(Action wrapped, string? token, MethodBase minfo)
        {
            if (wrapped == null) throw new ArgumentNullException(nameof(wrapped));
            if (minfo == null) throw new ArgumentNullException(nameof(minfo));

            token ??= minfo.Name;
            var type = minfo.DeclaringType;

            var typeName = "<>";

            if (null != type)
            {
                typeName = type.Name;
            }

            var sw = Stopwatch.StartNew();

            wrapped();

            sw.Stop();
            

            Set(token, sw.Elapsed);

            if (ReportTimings)
            {
                Debug.WriteLine("@PX Execution timing '{0}.{1}': {2}", typeName, token, sw.Elapsed);
            }

            return sw.Elapsed;
        }

        public static async Task<TimeSpan> MeasuredCall(Func<Task> wrapped, string? token, MethodBase minfo)
        {
            if (wrapped == null) throw new ArgumentNullException(nameof(wrapped));
            if (minfo == null) throw new ArgumentNullException(nameof(minfo));

            token ??= minfo.Name;
            var type = minfo.DeclaringType;

            var typeName = "<>";

            if (null != type)
            {
                typeName = type.Name;
            }

            var sw = Stopwatch.StartNew();

            await wrapped();

            sw.Stop();


            Set(token, sw.Elapsed);

            if (ReportTimings)
            {
                Debug.WriteLine("@PX Execution timing '{0}.{1}': {2}", typeName, token, sw.Elapsed);
            }

            return sw.Elapsed;
        }

        public static TimeSpan Measure(Action method)
        {
            var sw = Stopwatch.StartNew();

            method();

            sw.Stop();
            return sw.Elapsed;
        }

        public static async Task<TimeSpan> Measure(Func<Task> method)
        {
            var sw = Stopwatch.StartNew();

            await method();

            sw.Stop();
            return sw.Elapsed;
        }

        public static void MeasuredCall(Action method, string token = null)
        {
            MeasuredCall(method, token, method.Method);
        }

        public static async Task MeasuredCall(Func<Task> method, string token = null)
        {
            await MeasuredCall(method, token, method.Method);
        }

        public static TResult MeasuredCall<TResult>(Func<TResult> method, string token = null)
        {
            var result = default(TResult);
            Action wrapped = () => { result = method(); };

            MeasuredCall(wrapped, token, method.Method);

            return result;
        }

        public static async Task<TResult> MeasuredCall<TResult>(Func<Task<TResult>> method, string token = null)
        {
            var result = default(TResult);
            Func<Task> wrapped = async () => { result = await method(); };

            await MeasuredCall(wrapped, token, method.Method);

            return result;
        }

        public static void MeasuredCall<T>(Action<T> method, T param, string token = null)
        {
            Action wrapped = () => method(param);

            MeasuredCall(wrapped, token, method.Method);
        }

        public static async Task MeasuredCall<T>(Func<T, Task> method, T param, string token = null)
        {
            Func<Task> wrapped = async () => await method(param);

            await MeasuredCall(wrapped, token, method.Method);
        }

        public static TR MeasuredCall<TP, TR>(Func<TP, TR> method, TP param, string token = null)
        {
            var result = default(TR);
            Action wrapped = () => { result = method(param); };

            MeasuredCall(wrapped, token, method.Method);
            
            return result;
        }

        public static async Task<TR> MeasuredCall<TP, TR>(Func<TP, Task<TR>> method, TP param, string token = null)
        {
            var result = default(TR);
            Func<Task> wrapped = async () => { result = await method(param); };

            await MeasuredCall(wrapped, token, method.Method);

            return result;
        }
    }
}

