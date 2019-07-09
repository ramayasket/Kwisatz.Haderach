using System;
using PostSharp.Aspects;
using Kw.Common;

namespace Kw.Aspects
{
	/// <summary>
	/// Cleans up memory upon executing attributed method.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	[LinesOfCodeAvoided(18)]
	public class GetTotalMemoryAttribute : MethodInterceptionAspect
	{
		public sealed override void OnInvoke(MethodInterceptionArgs args)
		{
			try
			{
				args.Proceed();
			}
			finally
			{
				GC.GetTotalMemory(true);
			}
		}
	}
}
