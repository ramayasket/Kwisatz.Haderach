#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Kw.Common
{
    /// <summary>
    /// Разные методы расширения общего назначения.
    /// </summary>
    /// TODO English comments
    public static class CommonExtensions
    {
        /// <summary>
        /// Safely dereferences a .
        /// </summary>
        /// <typeparam name="T">Type of object to dereference.</typeparam>
        /// <param name="x"></param>
        /// <param name="expression"></param>
        /// <returns>Value of expression or null if x is null. </returns>
        public static object? SafeTake<T>(this T? x, Expression<Func<T, object>> expression) where T : class
        {
            if (null == x)
                return null;

            var function = expression.Compile();

            return function(x);
        }

        /// <summary>
        /// Returns value of nullable if not null or default otherwise.
        /// </summary>
        /// <typeparam name="T">Struct type.</typeparam>
        /// <param name="nvalue">Nullabe value.</param>
        public static T ValueOrDefault<T>(this T? nvalue) where T:struct
        {
            // ReSharper disable once MergeConditionalExpression
            return nvalue.HasValue ? nvalue.Value : default(T);
        }
        
        /// <summary>
        /// Safely returns dictionary entry.
        /// </summary>
        /// <typeparam name="TK">Key type</typeparam>
        /// <typeparam name="TV">Value type</typeparam>
        /// <param name="map">Dictionary object</param>
        /// <param name="key">Key object</param>
        /// <returns>Keyed value or default</returns>
        public static TV ValueOrDefault<TK, TV>(this IDictionary<TK, TV> map, TK key)
        {
            return map.ContainsKey(key) ? map[key] : default(TV);
        }
        /// <summary>
        /// Returns new map with pairs copied from another map.
        /// </summary>
        /// <typeparam name="K">Key type.</typeparam>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="from">Source map.</param>
        /// <returns>New map.</returns>
        public static Dictionary<K, T> Clone<K, T>(this IDictionary<K, T> from)
        {
            var @new = new Dictionary<K, T>();
            var pairs = from.ToArray();

            foreach(var pair in pairs)
            {
                @new[pair.Key] = pair.Value;
            }

            return @new;
        }

        /// <summary>
        /// Returns new list with values copied from another list.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="from">Source list.</param>
        /// <returns>New list.</returns>
        public static List<T> Clone<T>(this IList<T> from)
        {
            var @new = new List<T>();

            @new.AddRange(from);

            return @new;
        }

        /// <summary>
        /// Returns new array with values copied from another array.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="from">Source array.</param>
        /// <returns>New array.</returns>
        public static T[] Clone<T>(this T[] from)
        {
            var @new = new T[from.Length];

            for(int i = 0; i < from.Length; i++)
            {
                @new[i] = from[i];
            }

            return @new;
        }

        /// <summary>
        /// Возвращает строковое представление объекта.
        /// </summary>
        /// <param name="target">Целевой объект.</param>
        /// <param name="replacement"></param>
        /// <returns>Строковое представление целевого объекта или пустая строка если он не задан.</returns>
        public static string SafeToString(this object target, string replacement = "")
        {
            if (null == target)
                return replacement;

            return target.ToString();
        }

        public static string AsDebuggerDisplay(this object @object)
        {
            if(null == @object)
                return "null";

            if(@object is string @string)
                return "\"" + @string + "\"";

            if(@object is char c)
            {
                return "'" + c + "'";
            }

            return @object.ToString();
        }

        /// <summary>
        /// Сокращенная форма switch.
        /// </summary>
        /// <typeparam name="T">Тип данных.</typeparam>
        /// <param name="argument">Исходные данные.</param>
        /// <param name="condition">Проверочные данные.</param>
        /// <param name="result">Результат при проверке.</param>
        /// <returns>Если аргумент равен условию, то измененный аргумент, иначе неизменный аргумент.</returns>
        public static T InCase<T>(this T argument, T condition, T result)
        {
            return Equals(argument, condition) ? result : argument;
        }

        /// <summary>
        /// Сокращённая форма switch.
        /// </summary>
        /// <typeparam name="T">Тип данных.</typeparam>
        /// <param name="argument">Исходные данные.</param>
        /// <param name="condition">Проверочные данные.</param>
        /// <param name="result">Результат при проверке.</param>
        /// <returns>Если аргумент равен условию, то измененный аргумент, иначе неизменный аргумент.</returns>
        public static object InCase<T>(this T argument, T condition, object result)
        {
            return Equals(argument, condition) ? result : argument;
        }

        /// <summary>
        /// Возвращает непустые элементы массива.
        /// </summary>
        /// <typeparam name="T">Тип элемента массива.</typeparam>
        /// <param name="source">Исходный массив.</param>
        /// <returns>Отфильтрованный массив.</returns>
        public static T[] NonEmpty<T>(this T[] source)
        {
            return source.Where(v => !Equals(default(T), v)).ToArray();
        }

        /// <summary>
        /// Возвращает непустые элементы коллекции.
        /// </summary>
        /// <typeparam name="T">Тип элемента коллекции.</typeparam>
        /// <param name="source">Исходная коллекция.</param>
        /// <returns>Отфильтрованная коллекция.</returns>
        public static IEnumerable<T> NonEmpty<T>(this IEnumerable<T> source)
        {
            return source.Where(v => !Equals(default(T), v));
        }

        /// <summary>
        /// Возвращает значение или значение по умолчанию.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="value">Проверяемое значение.</param>
        /// <param name="def">Значение по умолчанию.</param>
        /// <returns></returns>
        public static T WhenEmpty<T>(this T value, T def)
        {
            if (Equals(default(T), value))
            {
                value = def;
            }

            return value;
        }

        /// <summary>
        /// Проверяет присваиваемость типа к образцовому.
        /// </summary>
        /// <typeparam name="T">Образцовый тип.</typeparam>
        /// <param name="target">Исследуемый тип.</param>
        /// <returns>True, если переменной образцового типа может быть присвоена ссылка на объект исследуемого типа. Другими словами, если исследуемый тип уналедован от образцового. Иначе, False.</returns>
        public static bool Is<T>(this Type target)
        {
            return target.Is(typeof (T));
        }

        /// <summary>
        /// Проверяет принадлежность объекта к образцовому типу.
        /// </summary>
        /// <typeparam name="T">Образцовый тип.</typeparam>
        /// <param name="target">Исследуемый объект.</param>
        /// <returns>True если исследуемый объект не null и может использоваться для присваивания переменной типа T.</returns>
        public static bool Is<T>(this object target)
        {
            if(null == target)
                return false;

            return target.GetType().Is(typeof(T));
        }

        public static bool Is(this object target, Type examplar)
        {
            if (null == target)
                return false;

            return target.GetType().Is(examplar);
        }

        public static bool Is(this Type target, Type examplar, bool nullPassthrough = false)
        {

            if (nullPassthrough)
            {
                Type tnull = null;

                //
                //    ReSharper disable once ExpressionIsAlwaysNull
                //
                if (tnull.In(target, examplar))
                {
                    return false;
                }
            }

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (examplar == null)
                throw new ArgumentNullException(nameof(examplar));

            return examplar.IsAssignableFrom(target);
        }

        public static void ThrowUnless<T>(this Type target, string message = null)
        {
            target.ThrowUnless(typeof(T), message);
        }

        public static void ThrowUnless<T>(this object target, string message = null)
        {
            if (null == target)
                throw new IncorrectDataException("Did not expect null at this point.");

            target.GetType().ThrowUnless(typeof(T), message);
        }

        public static void ThrowUnless(this object target, Type examplar, string message = null)
        {
            if (null == target)
                throw new IncorrectDataException("Did not expect null at this point.");

            ThrowUnless(target.GetType(), examplar, message);
        }

        public static void ThrowUnless(this Type type, Type examplar, string message = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (examplar == null) throw new ArgumentNullException(nameof(examplar));

            message ??= $"Invalid type: {type.FullName}, expected {examplar.FullName}.";

            if (!Is(type, examplar)) throw new IncorrectTypeException(message);
        }

        /// <summary>
        /// Возвращает компактное строковое представление Guid.
        /// </summary>
        /// <param name="id">Guid</param>
        /// <returns>Компактная строка.</returns>
        /// <remarks>
        /// Компактное строковое представление не включает в себя незначащие нули.
        /// Например, для Guid {00000003-0003-0000-0000-000000000000} компактное представление "3-3-0-0-0".
        /// </remarks>
        public static string ToCompactString(this Guid id)
        {
            var shorts = new List<string>();
            var split = id.ToString().Split('-');

            foreach (var s0 in split)
            {
                string s = string.Empty;
                int i;

                for (i = 0; i < s0.Length - 1; i++)
                {
                    if (s0[i] != '0')
                        break;
                }

                for (; i < s0.Length; i++)
                {
                    s += s0[i];
                }

                shorts.Add(s);
            }

            return string.Join("-", shorts.ToArray());
        }

        /// <summary>
        /// Возвращает ссылку на функцию на основе метода.
        /// </summary>
        /// <typeparam name="T">Тип входных данных функции.</typeparam>
        /// <typeparam name="R">Тип выходных данных функции.</typeparam>
        /// <param name="action">Метод для выполнения.</param>
        /// <returns>Ссылка на функцию.</returns>
        public static Func<T, R> AsFunction<T, R>(this Action<T> action) where T:class
        {
            return new ActionWrapper<T>(action).AsFunction<R>;
        }

        /// <summary>
        /// Возвращает ссылку на метод обработки коллекции входных данных.
        /// </summary>
        /// <returns>Метод обработки коллекции входных данных.</returns>
        public static Action<T[]> AsProcessMany<T>(this Action<T> action)
        {
            return new ActionWrapper<T>(action).ProcessMany;
        }

        /// <summary>
        /// Запоминает ссылку на метод.
        /// </summary>
        /// <typeparam name="T">Тип входных данных для метода.</typeparam>
        private readonly struct ActionWrapper<T>
        {
            /// <summary>
            /// Ссылка на метод.
            /// </summary>
            public Action<T> Action => _action;

            /// <summary>Ссылка на метод.</summary>
            private readonly Action<T> _action;

            /// <param name="action">Ссылка на метод.</param>
            public ActionWrapper(Action<T> action)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }

            /// <summary>
            /// Выполняет метод и возвращает значение по умолчанию.
            /// </summary>
            /// <typeparam name="R">Тип выходных данных.</typeparam>
            /// <param name="data">Входные данные.</param>
            /// <returns>Значение по умолчанию для типа {R}.</returns>
            public R AsFunction<R>(T data)
            {
                _action(data);
                return default (R);
            }

            /// <summary>
            /// Обрабатывает коллекцию входных данных.
            /// </summary>
            /// <param name="data">Коллекция входных данных.</param>
            public void ProcessMany(IEnumerable<T> data)
            {
                if (data == null) throw new ArgumentNullException(nameof(data));

                foreach (T t in data)
                {
                    _action(t);
                }
            }
        }

        public static void WriteLine(this StringBuilder builder, string format, params object[] arguments)
        {
            var line = string.Format(format, arguments);
            builder.AppendLine(line);
        }
    }
}


