using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Kw.Common.Threading;

namespace Kw.Common
{
    public class FormattedValue<T> where T : struct
    {
        public static string ToFormat(T value)
        {
            return value.ToString();
        }

        public static string ToFormat(DateTime value)
        {
            return value.ToString(Literals.RussianCulture);
        }

        private static T AsT(object v)
        {
            return (T) v;
        }

        public static T ToValue(string formatted, T def = default(T))
        {
            if (string.IsNullOrEmpty(formatted))
            {
                return def;
            }

            //
            //    Date/Time needs a special witchcraft with it.
            //
            if (typeof(DateTime) == typeof(T))
            {
                //
                //    We store DateTime in Russian format, but just in case:
                //    may as well be (rarely) in InvariantCulture (using /).
                //
                var value = DateTime.Parse(formatted, formatted.Contains("/") ? CultureInfo.InvariantCulture : Literals.RussianCulture);

                //
                //    This way the formatted string is compatible with the TryParse() call below.
                //
                formatted = value.ToString(Thread.CurrentThread.CurrentCulture);
            }

            Type realType = typeof(T), formalType = realType;

            if (formalType.IsEnum)
            {
                realType = typeof(Enum);
            }

            //
            //    Getting reflection of T.TryParse(this string s, out T value)
            //
            var method = realType.GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == "TryParse" && m.GetParameters().Count() == 2);

            //
            //    For enum make up TryParse<T>(this string s, out T value)
            //
            if (formalType.IsEnum)
            {
                method = method.MakeGenericMethod(formalType);
            }

            //
            //    Packing objects into array.
            //
            var objects = new object[] { formatted, def };

            //
            //    Call to TryParse presumably under Thread.CurrentThread.CurrentCulture
            //
            return (bool)method.Invoke(null, objects) ? (T)objects[1] : def;
        }
    }
}

