using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Kw.Common
{
	/// <summary>
	/// Константы общего назначения
	/// ReSharper disable InconsistentNaming
	/// </summary>
	public static class Literals
	{
		private static readonly OverridableLiteral _site = new OverridableLiteral("SiteServices");
		private static readonly OverridableLiteral _buffer = new OverridableLiteral("BufferingServices");
		private static readonly OverridableLiteral _app = new OverridableLiteral("ApplicationServices");
		private static readonly OverridableLiteral _inhouse = new OverridableLiteral("InhouseServices");

		public static OverridableLiteral SiteServices => _site;
		public static OverridableLiteral BufferingServices => _buffer;
		public static OverridableLiteral InhouseServices => _inhouse;
		public static OverridableLiteral ApplicationServices => _app;
		public static CultureInfo RussianCulture => CultureInfo.GetCultureInfo("ru-RU");
	}
}

