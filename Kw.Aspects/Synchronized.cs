using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kw.Aspects
{
    /// <summary>
    /// Parent class for synchronization attributes.
    /// </summary>
    public class Synchronized
    {
        /// <summary>
        /// Makes method call synchronized.
        /// </summary>
        [Serializable]
        [AttributeUsage(AttributeTargets.Method, Inherited = false)]
        [ProvideAspectRole(BasicRoles.Control)]
        public class MethodAttribute : MethodInterceptionAspect
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

        /// <summary>
        /// Makes property or field synchronized.
        /// </summary>
        [Serializable]
        [AttributeUsage(AttributeTargets.Property)]
        [ProvideAspectRole(BasicRoles.Control)]
        public class PropertyAttribute : LocationInterceptionAspect
        {
            /// <summary>
            /// Invoked instead of the Get semantic of the property to which the current aspect is applied.
            /// </summary>
            public override void OnGetValue(LocationInterceptionArgs args)
            {
                if (null != args.Instance)
                {
                    lock (args.Instance)
                    {
                        base.OnGetValue(args);
                    }
                }
                else
                {
                    base.OnGetValue(args);
                }
            }

            /// <summary>
            /// Invoked instead of the Set semantic of the property to which the current aspect is applied.
            /// </summary>
            public override void OnSetValue(LocationInterceptionArgs args)
            {
                if (null != args.Instance)
                {
                    lock (args.Instance)
                    {
                        base.OnSetValue(args);
                    }
                }
                else
                {
                    base.OnSetValue(args);
                }
            }
        }
    }
}
