using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Kw.Common
{
	/// <summary>
	/// Методы расширения для работы с DateTime и TimeSpan
	/// </summary>
	public static class TimeExtensions
	{
		public static DateTime RoundSeconds(this DateTime t)
		{
			var tms = t.Millisecond;
			var sec = t.Second + ((tms > 500) ? 1 : 0);
			
			return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, sec);
		}

		public static TimeSpan RoundSeconds(this TimeSpan t)
		{
			var tms = t.TotalMilliseconds;
			var rms = Math.Round(tms/1000, 0) * 1000;

			return TimeSpan.FromMilliseconds(rms);
		}

		public static TimeSpan RoundMilliseconds(this TimeSpan t)
		{
			var tms = t.TotalMilliseconds;
			var rms = Math.Round(tms, 0);

			return TimeSpan.FromMilliseconds(rms);
		}

		public static TimeSpan Finish(this Stopwatch sw)
		{
			sw.Stop();
			return sw.Elapsed;
		}
	}
}

