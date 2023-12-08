using System;
using System.Collections.Generic;
using System.Linq;

namespace Kw.Common
{
    /// <summary>
    /// Conversions between whole numeric types and bit form strings.
    /// </summary>
    /// TODO add byte[]
    public static class BitStringConverter
    {
        static readonly Type[] AllowedTypes =
        {
            typeof(sbyte),
            typeof(byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
        };

        static readonly Dictionary<Type, int> TypeSizes = new[]
        {
            ( typeof(sbyte), 1 ),
            ( typeof(byte), 1 ),
            ( typeof(Int16), 2 ),
            ( typeof(UInt16), 2 ),
            ( typeof(Int32), 4 ),
            ( typeof(UInt32), 4 ),
            ( typeof(Int64), 8 ),
            ( typeof(UInt64), 8 ),
        }
            .ToDictionary(x => x.Item1, x => x.Item2);

        /// <summary>
        /// Converts <seealso cref="UInt64"/> value to bit form string of given length.
        /// </summary>
        /// <param name="x">Value to convert.</param>
        /// <param name="length">Bit form string length. Used to support types other than <seealso cref="UInt64"/>.</param>
        /// <returns>Bit (base 2) representation of value.</returns>
        static string ToBitString(UInt64 x, int length)
        {
            var @var = x;

            var bits = new char[length * 8]; // a 'bit' here is a character '0' or '1'

            for (int i = bits.Length - 1; i >= 0; i--)
            {
                bits[i] = (@var % 2 == 0) ? '0' : '1';
                @var >>= 1;
            }

            var output = new string(bits);
            return output;
        }

        /// <summary>
        /// Converts bit form characters to <seealso cref="UInt64"/> value.
        /// </summary>
        /// <param name="buffer">Bit form characters.</param>
        /// <returns><seealso cref="UInt64"/> value.</returns>
        static UInt64 FromBitString(char[] buffer)
        {
            UInt64 v = 0;

            foreach (var bit in buffer)
            {
                v <<= 1;

                if ('1' == bit)
                    v |= 1;
            }

            return v;
        }

        /// <summary>
        /// Converts from bit form string to whole number.
        /// </summary>
        /// <typeparam name="T">Whole number type/</typeparam>
        /// <param name="bitform">Bit form string.</param>
        /// <returns>Converted whole number value.</returns>
        public static T FromBitString<T>(this string bitform)
            where T : struct
        {
            var type = typeof(T);

            // type check
            if (type.Out(AllowedTypes))
                throw new IncorrectTypeException($"Only whole number types - [s]byte, [U]Int[16,32,64] - are supported");

            // length of bit form depending on type
            var length = TypeSizes[type] * 8;

            // bit form string length check
            if (bitform.Length > length)
                throw new ArgumentException($"String length exceeds {length} which is number of bits in {type.Name}");

            // make an array out of string
            var buffer = bitform.ToCharArray().ToArray();

            // upon group by, we expect an array { '0', '1' }, any other characters make string invalid
            var invalid = buffer
                .GroupBy(x => x)
                .Select(x => x.Key)
                .Except(new[] { '0', '1' })
                .OrderBy(x => x)
                .ToArray();

            if (invalid.Any())
                throw new ArgumentException("Invalid characters in the input string: " + string.Join(",", invalid.Select(x => $"'{x}'")));

            //
            // finally we've done all the checks and have a healthy bitform string
            //
            var v = FromBitString(buffer); // convert string to UInt64
            var vx = Convert.ChangeType(v, type); // and then convert to the needed type

            return (T)vx;
        }

        /// <summary> Converts number to bit string form. </summary>
        public static string ToBitString(this Int64 x) => ToBitString(Convert.ToUInt64(x), sizeof(Int64));

        /// <summary> Converts number to bit string form. </summary>
        public static string ToBitString(this UInt64 x) => ToBitString(Convert.ToUInt64(x), sizeof(UInt64));

        /// <summary> Converts number to bit string form. </summary>
        public static string ToBitString(this Int32 x) => ToBitString(Convert.ToUInt64(x), sizeof(Int32));

        /// <summary> Converts number to bit string form. </summary>
        public static string ToBitString(this UInt32 x) => ToBitString(Convert.ToUInt64(x), sizeof(UInt32));

        /// <summary> Converts number to bit string form. </summary>
        public static string ToBitString(this Int16 x) => ToBitString(Convert.ToUInt64(x), sizeof(Int16));

        /// <summary> Converts number to bit string form. </summary>
        public static string ToBitString(this UInt16 x) => ToBitString(Convert.ToUInt64(x), sizeof(UInt16));

        /// <summary> Converts number to bit string form. </summary>
        public static string ToBitString(this byte x) => ToBitString(Convert.ToUInt64(x), sizeof(byte));

        /// <summary> Converts number to bit string form. </summary>
        public static string ToBitString(this sbyte x) => ToBitString(Convert.ToUInt64(x), sizeof(sbyte));
    }
}
