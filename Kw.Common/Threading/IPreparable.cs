using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kw.Common.Threading
{
	/// <summary>
	/// Интерфейс подготавливаемабельного объекта.
	/// </summary>
	public interface IPreparable
	{
		WaitHandle Ready { get; }
		Event ReadyEvent { get; }
	}
}

