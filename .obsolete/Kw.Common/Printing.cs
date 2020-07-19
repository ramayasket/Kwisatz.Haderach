using System;

namespace Kw.Common
{
    [Flags]
    public enum Printing
    {
        Console = 1,
        File = 2,
        Debug = 4,
    }
}
