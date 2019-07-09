using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kw.Common;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace Kw.Aspects
{
	[Serializable]
	public abstract class ProtectionHandler
	{
		public abstract void Catch(object source, Exception x);
	}

	[Serializable]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	[LinesOfCodeAvoided(13)]
	public class ProtectionAttribute : MethodInterceptionAspect
	{
		private readonly Type _handlerType;
		private ProtectionHandler _handler;

		public ProtectionAttribute(Type handlerType)
		{
			_handlerType = handlerType;
		}

		public ProtectionAttribute()
		{
		}

		/// <inheritdoc />
		public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
		{
			if (null != _handlerType)
			{
				if (!typeof(ProtectionHandler).IsAssignableFrom(_handlerType))
					throw new IncorrectTypeException($"Type {_handlerType.Name} isn't (but must be) derived from GuardedHandler.");

				_handler = (ProtectionHandler)Activator.CreateInstance(_handlerType);
			}


			base.CompileTimeInitialize(method, aspectInfo);
		}

		public sealed override void OnInvoke(MethodInterceptionArgs args)
		{
			try
			{
				args.Proceed();
			}
			catch (Exception x)
			{
				_handler?.Catch(args.Instance, x);

				args.ReturnValue = x;
			}
		}
	}
}
