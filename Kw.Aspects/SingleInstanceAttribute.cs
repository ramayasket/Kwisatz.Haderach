using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Serialization;

namespace Kw.Aspects
{
	/// <summary>
	/// Returns single instance for the declaring class.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Property)]
	[MulticastAttributeUsage(MulticastTargets.Property)]
	[LinesOfCodeAvoided(1)]
	public class SingleInstanceValueAttribute : LocationInterceptionAspect
	{
		private Type _declaringType;

		/// <inheritdoc />
		public override void CompileTimeInitialize(LocationInfo targetLocation, AspectInfo aspectInfo)
		{
			_declaringType = targetLocation.DeclaringType;

			base.CompileTimeInitialize(targetLocation, aspectInfo);
		}

		/// <inheritdoc />
		public override void OnGetValue(LocationInterceptionArgs args)
		{
			args.Value = SingleInstanceAttribute.GetInstance(_declaringType);
		}
	}

	/// <summary>
	/// Makes a class singletone.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Class)]
	[MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict)]
	[LinesOfCodeAvoided(10)]
	public class SingleInstanceAttribute : InstanceLevelAspect
	{
		private static readonly ConcurrentDictionary<Type, object> _instances = new ConcurrentDictionary<Type, object>();
		private Type _instanceType;

		/// <inheritdoc />
		public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
		{
			_instanceType = type;

			base.CompileTimeInitialize(type, aspectInfo);
		}

		/// <summary>
		/// Handles construction of the target class instance.
		/// </summary>
		[OnMethodEntryAdvice, MulticastPointcut(MemberName = ".ctor")]
		public void OnConstructing(MethodExecutionArgs args)
		{
			var i = Instance;
			var it = i.GetType();

			var v = _instances.GetOrAdd(it, i);

			if (!ReferenceEquals(v, i))
				throw new InvalidOperationException($"Single instance for the type {it.Name} has already been initialized.");
		}

		/// <summary>
		/// Returns single instance for the given type.
		/// </summary>
		/// <param name="type">The type to get single instance for.</param>
		/// <returns>Single instance value.</returns>
		public static object GetInstance(Type type) => _instances.ContainsKey(type) ? _instances[type] : null;
	}
}
