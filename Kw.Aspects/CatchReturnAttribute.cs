using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace Kw.Aspects
{
	[Serializable]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class CatchReturnAttribute : MethodInterceptionAspect
	{
		public sealed override void OnInvoke(MethodInterceptionArgs args)
		{
			try
			{
				args.Proceed();
			}
			catch (Exception x)
			{
				args.ReturnValue = x;
			}
		}
	}
}
