is a simple active .exe process running in background to disable the rounded corners in Windows 11

tested on **Windows 11 Pro 25H2 build 26200.7623**

before launching the process need forcing File Explorer to run in a separate process:  

`Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "SeparateProcess" -Value 1 -Type DWORD`  

⚠️ *without this tweak the SquareCorners not work on File Explorer...  
this why this way forces it to use a legacy rendering model that responds better to DWM commands*


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
(but is not needed since SC uses SetWindowPos which updates windows in real time without crashing the desktop manager)

ℹ️ *any window-app already opened before SC start may need to be restarted*


# HOW TO TWEAK

### 🧐 How to exclude specific processes (Tutorial 👉 need to recompile OBV):

1. Add this requirement to `DllImports` section
```
[DllImport("user32.dll")]
private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
```
2. Define the exclusion list inside the `SquareCorners` class
```
private static readonly HashSet<string> Exclusions = new HashSet<string> {
	"vlc",
	"steam",
	"example_game"
};
```
3. Update the `EventCallback` method with the filtering logic
```
static void EventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
	if (idObject == 0 && hwnd != IntPtr.Zero) {
		try {
			// Retrieve the process ID and name from the window handle
			GetWindowThreadProcessId(hwnd, out uint processId);
			string pName = Process.GetProcessById((int)processId).ProcessName.ToLower();

			// Skip execution if the process name is in the exclusion list
			if (Exclusions.Contains(pName)) return;

			// Apply square corners and border fix
			int cornerAttr = 1; // DWMWCP_DONOTROUND
			DwmSetWindowAttribute(hwnd, 33, ref cornerAttr, sizeof(int));
			
			int borderColor = 0x010101;
			DwmSetWindowAttribute(hwnd, 34, ref borderColor, sizeof(int));
			
			SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, 0x0027);
		}
		catch {
			// ⚙️ HERE YOU CAN ADD SETUP IGNORED RESTRICTED PROCESSES, FOLLOW EXAPLES
      		// if (pName == "notepad") return;
      		// AND IF NEED IGNORE MORE THAN ONE PROCESS...
      		// if (pName == "notepad" || pName == "explorer" || pName == "lockapp" || pName == "shellexperiencehost") return;
		}
	}
}
```

# HOW TO COMPILE (if you want to compile the .exe yourself)

> I made SC myself so I dont care much about security...  
> the code is simple and easy you can review the source  
> and if you dont trust the .exe just dont use it ! :|  

the .cs file is the source code, you can compile it yourself using PS (for stay sure and safe)

1. download both .cs files from source

2. `Test-Path "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"`  
if the result is **True** you are ready to compile, otherwise you need use a C# Compiler (Roslyn) included in .NET SDK

3. `& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /reference:System.Windows.Forms.dll /out:"<PATH_OF_YOUR_CHOICE>\SquareCorners.exe" "<PATH_OF_CS_FILES>\SquareCorners.cs" "<PATH_OF_CS_FILES>\AssemblyInfo.cs"`  
this comand using the integrated Windows compiler if present

# LAST WORDS (LOL)

*the 1.0 version is deleted why not really stable, also use CPU resources (even very very less) every 2-3sec, so...*
