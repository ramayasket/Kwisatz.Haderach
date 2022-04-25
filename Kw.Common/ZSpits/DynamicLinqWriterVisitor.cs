﻿using Kw.Common.OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kw.Common.ZSpitz.Util;
using static System.Linq.Expressions.ExpressionType;
using static Kw.Common.ZSpitz.Util.Functions;
using static System.Linq.Enumerable;
using static Kw.Common.ZSpitz.Functions;

namespace Kw.Common.ZSpitz
{
    public class DynamicLinqWriterVisitor : BuiltinsWriterVisitor {
        public static readonly HashSet<Type> CustomAccessibleTypes = new();
        private static readonly HashSet<Type> predefinedTypes = new() {
            typeof(object),
            typeof(bool),
            typeof(char),
            typeof(string),
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Math),
            typeof(Convert),
            typeof(Uri)
        };

        private static bool isAccessibleType(Type t) =>
            t.IsNullable() ?
                isAccessibleType(t.UnderlyingSystemType) :
                t.Inside(predefinedTypes) || t.Inside(CustomAccessibleTypes);

        ParameterExpression? currentScoped;

        public DynamicLinqWriterVisitor(object o, OneOf<string, Language?> languageArg, bool hasPathSpans) :
            base(
                o,
                languageArg.ResolveLanguage() ?? throw new ArgumentException("Invalid language"),
                new[] { "parameters", "" },
                hasPathSpans
            ) { }

        private static readonly Dictionary<ExpressionType, string> simpleBinaryOperators = new() {
            [Add] = "+",
            [AddChecked] = "+",
            [Divide] = "/",
            [Modulo] = "%",
            [Multiply] = "*",
            [MultiplyChecked] = "*",
            [Subtract] = "-",
            [SubtractChecked] = "-",
            [AndAlso] = "&&",
            [OrElse] = "||",
            [Equal] = "==",
            [NotEqual] = "!=",
            [GreaterThanOrEqual] = ">=",
            [GreaterThan] = ">",
            [LessThan] = "<",
            [LessThanOrEqual] = "<=",
            [Coalesce] = "??",
        };

        // can be verified against https://dynamic-linq.net/expression-language#operators using:
        // precedence.GroupBy(kvp => kvp.Value, kvp => kvp.Key, (key, grp) => new {key, values = grp.OrderBy(x => x.ToString()).Joined(", ")}).OrderBy(x => x.key);
        private static readonly Dictionary<ExpressionType, int?> precedence = new() {
            [Add] = 3,
            [AddChecked] = 3,
            [And] = 5,
            [AndAlso] = 5,
            [ArrayIndex] = 0,
            [ArrayLength] = 0,
            [Block] = null,
            [Call] = 0,
            [Coalesce] = 7,
            [Conditional] = 0,
            [Constant] = -1,
            [ExpressionType.Convert] = 0,
            [ConvertChecked] = 0,
            [DebugInfo] = null,
            [Decrement] = null,
            [Default] = null,
            [Divide] = 2,
            [Dynamic] = null,
            [Equal] = 4,
            [ExclusiveOr] = null,
            [Extension] = null,
            [Goto] = null,
            [GreaterThan] = 4,
            [GreaterThanOrEqual] = 4,
            [Increment] = null,
            [ExpressionType.Index] = 0,
            [Invoke] = null,
            [IsFalse] = null,
            [IsTrue] = null,
            [Label] = null,
            [Lambda] = null,
            [LeftShift] = null,
            [LessThan] = 4,
            [LessThanOrEqual] = 4,
            [ListInit] = null,
            [Loop] = null,
            [MemberAccess] = 0,
            [MemberInit] = null,
            [Modulo] = 2,
            [Multiply] = 2,
            [MultiplyChecked] = 2,
            [Negate] = 1,
            [NegateChecked] = 1,
            [New] = 0,
            [NewArrayBounds] = null,
            [NewArrayInit] = null,
            [Not] = 1,
            [NotEqual] = 4,
            [OnesComplement] = null,
            [Or] = 6,
            [OrElse] = 6,
            [Parameter] = -1,
            [Power] = null,
            [Quote] = null,
            [RightShift] = null,
            [RuntimeVariables] = null,
            [Subtract] = 3,
            [SubtractChecked] = 3,
            [Switch] = null,
            [Throw] = null,
            [Try] = null,
            [TypeAs] = 0,
            [TypeEqual] = null,
            [TypeIs] = 0,
            [UnaryPlus] = null,
            [Unbox] = 0
        };

