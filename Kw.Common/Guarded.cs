using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kw.Common
{
	/// <summary>
	/// Executes code with no regard to exceptions
	/// </summary>
	/// ReSharper disable EmptyGeneralCatchClause
	/// ReSharper disable InconsistentNaming
	public static class Guarded
	{
		public static void Execute(this Action action)
		{
			try
			{
				action();
			}
			catch
			{
			}
		}

		public static void Execute<T>(this Action<T> action, T t)
		{
			try
			{
				action(t);
			}
			catch
			{
			}
		}

		public static R Execute<R>(this Func<R> action)
		{
			try
			{
				return action();
			}
			catch
			{
				return default(R);
			}
		}

		public static R Execute<T, R>(this Func<T, R> action, T t)
		{
			try
			{
				return action(t);
			}
			catch
			{
				return default(R);
			}
		}
	}
}
