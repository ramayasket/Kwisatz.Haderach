using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Storage.Utilities.Filters
{
	public class InFilter<T> : Filter where T:struct
	{
		public T[] Data { get; set; }

		public InFilter(string column, IEnumerable<T> data) : base(column)
		{
			Data = data.ToArray();

			if(!Data.Any())
				throw new ArgumentException("Empty collection not allowed with InFilter.");
		}

		public override string Expression
		{
			get
			{
				if (Data.Any())
				{
					var printed = Data.Select(Express).ToArray();
					return string.Format("{0} in ({1})", Column, string.Join(",", printed));
				}

				return string.Empty;
			}
		}
	}
}