        private static int getPrecedence(Expression node) {
            var (nodeType, _) = node;
            return nodeType switch {
                Call when node is MethodCallExpression mcexpr && mcexpr.Method.IsStringConcat() => 3,
                And or AndAlso when node.Type == typeof(bool) => 5,
                _ => precedence[node.NodeType] ?? -1
            };
        }

        private void parens(OneOf<Expression, int> outer, string path, Expression inner) {
            var precedence = (
                outer: outer.Match(
                    expr => getPrecedence(expr),
                    i => i
                ),
                inner: getPrecedence(inner)
            );

            var writeParens = precedence.inner > precedence.outer;
            if (writeParens) { Write("("); }
            WriteNode(path, inner);
            if (writeParens) { Write(")"); }
        }

        private bool isEquivalent(Expression? x, Expression? y) {
            if (x is null) { return y is null; }
            if (y is null) { return x is null; }
            var (x1, y1) = (x.SansConvert(), y.SansConvert());
            return
                x1 is MemberExpression mexpr1 && y1 is MemberExpression mexpr2 ?
                    mexpr1.Member == mexpr2.Member &&
                    isEquivalent(mexpr1.Expression, mexpr2.Expression) :
                x1 is MethodCallExpression call1 && y1 is MethodCallExpression call2 ?
                    call1.Method == call2.Method &&
                    call1.Arguments.Count == call2.Arguments.Count &&
                    call1.Arguments.ZipT(call2.Arguments).All(x => isEquivalent(x.Item1, x.Item2)) :
                x1 is ConstantExpression cexpr1 && y1 is ConstantExpression cexpr2 ?
                    Equals(cexpr1.Value, cexpr2.Value) :
                x1 == y1;
        }

        protected override void WriteBinary(BinaryExpression expr) {
            var values = new List<Expression>();
            var grouped = expr.OrClauses().ToLookup(orClause => {
                if (!(orClause.clause is BinaryExpression bexpr && bexpr.NodeType == Equal)) { return null; }
                var matched = values.FirstOrDefault(x => isEquivalent(x, bexpr.Left));
                if (matched is null) {
                    matched = bexpr.Left;
                    values.Add(matched);
                }
                return matched;
            });

            (Expression left, string leftPath, Expression right, string rightPath) parts;

            if (grouped.Any(grp => grp.Key is { } && grp.Count() > 1)) { // can any elements be written using `in`?
                var firstClause = true;
                foreach (var grp in grouped) {
                    if (firstClause) {
                        firstClause = false;
                    } else {
                        Write(" || ");
                    }

                    if (grp.Key is null || grp.Count() == 1) {
                        // if only one element in the group, there's no need for a foreach
                        // but the rest of the logic is shared
                        foreach (var x in grp) {
                            WriteNode(x);
                        }
                        continue;
                    }

                    // write value
                    var firstElement = true;
                    foreach (var (path, clause) in grp) {
                        var bexpr = (BinaryExpression)clause;
                        var (left, leftPath, right, rightPath) = (
                            bexpr.Left,
                            "Left",
                            bexpr.Right,
                            "Right"
                        );
                        if (
                            TryGetEnumComparison(clause, out parts) ||
                            TryGetCharComparison(clause, out parts)
                        ) {
                            (left, leftPath, right, rightPath) = parts;
                        }

                        if (firstElement) {
                            parens(0, $"{path}.{leftPath}", left);
                            Write(" in (");
                            firstElement = false;
                        } else {
                            Write(", ");
                        }
                        WriteNode($"{path}.{rightPath}", right);
                    }
                    Write(")");
                }
                return;
            }

            if (
                TryGetEnumComparison(expr, out parts) ||
                TryGetCharComparison(expr, out parts)
            ) {
                var (left, leftPath, right, rightPath) = parts;
                parens(expr, leftPath, left);
                Write($" {simpleBinaryOperators[expr.NodeType]} ");
                parens(expr, rightPath, right);
                return;
            }

            simpleBinaryOperators.TryGetValue(expr.NodeType, out var @operator);
            @operator ??= expr.NodeType switch {
                And => "&&",
                Or => "||",
                _ => null
            };

            if (@operator is not null) {
                parens(expr, "Left", expr.Left);
                Write($" {@operator} ");
                parens(expr, "Right", expr.Right);
                return;
            }

            switch (expr.NodeType) {
                case ArrayIndex:
                    parens(expr, "Left", expr.Left);
                    Write("[");
                    WriteNode("Right", expr.Right);
                    Write("]");
                    return;
                case Power:
                    Write("Math.Pow(");
                    WriteNode("Left", expr.Left);
                    Write(", ");
                    WriteNode("Right", expr.Right);
                    Write(")");
                    return;
            }

            WriteNodeTypeNotImplemented(expr.NodeType);
        }

