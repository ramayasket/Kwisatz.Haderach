using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kw.Common.Threading
{
	/// <summary>
	/// Реализует прерываемые операции.
	/// </summary>
	public static class Interruptable
	{
		/// <summary>
		/// Результат выполнения действия
		/// </summary>
		public enum Signal
		{
			/// <summary>
			/// Истекло заданное время
			/// </summary>
			Elapsed = 0,
			/// <summary>
			/// Приложение было остановлено
			/// </summary>
			Shutdown,
			/// <summary>
			/// Приложение было приостановлено
			/// </summary>
			Pause,
			/// <summary>
			/// Сигнал, определяемый приложением.
			/// </summary>
			Application,
		}

		/// <summary>
		/// Ждет заданное время. Ожидание может быть прервано остановкой или приостановкой приложения.
		/// </summary>
		/// <param name="period">Время ожидания</param>
		/// <param name="sleep">Время паузы (атомарное время ожидания)</param>
		/// <param name="appSignal">Пользовательский callback для определяемого приложением сигнала.</param>
		/// <returns>Результат ожидания</returns>
		public static Signal Wait(long period, int sleep, Func<bool> appSignal = null)
		{
			for (int i = 0; i < period / sleep; i++)
			{
				if (AppCore.Exiting)
				{
					return Signal.Shutdown;
				}

				if (AppCore.Paused)
				{
					return Signal.Pause;
				}

				if (null != appSignal && appSignal())
				{
					return Signal.Application;
				}

				Thread.Sleep(sleep);
			}

			return Signal.Elapsed;
		}
	}
}

