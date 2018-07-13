using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kw.WinAPI
{
	public class TaskbarUtils
	{
		public static void RefreshNotificationArea()
		{
			var notificationAreaHandle = GetNotificationAreaHandle();
			if (notificationAreaHandle == IntPtr.Zero)
				return;

			RefreshWindow(notificationAreaHandle);
		}

		private static void RefreshWindow(IntPtr windowHandle)
		{
			const uint wmMousemove = 0x0200;
			RECT rect;

			User.GetClientRect(windowHandle, out rect);

			for (var x = 0; x < rect.right; x += 5)
			{
				for (var y = 0; y < rect.bottom; y += 5)
				{
					User.SendMessage(windowHandle, wmMousemove, 0, (y << 16) + x);
				}
			}
		}

		private static IntPtr GetNotificationAreaHandle()
		{
			const string notificationAreaTitle = "Notification Area";
			const string notificationAreaTitleInWindows7 = "User Promoted Notification Area";

			var systemTrayContainerHandle = User.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", string.Empty);
			var systemTrayHandle = User.FindWindowEx(systemTrayContainerHandle, IntPtr.Zero, "TrayNotifyWnd", string.Empty);
			var sysPagerHandle = User.FindWindowEx(systemTrayHandle, IntPtr.Zero, "SysPager", string.Empty);
			var notificationAreaHandle = User.FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", notificationAreaTitle);

			if (notificationAreaHandle == IntPtr.Zero)
				notificationAreaHandle = User.FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", notificationAreaTitleInWindows7);

			return notificationAreaHandle;
		}
	}
}