        private string escapedDoubleQuotes => language == Language.VisualBasic ? "\"\"" : "\\\"";

        protected override void WriteUnary(UnaryExpression expr) {
            switch (expr.NodeType) {
                case ExpressionType.Convert:
                case ConvertChecked:
                case Unbox:
                    var renderConversion =
                        expr.Type != expr.Operand.Type &&
                        !expr.Operand.Type.HasImplicitConversionTo(expr.Type);
                    if (renderConversion) { Write($"{typeName(expr.Type)}("); }
                    WriteNode("Operand", expr.Operand);
                    if (renderConversion) { Write(")"); }
                    return;
                case Not:
                case Negate:
                case NegateChecked:
                    if (expr.Type.UnderlyingIfNullable() == typeof(bool)) {
                        Write("!");
                    } else {
                        Write("-");
                    }
                    parens(expr, "Operand", expr.Operand);
                    return;
                case TypeAs:
                    // TODO under what circumstances can we elide the cast?
                    // We elide the cast in the code visitors, when the base instance is assignable to the cast target
                    // Is that safe?
                    Write("as(");
                    if (expr.Operand != currentScoped) {
                        WriteNode("Operand", expr.Operand);
                        Write(", ");
                    }
                    Write($"{escapedDoubleQuotes}{expr.Type.FullName}{escapedDoubleQuotes})");
                    return;
                case Quote:
                    WriteNode("Operand", expr.Operand);
                    return;
                case ArrayLength:
                    parens(expr, "Operand", expr.Operand);
                    Write(".Length");
                    return;
            }

            WriteNodeTypeNotImplemented(expr.NodeType);
        }

        private bool insideLambda = false;
        protected override void WriteLambda(LambdaExpression expr) {
            var exitLambda = false;
            if (!insideLambda) {
                Write("\"");
                insideLambda = true;
                exitLambda = true;
            }

            var count = expr.Parameters.Count;
            if (count > 1) {
                WriteNotImplemented("Multiple parameters in lambda expression.");
                return;
            } else if (count == 1) {
                currentScoped = expr.Parameters[0];
            }
            WriteNode("Body", expr.Body);

            if (exitLambda) {
                Write("\"");
                insideLambda = false;
            }
        }

        protected override void WriteParameter(ParameterExpression expr) {
            if (currentScoped is { } && expr != currentScoped) {
                WriteNotImplemented("Multiple ParameterExpression in current scope.");
                return;
            }

            // if we got here, that means we need to write out the ParameterExpression
            Write("it");
        }

        private Dictionary<object, int>? parameterIds;
        private int getParaneterId(object o, out bool isNew) => GetId(o, ref parameterIds, out isNew);

