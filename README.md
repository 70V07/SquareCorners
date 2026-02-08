is a simple active .exe process running in background to disable the rounded corners in Windows 11

tested on **Windows 11 Pro 25H2 build 26200.7623**

before launching the process need forcing File Explorer to run in a separate process:

`Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "SeparateProcess" -Value 1 -Type DWORD`  

⚠️ *without this tweak the SquareCorners not work on File Explorer...  
this why this way forces it to use a legacy rendering model that responds better to DWM commands*

---

# HOW TO USE

ℹ️ *if you want the process to work on admin-windows you must run it with administrator privileges*

1. download Latest Relase and put the .exe in a folder of your choice

2. execute the process (now the process is in background, so you can kill it using Task Manager, if need)

3. following how to add SC on Windows startup...

adds using Tasks:  
`Register-ScheduledTask -TaskName "SquareCornersBackground" -Action (New-ScheduledTaskAction -Execute "<PATH_OF_THE_EXE>") -Trigger (New-ScheduledTaskTrigger -AtLogOn) -Principal (New-ScheduledTaskPrincipal -GroupId "Builtin\Administrators" -RunLevel Highest) -Settings (New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit 0) -Force`  
and to remove if need...  
`Unregister-ScheduledTask -TaskName "SquareCornersBackground" -Confirm:$false`

adds using Registry (current user only):  
`Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "SquareCorners" -Value "<PATH_OF_THE_EXE>"`  
and to remove...  
`Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "SquareCorners" -ErrorAction SilentlyContinue`

adds using Registry (all users):  
`Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name "SquareCorners" -Value "<PATH_OF_THE_EXE>"`  
and to remove...  
`Remove-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name "SquareCorners" -ErrorAction SilentlyContinue`

if not work can try reload DWM (Desktop Window Manager) using PS: `Stop-Process -Name "dwm" -Force`  

ℹ️ *any window-app already opened before SC start may need to be restarted*

---

# HOW TO EXCLUDE SPECIFIC PROCESSES

some legacy applications or specific windows (like "Properties" dialogs) may show visual artifacts (exa: white borders) when forced to have square corners  
since v2.1 the tweak includes a native exclusion system. there is no external config file, so you must edit the source code and recompile to add new exclusions

1.  open SquareCorners.cs with a text editor (Notepad is fine)
    
2.  locate the `static void ApplyStyle(IntPtr handle) function`
    
3.  Scroll down to the // DISABLE PROCESSES section
    
4.  add a line for the process you want to exclude using its **Process Name** (without .exe)

**Example:** If you want to exclude Notepad:

codeC#

```
// inside ApplyStyle function...

// DISABLE PROCESSES
string procName = GetProcessName(handle);

// Existing exclusion
if (procName.Equals("GoogleDriveFS", StringComparison.OrdinalIgnoreCase)) return;

// YOUR NEW EXCLUSION:
if (procName.Equals("notepad", StringComparison.OrdinalIgnoreCase)) return;
```

5.  Save the file and Recompile the .exe (see instructions below)

> 💡 **Tip:** You can find the exact Process Name in **Task Manager > Details tab**

---

# HOW TO COMPILE (if you want to compile the .exe yourself)

> I made SC myself so I dont care much about security...  
> the code is simple and easy you can review the source  
> and if you dont trust the .exe just dont use it ! :|  

the .cs file is the source code, you can compile it yourself using PS (for stay sure and safe)

1. download both .cs files from source

2. `Test-Path "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"`  
if the result is **True** you are ready to compile, otherwise you need use a C# Compiler (Roslyn) included in .NET SDK

3. `& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /reference:System.Windows.Forms.dll /out:"<OUTPUT_PATH>\SquareCorners.exe" "<SOURCE_PATH>\SquareCorners.cs" "<SOURCE_PATH>\SquareCorners\AssemblyInfo.cs"`  
this comand using the integrated Windows compiler if present

---

# LAST WORDS (LOL)

the 1.0 version is deleted why not really stable, also use CPU resources (even very very less) every seconds, so...

**anyway this is the old 1.0 code (just for reference):**
```
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
```
