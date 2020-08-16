using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Kw.Common
{
    /// <summary>
    /// Measures execution timings.
    /// </summary>
    public static class Timings
    {
        /// <summary>
        /// Timings information.
        /// </summary>
        public class Information
        {
            /// <summary>
            /// Latest execution timing.
            /// </summary>
            public TimeSpan Latest { get; private set; }

            /// <summary>
            /// Average execution timing.
            /// </summary>
            public TimeSpan Average { get; private set; }

            /// <summary>
            /// Total execution timing.
            /// </summary>
            public TimeSpan Total { get; private set; }

            /// <summary>
            /// Number of measurings.
            /// </summary>
            public int Count { get; private set; }

            internal void Take(TimeSpan ts)
            {
                Latest = ts;

                Total += ts;
                Count++;

                Average = TimeSpan.FromTicks(Total.Ticks / Count);
            }
        }

        internal static readonly Dictionary<string, Information> _entries = new Dictionary<string, Information>();

        private static Information EnsureToken(string s)
        {
            lock (_entries)
            {
                var entry = new Information();

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

        /// <summary>
        /// Returns
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static TimeSpan Get(string s)
        {
            return EnsureToken(s).Latest;
        }

        public static Information GetInfo(string s)
        {
            return EnsureToken(s);
        }

        public static void Set(string s, TimeSpan ts)
        {
            EnsureToken(s).Take(ts);
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

        public static TimeSpan MeasuredCall(Action wrapped, string name, MethodBase minfo)
        {
            if (wrapped == null) throw new ArgumentNullException(nameof(wrapped));
            if (minfo == null) throw new ArgumentNullException(nameof(minfo));

            name ??= minfo.Name;
            var type = minfo.DeclaringType;

            var typeName = "<>";

            if (null != type)
            {
                typeName = type.Name;
            }

            var sw = Stopwatch.StartNew();

            wrapped();

            sw.Stop();

            Set(name, sw.Elapsed);

            return sw.Elapsed;
        }

        public static TimeSpan Measure(Action method)
        {
            var sw = Stopwatch.StartNew();

            method();

            sw.Stop();
            return sw.Elapsed;
        }
        
        public static void MeasuredCall(Action method, string name = null)
        {
            MeasuredCall(method, name, method.Method);
        }

        public static TResult MeasuredCall<TResult>(Func<TResult> method, string name = null)
        {
            var result = default(TResult);
            Action wrapped = () => { result = method(); };

            MeasuredCall(wrapped, name, method.Method);

            return result;
        }

        public static void MeasuredCall<T>(Action<T> method, T param, string token = null)
        {
            Action wrapped = () => method(param);

            MeasuredCall(wrapped, token, method.Method);
        }

        public static TR MeasuredCall<TP, TR>(Func<TP, TR> method, TP param, string token = null)
        {
            var result = default(TR);
            Action wrapped = () => { result = method(param); };

            MeasuredCall(wrapped, token, method.Method);
            
            return result;
        }
    }
}

