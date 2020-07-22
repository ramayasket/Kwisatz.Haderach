using Kw.Common;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kw.Aspects
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [ProvideAspectRole(BasicRoles.Tracing)]
    public class ExecutionTimingAttribute : MethodInterceptionAspect
    {
        private MethodBase _method;
        
        public override bool CompileTimeValidate(MethodBase method)
        {
            var result = base.CompileTimeValidate(method);

            _method = method;

            var name = method.Name;
            Type = method.DeclaringType;

            if (ExecutionTimings.ReportTimings)
            {
                Qizarate.Output?.WriteLine("CompileTimeValidate(): name = '{0}'", name);
            }

            Token = name;

            return result;
        }

        public string Token { get; private set; }
        public Type Type { get; private set; }

        public sealed override void OnInvoke(MethodInterceptionArgs args)
        {
            Action wrapped = args.Proceed;
            ExecutionTimings.MeasuredCall(wrapped, Token, _method);
        }
    }
}

