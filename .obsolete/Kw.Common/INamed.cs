using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Common
{
    /// <summary>
    /// Контракт чего-либо с именем.
    /// </summary>
    public interface INamed
    {
        string Name { get; }
    }
}

