using System;
using PostSharp.Aspects;

namespace Kw.Aspects.Interceptors
{
	/// <summary>
	/// Forces full garbage collection upon exiting the method.
	/// </summary>
	public class GetTotalMemory : Interceptor
	{
		/// <inheritdoc />
		public GetTotalMemory(Interceptor next) : base(next) { }

		/// <inheritdoc />
		public override void Invoke(MethodInterceptionArgs args)
		{
			try
			{
				Next.Invoke(args);
			}
			finally
			{
				GC.GetTotalMemory(true);
			}
		}
	}
}