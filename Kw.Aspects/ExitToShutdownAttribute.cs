using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Kw.Common;
using Kw.Common.Threading;
using PostSharp.Aspects;

namespace Kw.Aspects
{
	[Serializable]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	[LinesOfCodeAvoided(1)]
	public class ExitToShutdownAttribute : MethodInterceptionAspect
	{
		public sealed override void OnInvoke(MethodInterceptionArgs args)
		{
			args.Proceed();
			
			AppCore.Exiting = true;
		}
	}
}

