﻿using Kw.Common.OneOf;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Kw.Common.ZSpitz.Util;
using static System.Linq.Expressions.ExpressionType;
using static Kw.Common.ZSpitz.Globals;

namespace Kw.Common.ZSpitz
{
    public abstract class BuiltinsWriterVisitor : WriterVisitorBase {
        protected BuiltinsWriterVisitor(object o, OneOf<string, Language?> languageArg, IEnumerable<string>? insertionPointKeys, bool hasPathSpans) 
            : base(o, languageArg, insertionPointKeys, hasPathSpans) { }

        protected override void WriteNodeImpl(object? o, bool parameterDeclaration = false, object? metadata = null) {
            switch (o) {
                case ParameterExpression pexpr when parameterDeclaration:
                    WriteParameterDeclaration(pexpr);
                    break;
                case BlockExpression bexpr when metadata != null:
                    WriteBlock(bexpr, metadata);
                    break;
                case ConditionalExpression cexpr when metadata != null:
                    WriteConditional(cexpr, metadata);
                    break;
                case Expression expr:
                    writeExpression(expr);
                    break;
                case MemberBinding binding:
                    WriteBinding(binding);
                    break;
                case ElementInit init:
                    WriteElementInit(init);
                    break;
                case SwitchCase switchCase:
                    WriteSwitchCase(switchCase);
                    break;
                case CatchBlock catchBlock:
                    WriteCatchBlock(catchBlock);
                    break;
                case LabelTarget labelTarget:
                    WriteLabelTarget(labelTarget);
                    break;

                default:
                    WriteNotImplemented(
                        o is null ?
                            "Attempted code generation on null" :
                            $"Code generation not implemented for type '{o.GetType().Name}'"
                    );
                    break;
            }
        }

        void writeExpression(Expression expr) {
            switch (expr.NodeType) {

                case var nodeType when nodeType.Inside(BinaryExpressionTypes):
                    WriteBinary((BinaryExpression)expr);
                    break;

                case var nodeType when nodeType.Inside(UnaryExpressionTypes):
                    WriteUnary((UnaryExpression)expr);
                    break;

                case Lambda:
                    WriteLambda((LambdaExpression)expr);
                    break;

                case Parameter:
                    WriteParameter((ParameterExpression)expr);
                    break;

                case Constant:
                    WriteConstant((ConstantExpression)expr);
                    break;

                case MemberAccess:
                    WriteMemberAccess((MemberExpression)expr);
                    break;

                case New:
                    WriteNew((NewExpression)expr);
                    break;

                case Call:
                    WriteCall((MethodCallExpression)expr);
                    break;

                case MemberInit:
                    WriteMemberInit((MemberInitExpression)expr);
                    break;

                case ListInit:
                    WriteListInit((ListInitExpression)expr);
                    break;

                case NewArrayInit:
                case NewArrayBounds:
                    WriteNewArray((NewArrayExpression)expr);
                    break;

                case Conditional:
                    WriteConditional((ConditionalExpression)expr, null);
                    break;

                case Default:
                    WriteDefault((DefaultExpression)expr);
                    break;

                case TypeIs:
                case TypeEqual:
                    WriteTypeBinary((TypeBinaryExpression)expr);
                    break;

                case Invoke:
                    WriteInvocation((InvocationExpression)expr);
                    break;

                case ExpressionType.Index:
                    WriteIndex((IndexExpression)expr);
                    break;

                case Block:
                    WriteBlock((BlockExpression)expr, null);
                    break;

                case Switch:
                    WriteSwitch((SwitchExpression)expr);
                    break;

                case Try:
                    WriteTry((TryExpression)expr);
                    break;

                case Label:
                    WriteLabel((LabelExpression)expr);
                    break;

                case Goto:
                    WriteGoto((GotoExpression)expr);
                    break;

                case Loop:
                    WriteLoop((LoopExpression)expr);
                    break;

                case RuntimeVariables:
                    WriteRuntimeVariables((RuntimeVariablesExpression)expr);
                    break;

                case DebugInfo:
                    WriteDebugInfo((DebugInfoExpression)expr);
                    break;

                case Dynamic:
                    WriteDynamic((DynamicExpression)expr);
                    break;

                case Extension:
                    WriteExtension(expr);
                    break;

                default:
                    WriteNotImplemented($"NodeType: {expr.NodeType}, Expression object type: {expr.GetType().Name}");
                    break;
            }
        }

        protected void WriteNodeTypeNotImplemented(ExpressionType nodeType) => WriteNotImplemented($"No implementation for NodeType: {nodeType}");

