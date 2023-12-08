using System;
using System.Linq;

namespace Kw.Common
{
    /// <summary>
    /// Typed random generator with range.
    /// </summary>
    /// <typeparam name="T">Type of random values to generate.</typeparam>
    /// <remarks>
    /// Supported types are <seealso cref="SByte"/>, <seealso cref="Byte"/>, <seealso cref="Int16"/>, <seealso cref="UInt16"/>, <seealso cref="Int32"/>, <seealso cref="UInt32"/>, <seealso cref="Single"/>, <seealso cref="Double"/>, <seealso cref="Decimal"/>.
    /// </remarks>
    public sealed class Randomizer<T>
        where T:struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>
    {
        readonly Random _random;
        readonly T _low;
        readonly T _high;
        readonly Type _type;

        //// ReSharper disable StaticMemberInGenericType
        //// ===========================================
        //// WholeTypes and FloatingTypes are initialized and do not change,
        //// so it is safe to ignore the warning.

        /// <summary>
        /// Supported whole number types.
        /// </summary>
        /// <remarks>
        /// Int64 and UInt64 aren't supported because <seealso cref="System.Random"/> has no support for them.
        /// </remarks>
        static readonly Type[] WholeTypes = {
            typeof(sbyte), typeof(byte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
        };

        /// <summary>
        /// Supported floating number types.
        /// </summary>
        static readonly Type[] FloatingTypes = {
            typeof(float), typeof(double), typeof(decimal)
        };

        /// <summary>
        /// Initializes a new instance of the <seealso cref="Randomizer{T}"/> using the specified boundaries.
        /// </summary>
        /// <param name="low">Low boundary.</param>
        /// <param name="high">High boundary.</param>
        /// <remarks>
        /// The range of return values includes low boundary but not high boundary; that is, the return value is greater than or equal to low boundary and less than high boundary.
        /// </remarks>
        public Randomizer(T low, T high)
        {
            _type = typeof(T);

            if (_type.Out(WholeTypes) && _type.Out(FloatingTypes)) {

                var supported = string.Join(", ", WholeTypes.Concat(FloatingTypes).Select(t => $"'{t.Name}'"));
                throw new ArgumentException($"The type '{_type.Name}' isn't supported. The supported types are: {supported}");
            }

            if (low.CompareTo(high) >= 0)
                throw new ArgumentException("Parameter 'low' must not be greater or equal to parameter 'high'");

            _random = new Random(DateTime.Now.Millisecond);

            _low = low;
            _high = high;
        }

        /// <summary>
        /// Returns a random number within the boundaries.
        /// </summary>
        /// <returns>
        /// A numeric value greater than or equal to low boundary and less than high boundary.
        /// </returns>
        public T Next()
        {
            T y;

            if (_type.In(WholeTypes)) {
                var x = NextWhole();
                y = (T) Convert.ChangeType(x, _type);
            }
            else {
                var x = NextFloating();
                y = (T)Convert.ChangeType(x, _type);
            }

            return y;
        }

        int NextWhole()
        {
            var low = (int)Convert.ChangeType(_low, typeof(int));
            var high = (int)Convert.ChangeType(_high, typeof(int));

            return _random.Next(low, high);
        }

        double NextFloating()
        {
            var x = _random.NextDouble();

            var low = (double)Convert.ChangeType(_low, typeof(double));
            var high = (double)Convert.ChangeType(_high, typeof(double));

            var distance = high - low;

            var y = x * distance + low;

            return y;
        }
    }
}
