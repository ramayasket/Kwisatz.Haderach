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
    public class TimingAttribute : MethodInterceptionAspect
    {
        private MethodBase _method;
        private readonly string _name;

        public TimingAttribute(string name = null) => _name = name;
        
        public override bool CompileTimeValidate(MethodBase method)
        {
            _method = method;

            Token = _name ?? _method.Name;
            Type = _method.DeclaringType;

            return base.CompileTimeValidate(method);
        }

        public string Token { get; private set; }
        public Type Type { get; private set; }

        public sealed override void OnInvoke(MethodInterceptionArgs args)
        {
            Action wrapped = args.Proceed;
            Timings.MeasuredCall(wrapped, Token, _method);
        }
    }
}

