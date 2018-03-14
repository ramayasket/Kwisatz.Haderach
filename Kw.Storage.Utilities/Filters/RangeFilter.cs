using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Storage.Utilities.Filters
{
	public class RangeFilter<T> : Filter where T:struct
	{
		public T? From { get; set; }
		public T? To { get; set; }

		public RangeFilter(string column, T? from = null, T? to = null) : base(column)
		{
			From = @from;
			To = to;
		}

		public override string Expression
		{
			get
			{
				string from = null, to = null;

				if (From.HasValue)
				{
					from = string.Format("({0} >= {1})", Column, Express(From.Value));
				}

				if (To.HasValue)
				{
					to = string.Format("({0} <= {1})", Column, Express(To.Value));
				}

				var parts = (new[] {from, to}).Where(p => null != p).ToArray();

				if (parts.Any())
				{
					return string.Join(" and ", parts);
				}

				return null;	//	query must ignore this
			}
		}
	}
}

