using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace Kw.Common
{
	/// <summary>
	/// Методы расширения для класса System.String
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Преобразует строку в нотации PASCAL во фразу с расставленными пробелами.
		/// </summary>
		/// <param name="pascal">Строка в нотации PASCAL.</param>
		/// <returns>Преобразованная фраза.</returns>
		public static string Unpascal(this string pascal)
		{
			int i = 0;
			var accumulator = new StringBuilder();

			foreach (var c in pascal)
			{
				var s = c.ToString(CultureInfo.InvariantCulture);

				var low = s.ToLower();
				var high = s.ToUpper();

				if (low != high && s == high && i > 0)
				{
					accumulator.Append(' ');
				}

				accumulator.Append(s);

				i++;
			}

			var accumulated = accumulator.ToString();

			return accumulated;
		}

		/// <summary>
		/// Returns a string array that contains the substrings in this string that are delimited by specified string.
		/// </summary>
		/// <param name="text">String to split.</param>
		/// <param name="separator">A string that delimit the substrings in this string, an empty array that contains no delimiters, or null.</param>
		/// <param name="options">Specifies whether to return empty array elements.</param>
		/// <returns>An array whose elements contain the substrings in this string that are delimited by separator.</returns>
		public static string[] Split(this string text, string separator, StringSplitOptions options = StringSplitOptions.None)
		{
			if (null == text) throw new ArgumentNullException("text");

			return text.Split(new[] {separator}, options);
		}

		public static string DeflateSpaces(this string text)
		{
			if (null == text) throw new ArgumentNullException("text");

			string w = text, w0 = string.Empty;
			do
			{
				w0 = w;
				w = w.Replace("  ", " ");
			} while (w != w0);

			var trim = w.Trim();
			return trim;
		}

		public static bool IsNonEmpty(this string s)
		{
			return !string.IsNullOrEmpty(s) && (string.Empty != s.DeflateSpaces());
		}

		public static int Distance(this string source, string target)
		{
			return LevenshteinDistance(source, target);
		}

		public static string FixDecode(this string text)
		{
			if (null == text) throw new ArgumentNullException("text");

			string @fixed;

			if (text.Contains("&amp;"))
			{
				var rx = new Regex("&(amp;)+");
				@fixed = rx.Replace(text, "&");
			}
			else
			{
				@fixed = text;
			}

			return @fixed;
		}

		public static string Invert(this string s)
		{
			if (null == s)
				return null;
			
			var reverse = s.Reverse().ToArray();
			var builder = new StringBuilder(s.Length);

			foreach (var c in reverse)
			{
				builder.Append(c);
			}

			return builder.ToString();
		}

		public static long GetLongHashCode(this string s)
		{
#if ANYCPU
			const uint LOW_MASK = 0x00000000ffffffff;

			var invert = s.Invert();

			var low = s.GetHashCode();
			var high = (long)(invert.GetHashCode());

			var combined = (high << 32) | (low & LOW_MASK);
			return combined;
#else
#error `Any CPU` is not defined!
			throw new InvalidOperationException("`Any CPU` is not defined!");
#endif
		}

		public static string InvariantString(this long value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string InvariantString(this int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string InvariantString(this short value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string InvariantString(this float value)
		{
			return AsNonInteger(value.ToString("F", CultureInfo.InvariantCulture));
		}

		public static string InvariantString(this double value)
		{
			return AsNonInteger(value.ToString("F", CultureInfo.InvariantCulture));
		}

		private static string AsNonInteger(string s)
		{
			if (s.Contains('.'))
				return s;

			return s + ".0";
		}

		public static string RemoveWhitespace(this string input)
		{
			return input.Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty).Replace("\r\n", string.Empty);
		}
		//
		//	
		//
		private static int LevenshteinDistance(string source, string target)
		{
			//
			//	Degenerate cases.
			//
			if (source == target) return 0;
			if (source.Length == 0) return target.Length;
			if (target.Length == 0) return source.Length;

			//
			//	Create two work vectors of integer distances.
			//
			int[] previous = new int[target.Length + 1], current = new int[target.Length + 1];

			//
			//	Initialize previous row of distances.
			//	This row is A[0][i]: edit distance for an empty source
			//	the distance is just the number of characters to delete from target
			for (int i = 0; i < previous.Length; i++)
				previous[i] = i;

			for (int i = 0; i < source.Length; i++)
			{
				// calculate v1 (current row distances) from the previous row v0

				// first element of v1 is A[i+1][0]
				//   edit distance is delete (i+1) chars from source to match empty target
				current[0] = i + 1;

				// use formula to fill in the rest of the row
				for (int j = 0; j < target.Length; j++)
				{
					var cost = (source[i] == target[j]) ? 0 : 1;
					current[j + 1] = new[] { current[j]+1, previous[j+1]+1, previous[j] + cost }.Min();
				}

				// copy v1 (current row) to v0 (previous row) for next iteration
				for (int j = 0; j < previous.Length; j++)
					previous[j] = current[j];
			}

			var ret = current[target.Length];
			
			//AppCore.WriteLine("LevenshteinDistance('{0}', '{1}') is {2}.", source, target, ret);
			
			return ret;
		}

	}
}

