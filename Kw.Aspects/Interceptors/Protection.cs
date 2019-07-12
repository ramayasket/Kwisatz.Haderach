using System;
using System.Reflection;
using PostSharp.Aspects;

namespace Kw.Aspects.Interceptors
{
	/// <summary>
	/// Sets method return value
	/// </summary>
	public class Protection : Interceptor
	{
		/// <inheritdoc />
		public Protection(Interceptor next) : base(next) { }

		/// <inheritdoc />
		public override void Invoke(MethodInterceptionArgs args)
		{
			try
			{
				Next.Invoke(args);
			}
			catch (Exception x)
			{
				args.ReturnValue = x;
			}
		}

		/// <inheritdoc />
		public override Interceptor Compile(MethodBase method)
		{
			var rt = (method as MethodInfo)?.ReturnType ?? typeof(void);

			if(!rt.IsAssignableFrom(typeof(Exception)))
				throw new Exception($"Type {rt.Name} isn't assignable from Exception.");

			return base.Compile(method);
		}
	}
}