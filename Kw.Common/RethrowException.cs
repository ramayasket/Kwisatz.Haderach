using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Kw.Common
{
	/// <summary>
	/// Rethrow-исключение.
	/// </summary>
	/// <remarks>
	/// Rethrow-исключение используется там, где объект исключения возвращается как выходные данные,
	/// например, при выполнении метода Join() параллельного пула.
	/// Если выбрасывать такое исключение через throw, то информация о стеке вызова будет заменена на
	/// стек вызова строки throw.
	/// Для сохранения стека вызова объект исключения заворачивается как внутреннее исключение RethrowException.
	/// </remarks>
	[Serializable]
	public class RethrowException : Exception
	{
		internal const string RETHROW_MESSAGE = @"FY_COMMON_EXCEPTION_EXTENSIONS_RETHROW_EXCEPTION_MESSAGE";

		/// <summary>
		/// Инициализация экземпляра RethrowException из повторно-выбрасываемого исключения.
		/// </summary>
		/// <param name="wrap">Повторно-выбрасываемое исключение.</param>
		public RethrowException(Exception wrap) : base(RETHROW_MESSAGE, wrap)
		{
			if (wrap == null) throw new ArgumentNullException("wrap");
		}

		/// <summary>
		/// Проверяет объект исключения.
		/// </summary>
		/// <param name="x">Исключение для проверки.</param>
		/// <returns>True если объект <see cref="x"/> является RethrowException и содержит вложенное повторно-выброшенное исключение; иначе False.</returns>
		internal static bool IsValidRethrowException(Exception x)
		{
			return (null != x && typeof(RethrowException) == x.GetType() /* && RETHROW_MESSAGE == x.Message */ && null != x.InnerException);
		}

		protected RethrowException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}

