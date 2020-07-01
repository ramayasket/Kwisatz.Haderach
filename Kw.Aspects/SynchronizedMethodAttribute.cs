using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Aspects;

namespace Kw.Aspects
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [LinesOfCodeAvoided(14)]
    public class SynchronizedMethodAttribute : MethodInterceptionAspect
    {
        public sealed override void OnInvoke(MethodInterceptionArgs args)
        {
            if (null != args.Instance)
            {
                lock (args.Instance)
                {
                    args.Proceed();
                }
            }
            else
            {
                args.Proceed();
            }
        }
    }
}
