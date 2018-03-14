using System;

namespace Kw.Networking.Communications
{
	[Serializable]
	public abstract class SerialExchangeRequest
	{
	}

	[Serializable]
	public class VoidRequest : SerialExchangeRequest
	{
	}

	[Serializable]
	public class SerialExchangeResponse
	{
		/// <summary>
		/// Инициализирует ответ с ошибкой
		/// </summary>
		public SerialExchangeResponse(Exception error)
		{
			Error = error;
		}

		public SerialExchangeResponse() { }

		/// <summary>
		/// Исключение при обработке запроса
		/// </summary>
		public Exception Error { get; private set; }
	}
}

