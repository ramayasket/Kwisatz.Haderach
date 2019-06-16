using System;
using PostSharp.Aspects;

namespace Kw.Aspects
{
	/// <summary>
	/// Cleans up memory upon executing attributed method.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class GetTotalMemoryAttribute : MethodInterceptionAspect
	{
		public sealed override void OnInvoke(MethodInterceptionArgs args)
		{
			args.Proceed();

			GC.GetTotalMemory(true);
		}
	}
}
