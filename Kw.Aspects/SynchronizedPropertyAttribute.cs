using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Aspects;

namespace Kw.Aspects
{
	/// <summary>
	/// Makes property of field synchronized.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Property)]
	[LinesOfCodeAvoided(28)]
	public class SynchronizedPropertyAttribute : LocationInterceptionAspect
	{
		/// <summary>
		/// Invoked instead of the Get semantic of the property to which the current aspect is applied.
		/// </summary>
		public override void OnGetValue(LocationInterceptionArgs args)
		{
			if (null != args.Instance)
			{
				lock (args.Instance)
				{
					base.OnGetValue(args);
				}
			}
			else
			{
				base.OnGetValue(args);
			}
		}

		/// <summary>
		/// Invoked instead of the Set semantic of the property to which the current aspect is applied.
		/// </summary>
		public override void OnSetValue(LocationInterceptionArgs args)
		{
			if (null != args.Instance)
			{
				lock (args.Instance)
				{
					base.OnSetValue(args);
				}
			}
			else
			{
				base.OnSetValue(args);
			}
		}
	}
}
