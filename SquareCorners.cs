using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

class SquareCorners {
	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int pvAttribute, int cbAttribute);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll")]
	private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

	[DllImport("user32.dll")]
	private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

	[DllImport("user32.dll")]
	private static extern bool TranslateMessage([In] ref MSG lpMsg);

	[DllImport("user32.dll")]
	private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

	[DllImport("user32.dll", SetLastError = true)]
	static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

	const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
	const int DWMWCP_DONOTROUND = 1;
	
	// FIX: We keep 0x0017 (without FRAMECHANGED) which useful for glitches
	const uint SWP_FLAGS = 0x0017; 
	
	const uint WINEVENT_OUTOFCONTEXT = 0;
	const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
	const uint EVENT_OBJECT_SHOW = 0x8002;

	[StructLayout(LayoutKind.Sequential)]
	struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public int pt_x; public int pt_y; }

	static IntPtr hookHandle = IntPtr.Zero;
	static WinEventDelegate dele = null;

	static string GetProcessName(IntPtr hWnd) {
		try {
			uint pid;
			GetWindowThreadProcessId(hWnd, out pid);
			Process p = Process.GetProcessById((int)pid);
			return p.ProcessName;
		} catch {
			return "";
		}
	}

	static void ApplyStyle(IntPtr handle) {
		if (handle == IntPtr.Zero || !IsWindowVisible(handle)) return;

		StringBuilder classBuff = new StringBuilder(256);
		GetClassName(handle, classBuff, 256);
		string className = classBuff.ToString();

		// --- ESCLUSION CLASSES (SYSTEM & DIALOGS) ---
		// #32770           = Dialoghi standard (Proprietà, Salva con nome, etc.)
		// Shell_TrayWnd    = Barra delle Applicazioni (Taskbar)
		// Progman / WorkerW = Desktop
		// Windows.UI.Core.CoreWindow = Elementi Shell/Start Menu/Notification Center
		if (className == "#32770" || 
			className == "Shell_TrayWnd" || 
			className == "Progman" || 
			className == "WorkerW" ||
			className == "Windows.UI.Core.CoreWindow") 
		{
			return;
		}

		// --- ESCLUSION PROCESSES ---
		string procName = GetProcessName(handle);
		
		// NOTE: removed "explorer". classes above (Shell_TrayWnd, Progman) already protect the desktop and taskbar
		if (procName.Equals("GoogleDriveFS", StringComparison.OrdinalIgnoreCase)) return;


		// --- APPLY STYLE ---
		int cornerAttr = DWMWCP_DONOTROUND;
		DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerAttr, sizeof(int));
		SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_FLAGS);
	}

	static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
		if (idObject == 0 && idChild == 0 && hwnd != IntPtr.Zero) {
			ApplyStyle(hwnd);
		}
	}

	static void Main() {
		dele = new WinEventDelegate(WinEventProc);
		hookHandle = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_OBJECT_SHOW, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
		
		MSG msg;
		while (GetMessage(out msg, IntPtr.Zero, 0, 0)) {
			TranslateMessage(ref msg);
			DispatchMessage(ref msg);
		}

		if (hookHandle != IntPtr.Zero) UnhookWinEvent(hookHandle);
	}
}
