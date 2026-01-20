using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

class SquareCorners {
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int pvAttribute, int cbAttribute);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    const int DWMWCP_DONOTROUND = 1;
    
    // Flag per SetWindowPos: NoMove (2) | NoSize (1) | NoZOrder (4) | FrameChanged (32) = 39 (0x0027)
    const uint SWP_FLAGS = 0x0027;

    static void Main() {
        while (true) {
			foreach (Process p in Process.GetProcesses()) {
				// Skip System and Idle processes immediately to avoid unnecessary access attempts
				if (p.Id <= 4) continue; 

				try {
					IntPtr handle = p.MainWindowHandle;
					if (handle != IntPtr.Zero) {
						int attribute = DWMWCP_DONOTROUND;
						
						// Apply the DWM attribute for square corners
						DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref attribute, sizeof(int));
						
						// Force window frame graphics refresh
						SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_FLAGS);
					}
				}
				catch {
					// Ignore processes with restricted access (e.g. protected Antivirus or TrustedInstaller)
					// Example: Console.WriteLine("Access denied for process: " + p.ProcessName);
				}
			}
            Thread.Sleep(2000); // set the frequency for scanning windows
        }
    }
}
