using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        static Information EnsureInformation(string s)
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
        /// Gets timing for a name.
        /// </summary>
        /// <param name="s">Name.</param>
        /// <returns>Timing.</returns>
        public static TimeSpan GetTiming(string s)
        {
            return EnsureInformation(s).Latest;
        }

        /// <summary>
        /// Gets timing information for a name.
        /// </summary>
        /// <param name="s">Name.</param>
        /// <returns>Timing information.</returns>
        public static Information GetInformation(string s)
        {
            return EnsureInformation(s);
        }

        /// <summary>
        /// Sets timing for a name.
        /// </summary>
        /// <param name="s">Name.</param>
        /// <param name="ts">Timing.</param>
        public static void SetTiming(string s, TimeSpan ts)
        {
            EnsureInformation(s).Take(ts);
        }

        /// <summary>
        /// Erases information for a name.
        /// </summary>
        /// <param name="s">Name.</param>
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

        public static TimeSpan Measure(Action method)
        {
            var sw = Stopwatch.StartNew();

            method();

            sw.Stop();
            return sw.Elapsed;
        }

        /// <summary>
        /// Executes an action and takes a measure of it.
        /// </summary>
        /// <param name="method">Action to execute.</param>
        /// <param name="name">Name.</param>
        /// <returns>Timing.</returns>
        public static TimeSpan MeasuredCall(Action method, string name = null)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            name ??= method.Method.Name;

            var sw = Stopwatch.StartNew();

            method();

            sw.Stop();

            SetTiming(name, sw.Elapsed);

            return sw.Elapsed;
        }

        /// <summary>
        /// Calls a function and takes a measure of it.
        /// </summary>
        /// <typeparam name="TResult">Function result type.</typeparam>
        /// <param name="method">Function body.</param>
        /// <param name="name">Name.</param>
        /// <returns>Function's result.</returns>
        public static TResult MeasuredCall<TResult>(Func<TResult> method, string name = null)
        {
            var result = default(TResult);
            Action wrapped = () => { result = method(); };

            MeasuredCall(wrapped, name ?? method.Method.Name);

            return result;
        }

        /// <summary>
        /// Executes an action with an argument and takes a measure of it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method">Action to execute.</param>
        /// <param name="param">Action's argument.</param>
        /// <param name="name">Name.</param>
        public static void MeasuredCall<T>(Action<T> method, T param, string name = null)
        {
            Action wrapped = () => method(param);

            MeasuredCall(wrapped, name ?? method.Method.Name);
        }

        /// <summary>
        /// Calls a function and takes a measure of it.
        /// </summary>
        /// <typeparam name="TP"></typeparam>
        /// <typeparam name="TResult">Function result type.</typeparam>
        /// <param name="method">Function body.</param>
        /// <param name="param">Function's argument.</param>
        /// <param name="name">Name.</param>
        /// <returns>Function's result.</returns>
        public static TResult MeasuredCall<TP, TResult>(Func<TP, TResult> method, TP param, string name = null)
        {
            var result = default(TResult);

            Action wrapped = () => { result = method(param); };

            MeasuredCall(wrapped, name ?? method.Method.Name);
            
            return result;
        }
    }
}

