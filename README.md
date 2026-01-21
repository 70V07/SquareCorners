is a simple active .exe process running in background to disable the rounded corners in Windows 11

tested on **Windows 11 Pro 25H2 build 26200.7623**

before launching the process need forcing File Explorer to run in a separate process:  

`Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "SeparateProcess" -Value 1 -Type DWORD`  

⚠️ without this tweak the SquareCorners not work on File Explorer  
*this why this way forces it to use a legacy rendering model that responds better to DWM commands*


# HOW TO USE

ℹ️ if you want the process to run on admin-windows you must run it with administrator privileges

1. download *SquareCorners.exe* file and put it in a folder of your choice

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

- - -

if not work can try reload DWM (Desktop Window Manager) using PS: `Stop-Process -Name "dwm" -Force`  
(but is not needed since SC uses SetWindowPos which updates windows in real time without crashing the desktop manager)

ℹ️ *any window-app already opened before SC start may need to be restarted*


# HOW TO TWEAK

this line is for increase the frequency SC scan windows for disable rounded corners

`Thread.Sleep(2000);` (milliseconds)


# HOW TO COMPILE (if you want to compile the .exe yourself)

> OBV is a personal service, so the antivirus show many reports  
> also the code is simple and easy you can see the source
> if you dont trust SC just dont use it ! :|
> HASH: 112e25fa928dc7e101c1c30de9b2f4fe323f9675fb453d1637995a2add4affd5
> VT report: [https://www.virustotal.com/gui/file/112e25fa928dc7e101c1c30de9b2f4fe323f9675fb453d1637995a2add4affd5](https://www.virustotal.com/gui/file/112e25fa928dc7e101c1c30de9b2f4fe323f9675fb453d1637995a2add4affd5)

the .cs file is the source code, you can compile it yourself using PS (for stay sure and safe)

1. download *SquareCorners.cs*

2. `Test-Path "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"`  
if the result is **True** you are ready to compile, otherwise you need use a C# Compiler (Roslyn) included in .NET SDK

3. `& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /out:"<PATH_OF_THE_EXE>\SquareCorners.exe" "<PATH_OF_THE_SOURCE>\SquareCorners.cs"`  
this comand using the integrated Windows compiler if present
