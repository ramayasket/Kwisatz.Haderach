using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kw.Common
{
    using Whole = Int32;

    /// <summary>
    /// Stores known prime values in two hash sets.
    /// </summary>
    /// <remarks>
    /// Primes in range 1 ÷ int.MaxValue do not fit in one HashSet.
    /// </remarks>
    public class PrimeAccumulator : IEnumerable<Whole>
    {
        private readonly HashSet<Whole> _lower = new HashSet<Whole>();
        private readonly HashSet<Whole> _upper = new HashSet<Whole>();

        private Whole _count;

        private HashSet<Whole> this[Whole value] => value < int.MaxValue / 2 ? _lower : _upper;

        public bool Add(Whole value)
        {
            var add = this[value].Add(value);

            if (add)
            {
                _count ++;
            }

            return add;
        }

        public bool Contains(Whole value)
        {
            return this[value].Contains(value);
        }

        public Whole Length => _count;

        public IEnumerator<Whole> GetEnumerator()
        {
            return _lower.Concat(_upper).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Computes whether Number value is prime.
    /// </summary>
    public static class PrimeComputer
    {
        private static readonly PrimeAccumulator _known = new PrimeAccumulator();
        private static Whole _boundary;

        /// <summary>
        /// Returns a collection of known prime values.
        /// </summary>
        public static IEnumerable<Whole> KnownPrimes => _known;

        public static PrimeAccumulator Accumulator => _known;

        /// <summary>
        /// Returns boundary below which all primes are known.
        /// </summary>
        public static Whole KnownBoundary => _boundary;

        /// <summary>
        /// Returns known prime count without enumerating accumulator.
        /// </summary>
        public static Whole KnownCount => _known.Length;

        /// <summary>
        /// Accepts 1 as prime value.
        /// </summary>
        static PrimeComputer()
        {
            Accept(1);
        }

        /// <summary>
        /// Advances known boundary by 1.
        /// </summary>
        /// <param name="value">Potential new boundary.</param>
        private static void AdvanceBoundary(Whole value)
        {
            if (1 == value - _boundary)
            {
                _boundary = value;
            }
        }

        /// <summary>
        /// Accepts known prime value from a pre-computed range.
        /// </summary>
        /// <param name="value">Prime value.</param>
        public static bool AcceptFromRange(Whole value)
        {
            var add = Accept(value);
            //
            //    For a pre-computed range, assume everything below the value has been checked.
            //
            _boundary = Math.Max(_boundary, value);

            return add;
        }

        /// <summary>
        /// Accepts known prime value.
        /// </summary>
        /// <param name="value"></param>
        private static bool Accept(Whole value)
        {
            var add = _known.Add(value);

            AdvanceBoundary(value);

            return add;
        }

        /// <summary>
        /// Checks whether value is prime.
        /// </summary>
        /// <param name="value">Number to test.</param>
        /// <returns>True if value is prime.</returns>
        public static bool IsPrime(this Whole value)
        {
            if (_known.Contains(value))
                return true;

            if (value <= _boundary)
                return false;

            var upper = (Whole)Math.Sqrt(value);

            bool result = true;

            for (Whole factor = 2; factor <= upper; factor++)
            {
                if (factor.IsPrime())
                {
                    var mod = value % factor;

                    if (0 == mod)
                    {
                        result = false;
                        break;
                    }
                }
            }

            if (result)
                Accept(value);

            else
                AdvanceBoundary(value);

            return result;
        }

        public static Whole[] Factorize(this Whole value)
        {
            if (value.IsPrime())
                return new Whole[0];

            Whole reminder = value;
            var factors = new List<Whole>();

            var upper = (Whole)Math.Sqrt(value);

            for (Whole factor = 2; factor <= upper; factor++)
            {
                if (factor.IsPrime())
                {
                    Whole mod = 0;

                    while (0 == mod)
                    {
                        mod = reminder % factor;
                        if (0 == mod)
                        {
                            reminder /= factor;
                            factors.Add(factor);

                            if (reminder.IsPrime())
                            {
                                if(reminder != 1)
                                    factors.Add(reminder);

                                break;
                            }
                        }
                    }
                }
            }

            return factors.ToArray();
        }
    }
}
