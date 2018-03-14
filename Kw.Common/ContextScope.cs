using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kw.Common
{
	public class ContextScope<T> : IDisposable where T:class
	{
		[ThreadStatic]
		private static T _context;

		public ContextScope(T context)
		{
			_context = context;
		}

		public static T Context => _context;

		public void Dispose()
		{
			_context = null;
		}
	}
}
