using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace /* ReSharper disable once CheckNamespace */ Kw.WinAPI
{
	/// <summary>
	/// Imports for user32.dll
	/// </summary>
	public static class User
	{
		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		public static extern int CallNextHookEx(int idHook, int nCode, WM wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern int SetWindowsHookEx(WH idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern int UnhookWindowsHookEx(int idHook);

		[DllImport("user32")]
		public static extern int GetDoubleClickTime();

		[DllImport("user32")]
		public static extern int ToAscii(VK uVirtKey, uint uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, bool uFlags);

		[DllImport("user32")]
		public static extern int GetKeyboardState(byte[] pbKeyState);

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		public static extern short GetKeyState(VK vKey);

		[DllImport("user32.dll")]
		static extern void keybd_event(VK bVk, byte bScan, uint dwFlags, int dwExtraInfo);

		private const int KEYEVENTF_EXTENDEDKEY = 0x1;
		private const int KEYEVENTF_KEYUP = 0x2;

		/// <summary>
		/// Emulates keyboard down key.
		/// </summary>
		/// <param name="key"></param>
		public static void KeyboardDown(VK key)
		{
			keybd_event(key, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
		}

		/// <summary>
		/// Emulates keyboard up key.
		/// </summary>
		/// <param name="key"></param>
		public static void KeyboardUp(VK key)
		{
			keybd_event(key, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
		}

		/// <summary>
		/// Emulates keyboard press key.
		/// </summary>
		/// <param name="key"></param>
		public static void KeyboardPress(VK key)
		{
			KeyboardDown(key);
			KeyboardUp(key);
		}
	}
}

