﻿using System.Collections.Generic;

namespace Kw.Common.ZSpitz.Util {
    public static class HashSetTExtensions {
        public static bool AddRemove<T>(this HashSet<T> src, bool add, T element) => 
            add ? 
                src.Add(element) : 
                src.Remove(element);
    }
}
