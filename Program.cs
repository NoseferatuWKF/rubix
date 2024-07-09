using VDM;
using WinAPI;

static class Program
{
	static void Main(string[] args)
	{
		if (User32.RegisterHotKey(IntPtr.Zero, 1, Modifiers.MOD_ALT | Modifiers.MOD_NOREPEAT, '1')) Console.WriteLine("Registered Alt + 1");
		if (User32.RegisterHotKey(IntPtr.Zero, 2, Modifiers.MOD_ALT | Modifiers.MOD_NOREPEAT, '2')) Console.WriteLine("Registered Alt + 2");
		if (User32.RegisterHotKey(IntPtr.Zero, 3, Modifiers.MOD_ALT | Modifiers.MOD_NOREPEAT, '3')) Console.WriteLine("Registered Alt + 3");
		if (User32.RegisterHotKey(IntPtr.Zero, 4, Modifiers.MOD_ALT | Modifiers.MOD_NOREPEAT, '4')) Console.WriteLine("Registered Alt + 4");
		if (User32.RegisterHotKey(IntPtr.Zero, 5, Modifiers.MOD_ALT | Modifiers.MOD_NOREPEAT, '5')) Console.WriteLine("Registered Alt + 5");

		while (User32.GetMessage(out var msg, IntPtr.Zero, Modifiers.WM_HOTKEY, Modifiers.WM_HOTKEY))
		{
			Desktop.FromIndex((int) (msg.WParam - 1)).MakeVisible();
		}
	}
}
