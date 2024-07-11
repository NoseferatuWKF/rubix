using VDM;
using WinAPI;

static class Program
{
	static void Main(string[] args)
	{
		// TODO: make this dynamic and configurable
		if (User32.RegisterHotKey(IntPtr.Zero, 1, MOD.ALT | MOD.NOREPEAT, '1')) Console.WriteLine("Registered Alt + 1");
		if (User32.RegisterHotKey(IntPtr.Zero, 2, MOD.ALT | MOD.NOREPEAT, '2')) Console.WriteLine("Registered Alt + 2");
		if (User32.RegisterHotKey(IntPtr.Zero, 3, MOD.ALT | MOD.NOREPEAT, '3')) Console.WriteLine("Registered Alt + 3");
		if (User32.RegisterHotKey(IntPtr.Zero, 4, MOD.ALT | MOD.NOREPEAT, '4')) Console.WriteLine("Registered Alt + 4");
		if (User32.RegisterHotKey(IntPtr.Zero, 5, MOD.ALT | MOD.NOREPEAT, '5')) Console.WriteLine("Registered Alt + 5");

		while (User32.GetMessage(out var msg, IntPtr.Zero, MOD.HOTKEY, MOD.HOTKEY))
		{
			Desktop.FromIndex((int) (msg.WParam - 1)).MakeVisible();
		}
	}
}
