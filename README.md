is a simple active .exe process running in background to disable the rounded corners

tested on **Windows 11 Pro 25H2 build 26200.7623**

before launching the process need forcing File Explorer to run in a separate process:  

`Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "SeparateProcess" -Value 1 -Type DWORD`  

⚠️ without this tweak the SquareCorners not work on File Explorer  
*this why this way forces it to use a legacy rendering model that responds better to DWM commands*

## HOW TO USE IT

1. download *SquareCorners.exe* file and put it in a folder of your choice

2. execute the process (now the process is in background, so you can kill it using Task Manager, if need)

if not work can try reload DWM (Desktop Window Manager) using PS: `Stop-Process -Name "dwm" -Force`  
(but is not needed since SC uses SetWindowPos which updates windows in real time without crashing the desktop manager)

ℹ️ *any Window already open may need to be restarted*

## HOW TO TWEAK IT

this line is for increase the frequency SC scan windows for disable rounded corners

`Thread.Sleep(2000);` (milliseconds)

## HOW TO COMPILE (if you want to compile the .exe yourself)

the .cs file is the source code, you can compile it yourself using PS

1. download *SquareCorners.cs*

2. `Test-Path "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"`  
if the result is **True** you are ready to compile, otherwise you need use a C# Compiler (Roslyn) included in modern .NET SDK

3. `& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /out:"<PATH_OF_THE_EXE>\SquareCorners.exe" "<PATH_OF_THE_SOURCE>\SquareCorners.cs"`  
this comand using the integrated Windows compiler if present
