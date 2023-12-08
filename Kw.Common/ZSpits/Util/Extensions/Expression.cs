﻿using Kw.Common.OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Kw.Common.ZSpitz.Util;
using static System.Linq.Expressions.ExpressionType;

namespace Kw.Common.ZSpitz
{
    internal static class ExpressionExtensions {
        internal static void Deconstruct(this Expression expr, out ExpressionType nodeType, out Type type) =>
            (nodeType, type) = (expr.NodeType, expr.Type);

        static IEnumerable<(string path, Expression clause)> logicalCombinedClauses(string path, Expression expr, params ExpressionType[] nodeTypes) {
            // The return type cannot be IEnumerable<BinaryExpression> because it might contain any bool-returning expression
            if (expr.NodeType.Outside(nodeTypes) || expr.Type != typeof(bool)) {
                yield return (path, expr);
                yield break;
            }

            if (!path.IsNullOrEmpty()) { path += "."; }

            var bexpr = (BinaryExpression)expr;
            (string path, Expression expr)[] parts = new[] {
                (path + "Left", bexpr.Left),
                (path + "Right", bexpr.Right)
            };

            foreach (var (path1, expr1) in parts.SelectMany(x => logicalCombinedClauses(x.path, x.expr, nodeTypes))) {
                yield return (path1, expr1);
            }
        }

        internal static IEnumerable<Expression> AndClauses(this Expression expr) => logicalCombinedClauses("", expr, And, AndAlso).Select(x => x.clause);
        internal static IEnumerable<(string path, Expression clause)> OrClauses(this Expression expr) => logicalCombinedClauses("", expr, Or, OrElse);

        internal static IEnumerable<OneOf<MemberExpression, MethodCallExpression>> ChainClauses(this Expression? expr) {
            var (ret, subexpr) = expr switch {

                // instance member
                MemberExpression mexpr when mexpr.Expression is not null =>
                    (mexpr, mexpr.Expression),

                // instance method call
                MethodCallExpression callExpr when callExpr.Object is not null =>
                    (callExpr, callExpr.Object),

                // include instance method calls, or static extension method calls
                MethodCallExpression callExpr when
                        callExpr.Method.HasAttribute<ExtensionAttribute>() &&
                        callExpr.Arguments.FirstOrDefault() is not null =>
                    (callExpr, callExpr.Arguments.First()),

                _ => ((OneOf<MemberExpression, MethodCallExpression>?)null, (Expression?)null)
            };

            if (ret is null) { yield break; }

            foreach (var item in ChainClauses(subexpr)) {
                yield return item;
            }
            yield return ret.Value;
        } 
    }
}
