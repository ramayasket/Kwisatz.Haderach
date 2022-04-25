using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Kw.Common.ZSpitz.Util;

namespace Kw.Common.ZSpitz
{
    internal static class NewExpressionExtensions {
        internal static IEnumerable<(string?, Expression, int)> NamesArguments(this NewExpression expr) =>
            expr.Constructor!.GetParameters().Zip(expr.Arguments, (x, y) => (x.Name, y)).WithIndex()!;
    }
}
