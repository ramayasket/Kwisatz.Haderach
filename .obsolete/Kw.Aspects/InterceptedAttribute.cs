using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kw.Common;
using PostSharp.Aspects;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable CoVariantArrayConversion
namespace Kw.Aspects
{
    public abstract class Interceptor
    {
        public Interceptor Next { get; }
        public abstract void Invoke(MethodInterceptionArgs args);

        public virtual Interceptor Compile(MethodBase method) => Next;

        protected Interceptor(Interceptor next) => Next = next;
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [LinesOfCodeAvoided(13)]
    public class InterceptedAttribute : MethodInterceptionAspect
    {
        private class ProceedInterceptor : Interceptor
        {
            private readonly Action _proceed;

            /// <inheritdoc />
            public ProceedInterceptor(Action proceed) : base(null) => _proceed = proceed;

            /// <inheritdoc />
            public override void Invoke(MethodInterceptionArgs args) => _proceed();
        }

        private readonly Type[] _interceptors;

        public InterceptedAttribute(params Type[] interceptors) => _interceptors = interceptors;

        public Interceptor CreateInterceptors(Interceptor current)
        {
            foreach (var type in _interceptors.Reverse())
            {
                var ctor = type.GetConstructor(new[] { typeof(Interceptor) });
                current = (Interceptor)ctor.Invoke(new[] { current });
            }

            return current;
        }

        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            if (null != _interceptors)
                foreach (var type in _interceptors)
                    if(!type.Is<Interceptor>() || type.IsAbstract)
                        throw new ArgumentException($"Expected an array of concrete types inherited from Interceptor, but {type.Name} is not.");

            var current = CreateInterceptors(null);

            do current = current.Compile(method); while (null != current);

            base.CompileTimeInitialize(method, aspectInfo);
        }

        public sealed override void OnInvoke(MethodInterceptionArgs args) => CreateInterceptors(new ProceedInterceptor(args.Proceed)).Invoke(args);
    }
}
