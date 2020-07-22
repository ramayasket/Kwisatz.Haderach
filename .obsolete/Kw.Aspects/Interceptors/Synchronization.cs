using System;
using System.Reflection;
using PostSharp.Aspects;

namespace Kw.Aspects.Interceptors
{
    public class Synchronization : Interceptor
    {
        /// <inheritdoc />
        public Synchronization(Interceptor next) : base(next) { }

        /// <inheritdoc />
        public override void Invoke(MethodInterceptionArgs args)
        {
            if (null != args.Instance)
                lock (args.Instance)
                    Next.Invoke(args);

            else
                Next.Invoke(args);
        }

        /// <inheritdoc />
        public override Interceptor Compile(MethodBase method)
        {
            if (method.IsStatic)
                throw new Exception("Synchronization only available to instance methods.");

            return base.Compile(method);
        }
    }
}