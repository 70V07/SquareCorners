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

	// NEW IMPORTS TO IDENTIFY CLASSES AND PROCESSES
	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

	[DllImport("user32.dll", SetLastError = true)]
	static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

	// DWM CONSTANTS
	const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
	const int DWMWCP_DONOTROUND = 1;
	
	// SETWINDOWPOS FLAGS
	// MODIFICATO: Rimosso 0x0020 (SWP_FRAMECHANGED) per evitare bug grafici nei Common Item Dialogs
	const uint SWP_FLAGS = 0x0017; 
	
	const uint WINEVENT_OUTOFCONTEXT = 0;
	const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
	const uint EVENT_OBJECT_SHOW = 0x8002;

	[StructLayout(LayoutKind.Sequential)]
	struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public int pt_x; public int pt_y; }

	static IntPtr hookHandle = IntPtr.Zero;
	static WinEventDelegate dele = null;

	// HELPER METHOD TO SAFELY RETRIEVE THE PROCESS NAME
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

		// RETRIEVE THE WINDOW CLASS NAME
		StringBuilder classBuff = new StringBuilder(256);
		GetClassName(handle, classBuff, 256);
		string className = classBuff.ToString();

		// --- DISABLE WINDOWS DIALOGS ---
		// #32770 IS THE STANDARD CLASS FOR WINDOWS DIALOGS (PROPERTIES, RUN, FILE COPY, ETC.)
		// THESE WINDOWS OFTEN DRAW CUSTOM BORDERS THAT GLITCH IF FORCED.
		if (className == "#32770") return;


		// --- DISABLE PROCESSES ---
		// RETRIEVE THE PROCESS NAME ONLY IF NECESSARY
		string procName = GetProcessName(handle);
		
		// EXCLUSION FOR GOOGLE DRIVE DESKTOP (PREVENTS WHITE BORDER GLITCH)
		// AGGIUNTO: explorer per prevenire bug di rendering nei dialoghi shell gestiti dal processo explorer.exe
		if (procName.Equals("GoogleDriveFS", StringComparison.OrdinalIgnoreCase) || procName.Equals("explorer", StringComparison.OrdinalIgnoreCase)) return;


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
