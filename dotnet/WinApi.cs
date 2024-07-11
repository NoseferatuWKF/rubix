using System.Runtime.InteropServices;
using System.Text;
using VDM;

namespace WinAPI;

internal enum MOD : UInt32
{
	ALT = 0x0001,
	NOREPEAT = 0x4000,
	HOTKEY = 0x0312,
}

sealed internal class User32
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct Msg 
	{
		internal IntPtr Hwnd;
		internal UInt32 Message;
		internal IntPtr WParam;
		internal IntPtr LParam;
	}

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
	[DllImport("user32.dll")]
	internal static extern Boolean RegisterHotKey(IntPtr hWnd, Int32 id, MOD fsModifiers, UInt32 vk);

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessage
	[DllImport("user32.dll")]
	internal static extern Boolean GetMessage(out Msg lpMsg, IntPtr hWnd, MOD wMsgFilterMin, MOD wMsgFilterMax);

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowthreadprocessid
	[DllImport("user32.dll")]
	internal static extern UInt32 GetWindowThreadProcessId(IntPtr hWnd, out Int32 lpdwProcessId);

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-attachthreadinput
	[DllImport("user32.dll")]
	internal static extern Boolean AttachThreadInput(UInt32 idAttach, UInt32 idAttachTo, Boolean fAttach);

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getforegroundwindow
	[DllImport("user32.dll")]
	internal static extern IntPtr GetForegroundWindow();

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern Boolean SetForegroundWindow(IntPtr hWnd);

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumwindows
	[DllImport("User32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern Boolean EnumWindows(Desktop.CallBackPtr lpEnumFunc, IntPtr lParam);

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowtextlengtha
	[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern Int32 GetWindowTextLength(IntPtr hWnd);

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowtexta
	[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern Int32 GetWindowText(IntPtr hWnd, StringBuilder lpString, Int32 nMaxCount);
}

sealed internal class Kernel32
{
	// https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getcurrentthreadid
	[DllImport("kernel32.dll")]
	internal static extern UInt32 GetCurrentThreadId();
}
