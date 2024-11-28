using Rubix;
using WinAPI;

const Byte ASCII_OFFSET = 48; // 0
const Byte DESKTOP_LIMIT = 9;

// Loop over existing virtual desktops and registers hotkey for each index
var desktops = DesktopManager.VirtualDesktopManagerInternal.GetCount(); 

// append new desktops until limit
while (desktops < DESKTOP_LIMIT)
{
	DesktopManager.VirtualDesktopManagerInternal.CreateDesktop();
	desktops++;
}

// remove desktops from last index until limit
while (desktops > DESKTOP_LIMIT)
{
	var index = (Byte) (desktops - 1);
	DesktopManager.VirtualDesktopManagerInternal.RemoveDesktop(
		DesktopManager.GetDesktop(index),
		DesktopManager.GetDesktop(0));
	desktops--;
}

// register hotkeys foreach desktop index
for (UInt32 i = 1; i <= DESKTOP_LIMIT; ++i)
{
	// hotkey for switching desktops
	if (User32.RegisterHotKey(
			IntPtr.Zero,
			i,
			MOD.ALT | MOD.NOREPEAT,
			i + ASCII_OFFSET))
	{
		Console.WriteLine($"Registered Alt + {i}");
	}

	// hotkey for moving active windows between desktops
	if (User32.RegisterHotKey(
			IntPtr.Zero,
			// offset hotkey with desktop limit
			i + DESKTOP_LIMIT,
			MOD.ALT | MOD.SHIFT | MOD.NOREPEAT,
			i + ASCII_OFFSET))
	{
		Console.WriteLine($"Registered Alt + Shift {i}");
	}
}

// polling windows message queue and filter it by the registered hotkey
User32.Msg msg;
while (User32.GetMessage(out msg, IntPtr.Zero, MOD.HOTKEY, MOD.HOTKEY))
{
	var hotkey = (Byte) (msg.WParam - 1);
	if (hotkey < DESKTOP_LIMIT)
	{
		DesktopManager.SwitchDesktop(hotkey);
	} else
	{
		var index = (Byte) (hotkey - DESKTOP_LIMIT);
		DesktopManager.MoveActiveWindow(index);
	}
}
