using System.Linq.Expressions;

namespace Kw.Common.ZSpitz
{
    public static class LambdaExpressionExtensions {
        public static object? GetTarget(this LambdaExpression expr) => expr.Compile().Target;
    }
}