        protected override void WriteConstant(ConstantExpression expr) {
            var value = expr.Value;
            if (!insideLambda) {
                Write(RenderLiteral(value, language));
                return;
            }

            var underlying = value?.GetType().UnderlyingIfNullable() ?? typeof(void);
            var literal = RenderLiteral(value, "C#");
            if (
                value is null ||
                value is bool ||
                underlying.IsEnum ||
                underlying.IsNumeric()
            ) {
                Write(literal);
                return;
            }

            if (
                value is string ||
                value is char
            ) {
                Write(RenderLiteral(literal, language)[1..^1]);
                return;
            }

            if (value is DateTime dte) {
                // we need to supply both parameters to the DateTime constructor;
                // otherwise Dynamic LINQ interperts it as a conversion, and fails
                Write($"DateTime({dte.Ticks}, DateTimeKind.{dte.Kind})");
                return;
            }

            // TODO handle DateTimeOffset and TimeSpan

            writeDynamicLinqParameter(value, () => {
                var (isLiteral, repr) = TryRenderLiteral(value, language);
                if (!isLiteral) {
                    var sv = StringValue(value, language);
                    if (sv != repr) {
                        repr = $"{repr} {{ {sv} }}";
                    }
                }
                return repr;
            });
        }

        protected override void WriteMemberAccess(MemberExpression expr) {
            if (expr.Expression is { } && expr.Expression.Type.IsClosureClass() && expr.Expression is ConstantExpression cexpr) {
                writeDynamicLinqParameter((cexpr.Value, expr.Member), () => expr.Member.Name);
                return;
            }

            writeMemberUse("Expression", expr.Expression, expr.Member);
        }

        private void writeDynamicLinqParameter(object key, Func<string> value) {
            var id = getParaneterId(key, out var isNew);
            if (isNew) {
                SetInsertionPoint("parameters");
                if (language == Language.VisualBasic) {
                    Write("' ");
                } else {
                    Write("// ");
                }
                Write($"@{id} = {value()}");
                WriteEOL();
                SetInsertionPoint("");
            }
            Write($"@{id}");
        }

        protected override void WriteNew(NewExpression expr) {
            if (expr.Type.IsAnonymous()) {
                Write("new(");
                foreach (var (name, arg, index) in expr.NamesArguments()) {
                    if (index > 0) { Write(", "); }

                    // if the expression being assigned from is a property access with the same name as the target property, 
                    // write only the target expression.
                    // Otherwise, write `property = expression`
                    if (!(arg is MemberExpression mexpr && mexpr.Member.Name.Replace("$VB$Local_", "") == name)) {
                        Write($"{name} = ");
                    }
                    WriteNode($"Arguments[{index}]", arg);
                }
                Write(")");
                return;
            }

            Write(typeName(expr.Type));
            Write("(");
            WriteNodes("Arguments", expr.Arguments);
            Write(")");
        }

        private void writeIndexerAccess(string instancePath, Expression instance, string argumentsPath, IEnumerable<Expression> arguments) {
            var lst = arguments.ToList();
            if (instance.Type.IsArray && lst.Count > 1) {
                WriteNotImplemented("Multidimensional array access not supported.");
                return;
            }
            // No such thing as a static indexer
            parens(0, instancePath, instance);
            Write("[");
            WriteNodes(argumentsPath, lst);
            Write("]");
        }

        private void writeMemberUse(string instancePath, Expression? instance, MemberInfo mi) {
            var declaringType = mi.DeclaringType!;
            if (instance is null) {
                if (!isAccessibleType(declaringType)) {
                    WriteNotImplemented($"Type '{declaringType.Name}' is not an accessible type; its' static methods cannot be used.");
                    return;
                }
                Write(typeName(declaringType) + ".");
            } else {
                if (instance.Type.IsClosureClass()) {
                    WriteNotImplemented("No representation for closed-over variables.");
                    return;
                } else if (
                    mi is MethodInfo mthd &&
                    !isAccessibleType(declaringType) &&
                    !isAccessibleType(mthd.ReturnType) &&
                    insideLambda
                ) {
                    WriteNotImplemented($"Within a quoted lambda, {(mthd.IsStatic ? "extension" : "instance")} methods must either be on an accessible type, or return an instance of an accessible type.");
                    return;
                } else if (instance.SansConvert() != currentScoped) {
                    parens(0, instancePath, instance);
                    Write(".");
                }
            }
            Write(mi.Name);
        }

        private static readonly MethodInfo[] containsMethods = IIFE(() => {
            IEnumerable<char> e = "";
            var q = "".AsQueryable();

            return new[] {
                GetMethod(() => e.Contains('a')).GetGenericMethodDefinition(),
                GetMethod(() => q.Contains('a')).GetGenericMethodDefinition()
            };
        });

