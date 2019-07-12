using System;
using System.Reflection;
using Kw.Common;
using PostSharp.Aspects;

namespace Kw.Aspects.Interceptors // ReSharper disable PossibleNullReferenceException
{
	/// <summary>
	/// Forces full garbage collection upon exiting the method.
	/// </summary>
	public class NonNullCondition : Interceptor
	{
		/// <inheritdoc />
		public NonNullCondition(Interceptor next) : base(next) { }

		/// <inheritdoc />
		public override void Invoke(MethodInterceptionArgs args)
		{
			var property = args.Method.QuerySingleAttribute<NonNullConditionAttribute>(true)?.Property;

			object value;

			try
			{
				value = args.Instance.GetType().GetProperty(property).GetValue(args.Instance);
			}
			catch
			{
				value = null;
			}

			if (null != value)
				args.Proceed();
		}

		/// <inheritdoc />
		public override Interceptor Compile(MethodBase method)
		{
			var att = method.QuerySingleAttribute<NonNullConditionAttribute>(true);

			if(null == att)
				throw new Exception($"Attribute [NonNullCondition] is required but not applied to {method.Name} method.");

			if (null == att.Property)
				throw new Exception($"Attribute [NonNullCondition] does not specify property name.");

			if(null == method.DeclaringType.GetProperty(att.Property))
				throw new Exception($"Property {att.Property} isn't declared in {method.DeclaringType.Name} type.");

			return Next;
		}
	}
}