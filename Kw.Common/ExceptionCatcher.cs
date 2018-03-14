using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kw.Common.Containers;

namespace Kw.Common
{
	public class ExceptionCatcher : Pair<Type,Action<Exception>>
	{
		public Type ErrorType { get; set; }
		public Action<Exception> Handler { get; set; }

		public ExceptionCatcher(Type x, Action<Exception> handler)
		{
			if (x == null) throw new ArgumentNullException("x");
			if (handler == null) throw new ArgumentNullException("handler");

			if (!typeof(Exception).IsAssignableFrom(x)) throw new ArgumentException("Expected a type which is derived from System.Exception. The 'x' parameter isn't.", "x");

			ErrorType = x;
			Handler = handler;
		}
	}
}

