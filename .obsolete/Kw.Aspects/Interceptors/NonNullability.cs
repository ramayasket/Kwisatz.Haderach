using System;
using System.Linq;
using System.Reflection;
using Kw.Common;
using PostSharp.Aspects;

namespace Kw.Aspects.Interceptors
{
    public class NonNullability : Interceptor
    {
        /// <inheritdoc />
        public NonNullability(Interceptor next) : base(next) { }

        /// <inheritdoc />
        public override void Invoke(MethodInterceptionArgs args)
        {
            var arguments = args.Method.QuerySingleAttribute<NonNullableAttribute>(true)?.Arguments ?? new string[0];
            var names = args.Method.GetParameters().Select(p => p.Name).ToArray();

            object argument(int i) => args.Arguments[i];

            for (var i = 0; i < args.Arguments.Count; i++)
            {
                if (!arguments.Any() || arguments.Contains(names[i]))
                {
                    // scalar check
                    if (null == argument(i))
                        throw new ArgumentNullException(names[i]);

                    // vector (array) check
                    if (argument(i) is Array items)
                    {
                        for (var m = 0; m < items.Length; m++)
                        {
                            if (null == items.GetValue(m))
                                throw new ArgumentNullException($"{names[i]}[{m}]");
                        }
                    }
                }
            }

            Next.Invoke(args);
        }

        /// <inheritdoc />
        public override Interceptor Compile(MethodBase method)
        {
            var att = method.QuerySingleAttribute<NonNullableAttribute>(true);

            if (null != att)
            {
                var names = method.GetParameters().Select(p => p.Name).ToArray();

                foreach (var argument in att.Arguments)
                    if (!names.Contains(argument))
                        throw new ArgumentException($"Parameter '{argument}' not found in {method.DeclaringType.Name}.{method.Name} method.");
            }

            return Next;
        }
    }
}