        private static readonly MethodInfo[] sequenceMethods = IIFE(() => {
            IEnumerable<char> e = "";
            var ordered = e.OrderBy(x => x);

            var q = "".AsQueryable();
            var orderedQ = q.OrderBy(x => x);

            // TODO what about Max/Min/Sum without predicate?

            return new[] {
                #region Enumerable
                GetMethod(() => e.All(x => true)),
                GetMethod(() => e.Any()),
                GetMethod(() => e.Any(x => true)),
                GetMethod(() => e.Average(x => x)),
                GetMethod(() => e.Cast<char>()),
                GetMethod(() => e.Contains('a')),
                GetMethod(() => e.Count()),
                GetMethod(() => e.Count(x => true)),
                GetMethod(() => e.DefaultIfEmpty()),
                GetMethod(() => e.DefaultIfEmpty('a')),
                GetMethod(() => e.Distinct()),
                GetMethod(() => e.First()),
                GetMethod(() => e.First(x => true)),
                GetMethod(() => e.FirstOrDefault()),
                GetMethod(() => e.FirstOrDefault(x => true)),
                GetMethod(() => e.GroupBy(x => x)),
                GetMethod(() => e.GroupBy(x => x, x => x)),
                GetMethod(() => e.Last()),
                GetMethod(() => e.Last(x => true)),
                GetMethod(() => e.LongCount()),
                GetMethod(() => e.LongCount(x => true)),
                GetMethod(() => e.Max(x => true)),
                GetMethod(() => e.Min(x => true)),
                GetMethod(() => e.OfType<char>()),
                GetMethod(() => e.OrderBy(x => x)),
                GetMethod(() => e.OrderByDescending(x=>x)),
                GetMethod(() => e.Select(x => x)),
                GetMethod(() => e.SelectMany(x => Range(1,1))),
                GetMethod(() => e.Single()),
                GetMethod(() => e.Single(x => true)),
                GetMethod(() => e.SingleOrDefault()),
                GetMethod(() => e.SingleOrDefault(x => true)),
                GetMethod(() => e.Skip(1)),
                GetMethod(() => e.SkipWhile(x => true)),
                GetMethod(() => e.Sum(x => x)),
                GetMethod(() => e.Take(1)),
                GetMethod(() => e.TakeWhile(x => true)),
                GetMethod(() => ordered.ThenBy(x => x)),
                GetMethod(() => ordered.ThenByDescending(x => x)),
                GetMethod(() => e.Where(x => true)),
                #endregion

                #region Queryable
                GetMethod(() => q.All(x => true)),
                GetMethod(() => q.Any()),
                GetMethod(() => q.Any(x => true)),
                GetMethod(() => q.Average(x => x)),
                GetMethod(() => q.Cast<char>()),
                GetMethod(() => q.Contains('a')),
                GetMethod(() => q.Count()),
                GetMethod(() => q.Count(x => true)),
                GetMethod(() => q.DefaultIfEmpty()),
                GetMethod(() => q.DefaultIfEmpty('a')),
                GetMethod(() => q.Distinct()),
                GetMethod(() => q.First()),
                GetMethod(() => q.First(x => true)),
                GetMethod(() => q.FirstOrDefault()),
                GetMethod(() => q.FirstOrDefault(x => true)),
                GetMethod(() => q.GroupBy(x => x)),
                GetMethod(() => q.GroupBy(x => x, x => x)),
                GetMethod(() => q.Last()),
                GetMethod(() => q.Last(x => true)),
                GetMethod(() => q.LongCount()),
                GetMethod(() => q.LongCount(x => true)),
                GetMethod(() => q.Max(x => true)),
                GetMethod(() => q.Min(x => true)),
                GetMethod(() => q.OfType<char>()),
                GetMethod(() => q.OrderBy(x => x)),
                GetMethod(() => q.OrderByDescending(x=>x)),
                GetMethod(() => q.Select(x => x)),
                GetMethod(() => q.SelectMany(x => Range(1,1))),
                GetMethod(() => q.Single()),
                GetMethod(() => q.Single(x => true)),
                GetMethod(() => q.SingleOrDefault()),
                GetMethod(() => q.SingleOrDefault(x => true)),
                GetMethod(() => q.Skip(1)),
                GetMethod(() => q.SkipWhile(x => true)),
                GetMethod(() => q.Sum(x => x)),
                GetMethod(() => q.Take(1)),
                GetMethod(() => q.TakeWhile(x => true)),
                GetMethod(() => orderedQ.ThenBy(x => x)),
                GetMethod(() => orderedQ.ThenByDescending(x => x)),
                GetMethod(() => q.Where(x => true))
                #endregion
            }.Select(x => x.GetGenericMethodDefinition()).ToArray();
        });

