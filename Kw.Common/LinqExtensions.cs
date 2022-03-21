#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Common
{
    /// <summary>
    /// Полезные методы расширения для LINQ
    /// </summary>
    public static class LinqExtensions
    {
        public static bool Empty<T>(this IEnumerable<T> collection)
        {
            return !collection.Any();
        }

        public static bool Empty<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            return !collection.Any(predicate);
        }

        public static bool ArrayEquals<T>(this IEnumerable<T> collection, params T[] that)
        {
            var source = collection.ToArray();

            if (that.Length == source.Length)
            {
                for (int i = 0; i < that.Length; i++)
                {
                    if (!source[i].Equals(that[i])) return false;
                }

                return true;
            }

            return false;
        }

        public static bool In<T>(this T t, IQueryable<T>? collection)
        {
            return collection?.Contains(t) ?? false;
        }

        public static bool In<T>(this T t, IEnumerable<T>? collection)
        {
            return collection?.Contains(t) ?? false;
        }

        public static bool In<T>(this T t, params T[]? collection)
        {
            return collection?.Contains(t) ?? false;
        }

        public static bool Out<T>(this T t, IQueryable<T>? collection)
        {
            return !collection?.Contains(t) ?? true;
        }

        public static bool Out<T>(this T t, IEnumerable<T>? collection)
        {
            return !collection?.Contains(t) ?? true;
        }

        public static bool Out<T>(this T t, params T[]? collection)
        {
            return !collection?.Contains(t) ?? true;
        }

        /// <summary>
        /// Проверяет, что все элементы коллекции all содержатся в коллекции collection.
        /// </summary>
        /// <typeparam name="T">Тип данных коллекций.</typeparam>
        /// <param name="all">Проверяемая коллекция.</param>
        /// <param name="collection">Целевая коллекция.</param>
        /// <returns></returns>
        public static bool AllIn<T>(this IEnumerable<T> all, params T[] collection)
        {
            return !all.Select(a => collection.Contains(a)).Contains(false);
        }

        /// <summary>
        /// Проверяет, что любой элемент коллекции all содержатся в коллекции collection.
        /// </summary>
        /// <typeparam name="T">Тип данных коллекций.</typeparam>
        /// <param name="all">Проверяемая коллекция.</param>
        /// <param name="collection">Целевая коллекция.</param>
        /// <returns></returns>
        public static bool AnyIn<T>(this IEnumerable<T> all, params T[] collection)
        {
            return all.Select(a => collection.Contains(a)).Contains(true);
        }

        /// <summary>
        /// Разбивает коллекцию на подколлекции указанного размера.
        /// </summary>
        /// <typeparam name="T">Тип элементов коллекции.</typeparam>
        /// <param name="source">Исходная коллекция.</param>
        /// <param name="portion">Размер подколлекции.</param>
        /// <returns>Коллекция подколлекций.</returns>
        public static T[][] Split<T>(this IEnumerable<T> source, int portion)
        {
            int i = 0;
            var portions = new List<T[]>();
            var sarray = source.ToArray();

            while (i * portion < sarray.Length)
            {
                portions.Add(sarray.Skip(i++ * portion).Take(portion).ToArray());
            }

            return portions.ToArray();
        }

        /// <summary>
        /// Concatenates single value and a collection.
        /// </summary>
        public static IEnumerable<T> Append<T>(this T first, IEnumerable<T> others)
        {
            return (new[] {first}).Concat(others);
        }
    }
}
