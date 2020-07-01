using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Common
{
    /// <summary>
    /// Атрибут, описывающий привязку
    /// </summary>
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property|AttributeTargets.Struct|AttributeTargets.Class, AllowMultiple = false)]
    public class MappingAttribute : Attribute
    {
    }
}

