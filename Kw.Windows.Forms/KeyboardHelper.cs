using System;
using System.Windows.Forms;
using Kw.WinAPI;

namespace Kw.Windows.Forms
{
	public static class KeyboardHelper
	{
		public static Keys VK2Keys(VK vk)
		{
			return (Keys)((int)vk);
		}

		public static VK Keys2VK(Keys keys)
		{
			switch (keys)
			{
				case Keys.Alt: return VK.MENU;
				case Keys.Control: return VK.LCONTROL;
				case Keys.Shift: return VK.LSHIFT;
				case Keys.KeyCode:
				case Keys.LineFeed:
				case Keys.Modifiers:
				case Keys.None:
					return (VK)0;
				default:
					return (VK)keys;
			}

			throw new NotImplementedException();
		}
	}
}
