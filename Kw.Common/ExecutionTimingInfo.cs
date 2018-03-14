using System;

namespace Kw.Common
{
	public class ExecutionTimingInfo
	{
		public TimeSpan Latest { get; private set; }
		public TimeSpan Average { get; private set; }
		public TimeSpan Summation { get; private set; }

		public int Measurings { get; private set; }

		public void Set(TimeSpan ts)
		{
			Latest = ts;

			Summation += ts;
			Measurings ++;

			Average = TimeSpan.FromTicks(Summation.Ticks / Measurings);
		}
	}
}