        protected override void WriteCall(MethodCallExpression expr) {
            if (expr.Method.IsStringConcat()) {
                var firstArg = expr.Arguments[0];
                IEnumerable<Expression>? argsToWrite = null;
                var argsPath = "";
                if (firstArg is NewArrayExpression newArray && firstArg.NodeType == NewArrayInit) {
                    argsToWrite = newArray.Expressions;
                    argsPath = "Arguments[0].Expressions";
                } else if (expr.Arguments.All(x => x.Type == typeof(string))) {
                    argsToWrite = expr.Arguments;
                    argsPath = "Arguments";
                }
                if (argsToWrite != null) {
                    // TODO insert parentheses for arguments that have lower precedence than string concatenation
                    WriteNodes(argsPath, argsToWrite, " + ");
                    return;
                }
            }

            if (expr.Method.IsGenericMethod && expr.Method.GetGenericMethodDefinition().Inside(containsMethods)) {
                parens(expr, "Arguments[1]", expr.Arguments[1]);
                Write(" in ");
                parens(expr, "Arguments[0]", expr.Arguments[0]);
                return;
            }

            var instance = expr.Object;
            var isIndexer =
                instance is { } && instance.Type.IsArray && expr.Method.Name == "Get" ||
                expr.Method.IsIndexerMethod();
            if (isIndexer) {
                writeIndexerAccess("Object", expr.Object!, "Arguments", expr.Arguments);
                return;
            }

            var path = "Object";
            var skip = 0;

            if (expr.Method.IsGenericMethod && expr.Method.GetGenericMethodDefinition().Inside(sequenceMethods)) {
                // they're all static extension methods on IEnumerable<T> / IQueryable<T>, so no further tests required
                path = "Arguments[0]";
                instance = expr.Arguments[0];
                skip = 1;
            }

            var arguments = expr.Arguments.Skip(skip).Select((x, index) => ($"Arguments[{index + skip}]", x));

            writeMemberUse(path, instance, expr.Method);
            Write("(");
            WriteNodes(arguments);
            Write(")");
        }

        protected override void WriteMemberInit(MemberInitExpression expr) =>
            throw new NotImplementedException();

        protected override void WriteListInit(ListInitExpression expr) =>
            throw new NotImplementedException();

        protected override void WriteNewArray(NewArrayExpression expr) =>
            throw new NotImplementedException();

        private bool isMemberChainEqual(Expression? x, Expression? y) =>
            x is null ? y is null :
            y is null ? x is null :
            x.SansConvert() is MemberExpression mexpr1 && y.SansConvert() is MemberExpression mexpr2 ?
                mexpr1.Member == mexpr2.Member && isMemberChainEqual(mexpr1.Expression, mexpr2.Expression) :
                x == y;

        private bool doesTestMatchChain(Expression valueClause, Expression? testClause) {
            if (
                testClause is not BinaryExpression bexpr ||
                bexpr.NodeType != NotEqual
            ) { return false; }

            var (_, testExpression) = (bexpr.Left, bexpr.Right) switch {
                (ConstantExpression x, Expression y) when x.Value is null => (x, y),
                (Expression y, ConstantExpression x) when x.Value is null => (x, y),
                _ => (null, null)
            };
            return testExpression is not null && isEquivalent(valueClause, testExpression);
        }

