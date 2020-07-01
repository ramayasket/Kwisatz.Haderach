using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Common
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception x)
        {
            var parts = new List<string>();
            var current = x;

            while (null != current)
            {
                parts.Add(current.Message);
                current = current.InnerException;
            }

            return string.Join(" :: ", parts.ToArray());
        }

        /// <summary>
        /// Повторный выбос исключения с предохранением стека вызова.
        /// </summary>
        /// <param name="x">Объект исключения для повторного вызова.</param>
        public static void Rethrow(this Exception x)
        {
            throw new RethrowException(x);
        }

        /// <summary>
        /// Возвращает повторно-выброшенное исключение содержащееся в rethrow-исключении.
        /// </summary>
        /// <param name="x">Объект исключения.</param>
        /// <returns></returns>
        public static Exception UnwrapRethrown(this Exception x)
        {
            return RethrowException.IsValidRethrowException(x) ? x.InnerException : x;
        }
    }
}

