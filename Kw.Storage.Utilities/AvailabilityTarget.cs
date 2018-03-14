using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Storage.Utilities
{
	public abstract class AvailabilityTarget
	{
		public abstract string Type { get; }

		public override string ToString()
		{
			return Type;
		}
	}

	public class SqlAvailabilityTarget : AvailabilityTarget
	{
		public override string Type
		{
			get { return "SQL"; }
		}
	}
}

