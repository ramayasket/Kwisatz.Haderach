using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kw.Common
{
	/// <summary>
	/// Некоторые методы расширения для Reflection
	/// </summary>
	public static class ReflectionExtensions
	{
		public static FieldInfo[] GetFieldsFlattenHierarchy(this Type type, BindingFlags flags)
		{
			if (type == null) throw new ArgumentNullException("type");

			flags &= ~BindingFlags.FlattenHierarchy;
			flags |= BindingFlags.DeclaredOnly;

			var fields = new List<FieldInfo>();
			var current = type;

			var hierarchy = new List<Type>();

			while (current != typeof(object))
			{
				Debug.Assert(null != current);

				hierarchy.Add(current);
				current = current.BaseType;
			}

			hierarchy.Reverse();

			foreach (var t in hierarchy)
			{
				fields.AddRange(t.GetFields(flags));
			}

			return fields.ToArray();
		}

		public static TA QuerySingleAttribute<TA>(this MemberInfo member, bool inherit) where TA : Attribute
		{
			if (member == null) throw new ArgumentNullException(nameof(member));

			if (member.IsDefined(typeof (TA), inherit))
			{
				return member.GetCustomAttributes(typeof (TA), inherit).OfType<TA>().SingleOrDefault();
			}

			return null;
		}

		public static TA[] QueryAttributes<TA>(this MemberInfo member, bool inherit) where TA : Attribute
		{
			if (member == null) throw new ArgumentNullException(nameof(member));

			if (member.IsDefined(typeof(TA), inherit))
			{
				return member.GetCustomAttributes(typeof(TA), inherit).OfType<TA>().ToArray();
			}

			return new TA[0];
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static MethodBase GetCurrentMethod()
		{
			StackTrace st = new StackTrace();
			StackFrame sf = st.GetFrame(1);

			var m = sf.GetMethod();

			return m;
		}
	}
}

