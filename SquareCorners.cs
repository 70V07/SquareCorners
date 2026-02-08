using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;

class SquareCorners {
	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int pvAttribute, int cbAttribute);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll")]
	private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

	[DllImport("user32.dll")]
	private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

	[DllImport("user32.dll")]
	private static extern bool TranslateMessage([In] ref MSG lpMsg);

	[DllImport("user32.dll")]
	private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

	const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
	const int DWMWCP_DONOTROUND = 1;
	const int DWMWA_BORDER_COLOR = 34;
	const uint SWP_FLAGS = 0x0027;
	const uint WINEVENT_OUTOFCONTEXT = 0;
	const uint EVENT_OBJECT_CREATE = 0x8000;
	const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

	[StructLayout(LayoutKind.Sequential)]
	struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public int pt_x; public int pt_y; }

	static WinEventDelegate dele = null;

	static int GetAccentColor() {
		try {
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM")) {
				if (key != null) {
					object val = key.GetValue("AccentColor");
					if (val != null) return (int)val;
				}
			}
		} catch { }
		return 0x00FFFFFF;
	}

	static void ApplyStyle(IntPtr handle) {
		if (handle == IntPtr.Zero) return;
		int cornerAttr = DWMWCP_DONOTROUND;
		int colorToApply = (handle == GetForegroundWindow()) ? GetAccentColor() : 0x003A3A3A;
		DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerAttr, sizeof(int));
		DwmSetWindowAttribute(handle, DWMWA_BORDER_COLOR, ref colorToApply, sizeof(int));
		SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_FLAGS);
	}

	static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
		if (idObject == 0 && hwnd != IntPtr.Zero) ApplyStyle(hwnd);
	}

	static void Main() {
		dele = new WinEventDelegate(WinEventProc);
		SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_OBJECT_CREATE, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
		
		MSG msg;
		while (GetMessage(out msg, IntPtr.Zero, 0, 0)) {
			TranslateMessage(ref msg);
			DispatchMessage(ref msg);
		}
	}
}
