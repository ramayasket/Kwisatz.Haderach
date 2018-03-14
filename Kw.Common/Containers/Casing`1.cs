using System;
using System.Globalization;

namespace Kw.Common.Containers
{
	/// <summary>
	/// Reference-typed wrapper around a structure.
	/// </summary>
	/// <typeparam name="T">Type of value.</typeparam>
	public class Casing<T> where T : struct
	{
		public T Value { get; set; }

		public Casing()
		{
			Value = default (T);
		}

		public Casing(T value = default(T))
		{
			Value = value;
		}

		public Casing(Casing<T> other)
		{
			Value = other.Value;
		}

		public static implicit operator T(Casing<T> that)
		{
			return that.Value;
		}

		public static implicit operator Casing<T>(T value)
		{
			return new Casing<T>(value);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, @"{0}", Value);
		}
	}
}

