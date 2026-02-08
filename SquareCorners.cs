using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;

class SquareCorners {
	/* Import Desktop Window Manager API to modify window attributes */
	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int pvAttribute, int cbAttribute);

	/* Import User32 API to refresh window position and state */
	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	/* Import User32 API to monitor system-wide window events */
	[DllImport("user32.dll")]
	private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

	/* Standard Windows message loop imports */
	[DllImport("user32.dll")]
	private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

	[DllImport("user32.dll")]
	private static extern bool TranslateMessage([In] ref MSG lpMsg);

	[DllImport("user32.dll")]
	private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

	/* Function to identify the window currently in focus */
	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	/* Delegate for handling Windows events callbacks */
	delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

	/* DWM Attribute Constants for Windows 11 styling */
	const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
	const int DWMWCP_DONOTROUND = 1; // Forces square corners
	const int DWMWA_BORDER_COLOR = 34; // Allows custom border color
	
	/* Flags for SetWindowPos to trigger a refresh without moving the window */
	const uint SWP_FLAGS = 0x0027; // NOSIZE | NOMOVE | NOZORDER | FRAMECHANGED
	
	/* Event Hook Constants */
	const uint WINEVENT_OUTOFCONTEXT = 0;
	const uint EVENT_OBJECT_CREATE = 0x8000;
	const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

	/* Windows Message Structure */
	[StructLayout(LayoutKind.Sequential)]
	struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public int pt_x; public int pt_y; }

	/* Persist the delegate in memory to prevent Garbage Collection */
	static WinEventDelegate dele = null;

	/* Retrieves the Windows Accent Color from the Registry */
	static int GetAccentColor() {
		try {
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM")) {
				if (key != null) {
					object val = key.GetValue("AccentColor");
					if (val != null) return (int)val;
				}
			}
		} catch { }
		return 0x00FFFFFF; // Fallback to white if registry access fails
	}

	/* Applies the visual styles (square corners and color) to a specific window handle */
	static void ApplyStyle(IntPtr handle) {
		if (handle == IntPtr.Zero) return;
		int cornerAttr = DWMWCP_DONOTROUND;
		
		/* Use Accent Color for focused window, dark grey for inactive windows */
		int colorToApply = (handle == GetForegroundWindow()) ? GetAccentColor() : 0x003A3A3A;
		
		/* Set corner preference to square */
		DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerAttr, sizeof(int));
		
		/* Apply border color */
		DwmSetWindowAttribute(handle, DWMWA_BORDER_COLOR, ref colorToApply, sizeof(int));
		
		/* Force the window to redraw with the new frame attributes */
		SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_FLAGS);
	}

	/* Callback executed when a window is created or gains focus */
	static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
		/* Ensure the event target is a window object */
		if (idObject == 0 && hwnd != IntPtr.Zero) ApplyStyle(hwnd);
	}

	static void Main() {
		/* Initialize the event hook to listen for foreground changes and new windows */
		dele = new WinEventDelegate(WinEventProc);
		SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_OBJECT_CREATE, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
		
		/* Keep the process alive and responsive to system messages */
		MSG msg;
		while (GetMessage(out msg, IntPtr.Zero, 0, 0)) {
			TranslateMessage(ref msg);
			DispatchMessage(ref msg);
		}
	}
}
