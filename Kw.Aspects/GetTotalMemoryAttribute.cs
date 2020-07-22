using Kw.Common;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;

namespace Kw.Aspects
{
    /// <summary>
    /// Cleans up memory upon executing attributed method.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [ProvideAspectRole(BasicRoles.Control)]
    public class GetTotalMemoryAttribute : MethodInterceptionAspect
    {
        public sealed override void OnInvoke(MethodInterceptionArgs args)
        {
            try
            {
                args.Proceed();
            }
            finally
            {
                GC.GetTotalMemory(true);
            }
        }
    }
}
