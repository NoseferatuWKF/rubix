using System.Runtime.InteropServices;

namespace WinAPI;

public enum Modifiers : uint
{
	MOD_ALT = 0x0001,
	MOD_NOREPEAT = 0x4000,
	WM_HOTKEY = 0x0312,
}

public class User32
{
	public struct Msg 
	{
		public IntPtr Hwnd;
		public uint Message;
		public IntPtr WParam;
		public IntPtr LParam;
	}

	[DllImport("user32.dll")]
	public static extern bool RegisterHotKey(IntPtr hWnd, int id, Modifiers fsModifiers, uint vk);

	[DllImport("user32.dll")]
	public static extern bool GetMessage(out Msg lpMsg, IntPtr hWnd, Modifiers wMsgFilterMin, Modifiers wMsgFilterMax);
}
