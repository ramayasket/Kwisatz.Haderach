using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;
using System.Reflection;

namespace Kw.Aspects // ReSharper disable PossibleNullReferenceException

{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [ProvideAspectRole(BasicRoles.Condition)]
    public class WhenNonNullAttribute : MethodInterceptionAspect
    {
        private readonly string _property;

        public WhenNonNullAttribute(string property)
        {
            _property = property;
        }

        /// <inheritdoc />
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            object value;

            try
            {
                value = args.Instance.GetType().GetProperty(_property).GetValue(args.Instance);
            }
            catch
            {
                value = null;
            }

            if (null != value)
                args.Proceed();
        }

        /// <inheritdoc />
        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            if (method.IsStatic)
                throw new Exception("Only available with instance methods.");

            var property = method.DeclaringType.GetProperty(_property);

            if(null == property)
                throw new Exception($"Property {_property} isn't declared in {method.DeclaringType.Name} type.");

            base.CompileTimeInitialize(method, aspectInfo);
        }
    }
}
