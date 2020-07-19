using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Common
{
    public class MultiDisposable : IDisposable
    {
        public IDisposable[] Members { get; private set; }

        public MultiDisposable(params IDisposable[] members)
        {
            if(members == null) throw new ArgumentNullException("members");
            if(members.Any(m => null == m))
                throw new ArgumentException("No null members accepted.");

            Members = members;
        }

        public void Dispose()
        {
            foreach (var member in Members)
            {
                member.Dispose();
            }
        }
    }
}

