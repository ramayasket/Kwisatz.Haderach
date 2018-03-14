using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kw.Common.Containers;
using Kw.Storage.Utilities;

namespace Kw.Storage.Utilities
{
	public interface IAvailabilityCheck
	{
		/// <summary>
		/// Какие сервера проверять. SQL и SPHINX
		/// </summary>
		AvailabilityTarget Target { get; }
		
		string SqlCoordinate { get; }
		Pair<string, int> SphinxCoordinate { get; }

		int WaitInterval { get; }
	}
}

