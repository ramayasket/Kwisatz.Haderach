using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Kw.Common
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Assignable<T> : IEquatable<T>, IEquatable<Assignable<T>>
	{
		public Assignable(T value = default(T))
			: this()
		{
			_value = value;
			_assigned = true;
		}

		public bool Assigned
		{
			get { return _assigned; }
		}

		public T Value
		{
			get
			{
				if (!_assigned) throw new InvalidOperationException("No value has been assigned yet.");
				return _value;
			}
		}

		#region Implementation

		private readonly bool _assigned;
		private readonly T _value;

		public override string ToString()
		{
			if (!_assigned)
			{
				return string.Empty;
			}

			var isReferenceType = !typeof(T).IsSubclassOf(typeof(ValueType));

			if (isReferenceType && null == (object)_value)
			{
				return "null";
			}

			return _value.ToString();
		}

		#endregion

		#region Operators

		public static implicit operator Assignable<T>(T value)
		{
			return new Assignable<T>(value);
		}

		public static implicit operator T(Assignable<T> assignable)
		{
			return assignable.Value;
		}

		public static bool operator ==(T left, Assignable<T> right)
		{
			return right.Equals(left);
		}

		public static bool operator ==(Assignable<T> left, T right)
		{
			return left.Equals(right);
		}

		public static bool operator ==(Assignable<T> left, Assignable<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(T left, Assignable<T> right)
		{
			return !right.Equals(left);
		}

		public static bool operator !=(Assignable<T> left, T right)
		{
			return !left.Equals(right);
		}

		public static bool operator !=(Assignable<T> left, Assignable<T> right)
		{
			return !left.Equals(right);
		}
	
		#endregion

		#region Equality

		public override int GetHashCode()
		{
			unchecked
			{
				return (_assigned.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(_value);
			}
		}

		public override bool Equals(object that)
		{
			if (!_assigned)
				return false;

			if (null == that)
				return false;

			if (that is T)
			{
				return Equals((T)that);
			}

			if (that is Assignable<T>)
			{
				return Equals((Assignable<T>)that);
			}

			return false;
		}

		public bool Equals(T that)
		{
			return _assigned && Equals(_value, that);
		}

		public bool Equals(Assignable<T> that)
		{
			if (_assigned ^ that._assigned)
				return false;

			return !_assigned || Equals(_value, that._value);
		}

		#endregion
	}
}

