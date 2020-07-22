using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kw.Aspects
{
    /// <summary>
    /// Gives method singlecall (analogous to singletone) pattern.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    [MulticastAttributeUsage(MulticastTargets.Method | MulticastTargets.InstanceConstructor)]
    [ProvideAspectRole(BasicRoles.Control)]
    public sealed class SinglecallAttribute : OnMethodBoundaryAspect
    {
        private static readonly HashSet<MethodBase> _methods = new HashSet<MethodBase>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public override void OnEntry(MethodExecutionArgs args)
        {
            var m = args.Method;

            lock (_methods)
            {
                if (_methods.Contains(args.Method))
                    throw new InvalidOperationException($"{args.Method.DeclaringType?.Name ?? string.Empty}.{args.Method.Name} is a single-call method and has already been called.");

                _methods.Add(args.Method);
            }
        }
    }
}