        protected virtual void WriteDynamic(DynamicExpression expr) {
            switch (expr.Binder) {
                case BinaryOperationBinder binaryOperationBinder:
                    WriteBinaryOperationBinder(binaryOperationBinder, expr.Arguments);
                    break;
                case ConvertBinder convertBinder:
                    WriteConvertBinder(convertBinder, expr.Arguments);
                    break;
                case CreateInstanceBinder createInstanceBinder:
                    WriteCreateInstanceBinder(createInstanceBinder, expr.Arguments);
                    break;
                case DeleteIndexBinder deleteIndexBinder:
                    WriteDeleteIndexBinder(deleteIndexBinder, expr.Arguments);
                    break;
                case DeleteMemberBinder deleteMemberBinder:
                    WriteDeleteMemberBinder(deleteMemberBinder, expr.Arguments);
                    break;
                case GetIndexBinder getIndexBinder:
                    WriteGetIndexBinder(getIndexBinder, expr.Arguments);
                    break;
                case GetMemberBinder getMemberBinder:
                    WriteGetMemberBinder(getMemberBinder, expr.Arguments);
                    break;
                case InvokeBinder invokeBinder:
                    WriteInvokeBinder(invokeBinder, expr.Arguments);
                    break;
                case InvokeMemberBinder invokeMemberBinder:
                    WriteInvokeMemberBinder(invokeMemberBinder, expr.Arguments);
                    break;
                case SetIndexBinder setIndexBinder:
                    WriteSetIndexBinder(setIndexBinder, expr.Arguments);
                    break;
                case SetMemberBinder setMemberBinder:
                    WriteSetMemberBinder(setMemberBinder, expr.Arguments);
                    break;
                case UnaryOperationBinder unaryOperationBinder:
                    WriteUnaryOperationBinder(unaryOperationBinder, expr.Arguments);
                    break;

                default:
                    WriteNotImplemented($"Dynamic expression with binder type {expr.Binder} not implemented");
                    break;
            }
        }

        protected virtual void WriteExtension(Expression expr) => WriteNotImplemented("NodeType: Extension not implemented.");

        // .NET 3.5 expression types
        protected abstract void WriteBinary(BinaryExpression expr);
        protected abstract void WriteUnary(UnaryExpression expr);
        protected abstract void WriteLambda(LambdaExpression expr);
        protected abstract void WriteParameter(ParameterExpression expr);
        protected abstract void WriteConstant(ConstantExpression expr);
        protected abstract void WriteMemberAccess(MemberExpression expr);
        protected abstract void WriteNew(NewExpression expr);
        protected abstract void WriteCall(MethodCallExpression expr);
        protected abstract void WriteMemberInit(MemberInitExpression expr);
        protected abstract void WriteListInit(ListInitExpression expr);
        protected abstract void WriteNewArray(NewArrayExpression expr);
        protected abstract void WriteConditional(ConditionalExpression expr, object? metadata);
        protected abstract void WriteDefault(DefaultExpression expr);
        protected abstract void WriteTypeBinary(TypeBinaryExpression expr);
        protected abstract void WriteInvocation(InvocationExpression expr);
        protected abstract void WriteIndex(IndexExpression expr);

        // .NET 4 expression types
        protected abstract void WriteBlock(BlockExpression expr, object? metadata);
        protected abstract void WriteSwitch(SwitchExpression expr);
        protected abstract void WriteTry(TryExpression expr);
        protected abstract void WriteLabel(LabelExpression expr);
        protected abstract void WriteGoto(GotoExpression expr);
        protected abstract void WriteLoop(LoopExpression expr);
        protected abstract void WriteRuntimeVariables(RuntimeVariablesExpression expr);
        protected abstract void WriteDebugInfo(DebugInfoExpression expr);

        // other types
        protected abstract void WriteElementInit(ElementInit elementInit);
        protected abstract void WriteBinding(MemberBinding binding);
        protected abstract void WriteSwitchCase(SwitchCase switchCase);
        protected abstract void WriteCatchBlock(CatchBlock catchBlock);
        protected abstract void WriteLabelTarget(LabelTarget labelTarget);

        // binders
        protected virtual void WriteBinaryOperationBinder(BinaryOperationBinder binaryOperationBinder, IList<Expression> args) => throw new NotImplementedException();
        protected virtual void WriteConvertBinder(ConvertBinder convertBinder, IList<Expression> args) => throw new NotImplementedException();
        protected virtual void WriteCreateInstanceBinder(CreateInstanceBinder createInstanceBinder, IList<Expression> args) => throw new NotImplementedException();
        protected virtual void WriteDeleteIndexBinder(DeleteIndexBinder deleteIndexBinder, IList<Expression> args) => throw new NotImplementedException();
        protected virtual void WriteDeleteMemberBinder(DeleteMemberBinder deleteMemberBinder, IList<Expression> args) => throw new NotImplementedException();
        protected virtual void WriteGetIndexBinder(GetIndexBinder getIndexBinder, IList<Expression> args) => throw new NotImplementedException();
        protected virtual void WriteGetMemberBinder(GetMemberBinder getMemberBinder, IList<Expression> args) => throw new NotImplementedException();
        protected virtual void WriteInvokeBinder(InvokeBinder invokeBinder, IList<Expression> args) => throw new NotImplementedException();
        protected virtual void WriteInvokeMemberBinder(InvokeMemberBinder invokeMemberBinder, IList<Expression> args) => throw new NotImplementedException();
        protected virtual void WriteSetIndexBinder(SetIndexBinder setIndexBinder, IList<Expression> args) => throw new NotImplementedException();
        protected virtual void WriteSetMemberBinder(SetMemberBinder setMemberBinder, IList<Expression> args) => throw new NotImplementedException();
        protected virtual void WriteUnaryOperationBinder(UnaryOperationBinder unaryOperationBinder, IList<Expression> args) => throw new NotImplementedException();

        // parameter declarations
        protected abstract void WriteParameterDeclaration(ParameterExpression prm);
    }
}
