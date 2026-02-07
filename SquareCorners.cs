using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

class SquareCorners {
	// Importazione per intercettare eventi di sistema
	[DllImport("user32.dll")]
	private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

	private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

	// Costanti per eventi: Creazione finestra (0x8000) e primo piano (0x0003)
	const uint EVENT_OBJECT_CREATE = 0x8000;
	const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
	const uint WINEVENT_OUTOFCONTEXT = 0;

	// Metodi DWM esistenti nel tuo codice
	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int pvAttribute, int cbAttribute);
	
	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
	const int DWMWCP_DONOTROUND = 1;
	const uint SWP_FLAGS = 0x0027;

	// Riferimento statico per impedire al Garbage Collector di eliminare il delegato
	private static WinEventDelegate _delegate;

	static void Main() {
		_delegate = new WinEventDelegate(EventCallback);
		
		// Registra l'hook per la creazione di nuovi oggetti (finestre)
		SetWinEventHook(EVENT_OBJECT_CREATE, EVENT_OBJECT_CREATE, IntPtr.Zero, _delegate, 0, 0, WINEVENT_OUTOFCONTEXT);
		// Registra l'hook per il cambio finestra attiva (copre finestre già aperte)
		SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _delegate, 0, 0, WINEVENT_OUTOFCONTEXT);

		// Necessario per mantenere il programma in ascolto degli eventi senza loop CPU
		System.Windows.Forms.Application.Run(); 
	}

	static void EventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
	if (idObject == 0 && hwnd != IntPtr.Zero) {
		// 1. Forza angoli retti
		int cornerAttribute = DWMWCP_DONOTROUND;
		DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerAttribute, sizeof(int));

		// 2. Tweak per il colore del bordo (Rimuove o scurisce il bordo bianco)
		// Impostando 0xFFFFFFFE (o un colore scuro) spesso risolve il glitch visivo
		int borderColor = 0x010101; // Nero quasi puro per nascondere il bordo
		DwmSetWindowAttribute(hwnd, 34, ref borderColor, sizeof(int)); // 34 = DWMWA_BORDER_COLOR

		SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_FLAGS);
	}
}
}
