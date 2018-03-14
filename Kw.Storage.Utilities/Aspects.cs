using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kw.Aspects;
using Kw.Common;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Configuration;
using PostSharp.Extensibility;
using PostSharp.Reflection;

namespace Kw.Storage.Utilities
{
	/// <summary>
	/// Ключевое поле для ссылочных наборов.
	/// </summary>
	public interface IKeyedReference
	{
		object ReferenceKey { get; set; }
	}

	/// <summary>
	/// Помечает тип как ссылочные данных с ключом.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Class)]
	[MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict)]
	[IntroduceInterface(typeof(IKeyedReference), OverrideAction = InterfaceOverrideAction.Ignore)]
	public class KeyedReferenceAttribute : InstanceLevelAspect, IKeyedReference
	{
		/// <summary>
		/// Добавленное свойство: ключевое значение для ссылочных данных.
		/// </summary>
		[IntroduceMember(OverrideAction = MemberOverrideAction.Ignore, Visibility = Visibility.Public)]
		public object ReferenceKey { get; set; }

		[OnLocationSetValueAdvice]
		[MulticastPointcut(Targets = MulticastTargets.Property)]
		public void OnPropertySet(LocationInterceptionArgs args)
		{
			if (args.Value == args.GetCurrentValue()) return;

			args.ProceedSetValue();
		}
	}

	public static class KeyedReferencing
	{
		public static object ReferenceKey(this object referenced)
		{
			var keyedReference = referenced as IKeyedReference;

			if(null != keyedReference)
			{
				return keyedReference.ReferenceKey;
			}

			return null;
		}

		public static T ReferenceKey<T>(this object referenced)
		{
			var keyedReference = referenced as IKeyedReference;

			if (null != keyedReference)
			{
				return Reinterpret<object>.Cast<T>(keyedReference.ReferenceKey);
			}

			return default(T);
		}
	}
	
	/// <summary>
	/// Давит PS0131.
	/// </summary>
	[Serializable, AttributeUsage(AttributeTargets.Method)]
	public class Yaspect : MethodInterceptionAspect
	{
		public sealed override void OnInvoke(MethodInterceptionArgs args) { args.Proceed(); }
	}
}

