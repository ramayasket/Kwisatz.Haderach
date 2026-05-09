using System;
using System.Collections.Generic;
using System.Text;

namespace Kw.Common
{
    public static class CSharpUtils
    {
        public static string ToCodeType(this Type type)
        {
            Dictionary<Type, string> keymap = new()
            {
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(sbyte), "sbyte" },
                { typeof(char), "char" },
                { typeof(decimal), "decimal" },
                { typeof(double), "double" },
                { typeof(float), "float" },
                { typeof(int), "int" },
                { typeof(uint), "uint" },
                { typeof(long), "long" },
                { typeof(ulong), "ulong" },
                { typeof(object), "object" },
                { typeof(short), "short" },
                { typeof(ushort), "ushort" },
                { typeof(string), "string" }
            };

            return keymap[type];
        }
    }
}
