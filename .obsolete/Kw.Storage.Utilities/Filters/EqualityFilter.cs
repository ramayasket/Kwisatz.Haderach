using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Storage.Utilities.Filters
{
	public class EqualityFilter<T> : Filter where T:struct
	{
		public T Value { get; set; }

		public EqualityFilter(string column, T value) : base(column)
		{
			Value = value;
		}

		public override string Expression
		{
			get
			{
				return string.Format("({0} = {1})", Column, Express(Value));
			}
		}
	}
}

