using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Kw.Common
{
	public static class EnumHelper
	{
		static readonly Dictionary<Type, Enum[]> _enumses = new Dictionary<Type, Enum[]>();

		public static bool IsValueSet(this Enum enumval, Enum test)
		{
			return enumval.GetSeparateValues().Contains(test);
		}

		public static T[] GetSeparateValues<T>(this T enumval, bool synchronous = false)
		{
			var type = enumval.GetType();

			var values = InternalGetValues<T>(type, enumval, synchronous);

			return values;
		}

		public static Enum[] GetValues(this Enum enumval, bool synchronous = false)
		{
			var type = enumval.GetType();

			var values = InternalGetValues<Enum>(type, null, synchronous);

			return values;
		}

		public static Enum[] GetValues(Type type, bool synchronous = false)
		{
			var values = InternalGetValues<Enum>(type, null, synchronous);

			return values;
		}

		public static T[] GetValues<T>(bool synchronous = false)
		{
			var type = typeof(T);

			var values = InternalGetValues<T>(type, null, synchronous);

			return values;
		}

		static T[] InternalGetValues<T>(Type type, object testValue, bool synchronous = false)
		{
			if (null == type) throw new ArgumentNullException("type");
			if(typeof(Enum) == type && null != testValue) throw new ArgumentException("Need to specify exact enum type when testValue is not null.", "type");

			var values = new T[0];

			Action<Type> checkCahe = t =>
			{
				if (_enumses.ContainsKey(t))
				{
					values = _enumses[t].Cast<T>().ToArray();
				}
			};

			Action<Type, Enum[]> setCache = (t, v) => { _enumses[t] = v; };

			
			if (synchronous)
			{
				lock (_enumses)
				{
					checkCahe(type);
				}
			}
			else
			{
				checkCahe(type);
			}

			if (!values.Any())
			{
				var enums = Enum.GetValues(type).Cast<Enum>().ToArray();
				values = enums.Cast<T>().ToArray();

				if (synchronous)
				{
					lock (_enumses)
					{
						setCache(type, enums);
					}
				}
				else
				{
					setCache(type, enums);
				}
			}

			if (null == testValue)
				return values;

			if (testValue is Enum)
			{
				var enumval = Convert.ToInt32(testValue);

				var filtered = new List<int>();

				foreach (var v in values.Cast<Enum>().Select(Convert.ToInt32))
				{
					if (v == (v & enumval))
					{
						filtered.Add(v);
					}
				}

				var enumFiltered = new T[filtered.Count];
				var ix = 0;

				foreach (var f in filtered)
				{
					var s = f.ToString(CultureInfo.InvariantCulture);
					enumFiltered[ix++] = (T)Enum.Parse(type, s);
				}

				return enumFiltered;
			}

			throw new ArgumentException("Expected enum value.", nameof(testValue));
		}

		static int ToInt32(Enum v)
		{
			//Enum.Parse()
			return 0;
		}

		static Enum ToEnum(int v)
		{
			throw new NotImplementedException();
		}
	}
}

