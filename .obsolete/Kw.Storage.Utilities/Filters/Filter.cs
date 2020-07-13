using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kw.Common;

namespace Kw.Storage.Utilities.Filters
{
	public abstract class Filter
	{
		public string Column { get; private set; }
		public abstract string Expression { get; }

		protected Filter(string column)
		{
			Column = column;
		}

		protected string Express<T>(T value) where T:struct
		{
			if (value is bool)
			{
				var bv = (new object[] {value}).Cast<bool>().Single();
				return bv ? "1" : "0";
			}

			if (value is int)
			{
				return (new object[] {value}).Cast<int>().Single().InvariantString();
			}

			if (value is long)
			{
				return (new object[] { value }).Cast<long>().Single().InvariantString();
			}

			if (value is float)
			{
				return (new object[] { value }).Cast<float>().Single().InvariantString();
			}
		    if (value is double)
		    {
                return (new object[] { value }).Cast<double>().Single().InvariantString();
		    }

		    throw new NotImplementedException();
		}
	}
}

