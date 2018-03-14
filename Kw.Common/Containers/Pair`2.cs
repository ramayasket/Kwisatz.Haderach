using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kw.Common.Containers
{
	[Serializable]
	public class Pair<T1, T2>
	{
		public T1 First { get; set; }
		public T2 Second { get; set; }

		public override string ToString()
		{
			string first, second;

			if (!typeof (T1).IsValueType && (null == (object) First))
			{
				first = "<null>";
			}
			else
			{
				first = First.ToString();
			}

			if (!typeof(T2).IsValueType && (null == (object)Second))
			{
				second = "<null>";
			}
			else
			{
				second = Second.ToString();
			}

			return string.Format("{{{0}, {1}}}", first, second);
		}
	}
}

