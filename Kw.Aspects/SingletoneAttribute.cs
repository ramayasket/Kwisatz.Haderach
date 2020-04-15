////
////  FUNDAMENTA MATHEMATICAE ET COGITATIONIS (FORAON)
////  by Andrei Samoylov of MIREA (C) 2013–2019
////
////  https://mixed.systems
////  as@andrique.ru
////

using System;
using System.Collections.Concurrent;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Extensibility;
using PostSharp.Reflection;

namespace Mixed.Model.Aspects
{
	/// <summary>
	/// Returns single instance for the declaring class.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Property)]
	[MulticastAttributeUsage(MulticastTargets.Property)]
	[LinesOfCodeAvoided(1)]
	public sealed class SingletoneValueAttribute : LocationInterceptionAspect
	{
		private Type _declaringType;

		/// <summary>
		/// Compile-time initialization.
		/// </summary>
		public override void CompileTimeInitialize(LocationInfo targetLocation, AspectInfo aspectInfo)
		{
			_declaringType = targetLocation.DeclaringType;

			base.CompileTimeInitialize(targetLocation, aspectInfo);
		}

		/// <summary>
		/// Intercepts property get operation.
		/// </summary>
		public override void OnGetValue(LocationInterceptionArgs args)
		{
			args.Value = SingletoneAttribute.GetInstance(_declaringType);
		}
	}

	/// <summary>
	/// Makes a class singletone.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Class)]
	[MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict)]
	[LinesOfCodeAvoided(10)]
	public class SingletoneAttribute : InstanceLevelAspect
	{
		private static readonly ConcurrentDictionary<Type, object> _instances = new ConcurrentDictionary<Type, object>();
		private Type _instanceType;

		/// <summary>
		/// Compile-time initialization.
		/// </summary>
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

		/// <summary>
		/// Returns single instance for the given type.
		/// </summary>
		/// <typeparam name="T">The type to get single instance for.</typeparam>
		/// <returns>Single instance value.</returns>
		public static T GetInstance<T>() => _instances.ContainsKey(typeof(T)) ? (T)_instances[typeof(T)] : default(T);
	}
}
