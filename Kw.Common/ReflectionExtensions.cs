using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Kw.Common
{
    /// <summary>
    /// Некоторые методы расширения для Reflection
    /// </summary>
    public static class ReflectionExtensions
    {
        public static void SetFieldValue<T>(this FieldInfo i, ref T target, object value) where T : struct
        {
            i.SetValueDirect(__makeref(target), value);
        }

        public static FieldInfo[] GetFieldsFlattenHierarchy(this Type type, BindingFlags flags)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

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

        public static TA? QuerySingleAttribute<TA>(this MemberInfo member, bool inherit) where TA : Attribute
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            if (member.IsDefined(typeof(TA), inherit))
            {
                return member.GetCustomAttributes(typeof(TA), inherit).OfType<TA>().SingleOrDefault();
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
    }
}

