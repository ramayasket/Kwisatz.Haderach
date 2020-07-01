using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Kw.Common;
using PostSharp.Aspects;

namespace Kw.Aspects
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [LinesOfCodeAvoided(6)]
    public class TraceInOutAttribute : MethodInterceptionAspect
    {
        public override bool CompileTimeValidate(MethodBase method)
        {
            Type = method.DeclaringType;
            Token = method.Name;

            return base.CompileTimeValidate(method);
        }

        public string Token { get; private set; }
        public Type Type { get; private set; }

        public sealed override void OnInvoke(MethodInterceptionArgs args)
        {
            AppCore.WriteLine("@PX Entering method {0}::{1}", Type, Token);
            args.Proceed();
            AppCore.WriteLine("@PX Exited method {0}::{1}", Type, Token);
        }
    }
}