        protected override void WriteConditional(ConditionalExpression expr, object? metadata) {
            if (expr.Type == typeof(void)) {
                WriteNotImplemented("Cannot represent void-returning conditionals.");
                return;
            };

            // TODO handle !(x.A == null) as well
            // TODO handle also !(x == null || x.A == null)

            // only check member expressions whose Expression.Type can take null (reference type or Nullable<T>)
            var chainClauses = expr.IfTrue.ChainClauses().Select(x =>
                x.Match(
                    mexpr => mexpr.Expression,
                    callExpr => callExpr.Object ?? callExpr.Arguments.First()
                )!
            ).Where(x => x.Type.IsNullable(true)).ToList();

            var ifFalseIsNull = expr.IfFalse is ConstantExpression cexpr && cexpr.Value is null;
            if (!ifFalseIsNull && expr.IfTrue.Type.IsNullable(true)) {
                chainClauses.Add(expr.IfTrue);
            }

            // we assume there are no test clauses for items in the member chain whose return value cannot be null
            var andClauses = expr.Test.AndClauses().ToList();
            if (
                chainClauses.Count > 0 &&
                chainClauses.Count == andClauses.Count &&
                chainClauses.ZipT(andClauses).All(x => doesTestMatchChain(x.Item1, x.Item2))
            ) {
                Write("np(");
                WriteNode("IfTrue", expr.IfTrue);
                if (!ifFalseIsNull) {
                    Write(", ");
                    WriteNode("IfFalse", expr.IfFalse);
                }
                Write(")");
                return;
            }

            Write("iif(");
            WriteNode("Test", expr.Test);
            Write(", ");
            WriteNode("IfTrue", expr.IfTrue);
            Write(", ");
            WriteNode("IfFalse", expr.IfFalse);
            Write(")");
        }

        protected override void WriteDefault(DefaultExpression expr) =>
            throw new NotImplementedException();

        protected override void WriteTypeBinary(TypeBinaryExpression expr) {
            Write("is(");
            if (expr.Expression != currentScoped) {
                WriteNode("Expression", expr.Expression);
                Write(", ");
            }
            Write($"{escapedDoubleQuotes}{expr.TypeOperand.FullName}{escapedDoubleQuotes})");
        }

        protected override void WriteInvocation(InvocationExpression expr) {
            WriteNotImplemented("Pending https://github.com/zzzprojects/System.Linq.Dynamic.Core/issues/441");
            //Write("(");
            //WriteNode("Expression", expr.Expression);
            //Write(")(");
            //WriteNodes("Arguments", expr.Arguments);
            //Write(")");
        }

        protected override void WriteIndex(IndexExpression expr) =>
            writeIndexerAccess("Object", expr.Object!, "Arguments", expr.Arguments);

        protected override void WriteBlock(BlockExpression expr, object? metadata) =>
            throw new NotImplementedException();

        protected override void WriteSwitch(SwitchExpression expr) =>
            throw new NotImplementedException();

        protected override void WriteTry(TryExpression expr) =>
            throw new NotImplementedException();

        protected override void WriteLabel(LabelExpression expr) =>
            throw new NotImplementedException();

        protected override void WriteGoto(GotoExpression expr) =>
            throw new NotImplementedException();

        protected override void WriteLoop(LoopExpression expr) =>
            throw new NotImplementedException();

        protected override void WriteRuntimeVariables(RuntimeVariablesExpression expr) =>
            throw new NotImplementedException();

        protected override void WriteDebugInfo(DebugInfoExpression expr) =>
            throw new NotImplementedException();

        protected override void WriteElementInit(ElementInit elementInit) =>
            throw new NotImplementedException();

        protected override void WriteBinding(MemberBinding binding) =>
            throw new NotImplementedException();

        protected override void WriteSwitchCase(SwitchCase switchCase) =>
            throw new NotImplementedException();

        protected override void WriteCatchBlock(CatchBlock catchBlock) =>
            throw new NotImplementedException();

        protected override void WriteLabelTarget(LabelTarget labelTarget) =>
            throw new NotImplementedException();

        protected override void WriteParameterDeclaration(ParameterExpression prm) =>
            throw new NotImplementedException();

        private static readonly Dictionary<Type, string> typeAliases = new() {
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(bool), "bool" },
            { typeof(float), "float" },
        };

        private static string typeName(Type t) =>
            t.IsNullable() ?
                typeName(t.UnderlyingIfNullable()) + "?" :
                typeAliases.TryGetValue(t, out var name) ?
                    name :
                    t.Name;
    }
}
