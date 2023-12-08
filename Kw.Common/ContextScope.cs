using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kw.Common
{
    /// <summary>
    /// Typed thread storage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ContextScope<T> : IDisposable where T:class
    {
        [ThreadStatic]
        static T _context;

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
