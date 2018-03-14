using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kw.Windows
{
	public interface IResourceRegistry
	{
		Image LoadIcon(string name);
		FontFamily LoadFont(string name);
	}

	public static class ResourceRegistry
	{
		private class EmptyRegistry : IResourceRegistry
		{
			public Image LoadIcon(string name)
			{
				return null;
			}

			public FontFamily LoadFont(string name)
			{
				return null;
			}
		}

		private static IResourceRegistry _registry;

		public static IResourceRegistry Registry
		{
			get { return _registry ?? new EmptyRegistry(); }
			set { _registry = value; }
		}
	}
}


