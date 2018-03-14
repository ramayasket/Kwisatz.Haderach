using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Common
{
	public class Reinterpret<T>
	{
		public static A Cast<A>(T source)
		{
			var src = new[] { source };
			var cast = src.Cast<A>().Single();

			return cast;
		}
	}
}

