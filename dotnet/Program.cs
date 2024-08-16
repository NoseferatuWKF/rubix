using VDM;
using WinAPI;

const UInt16 ASCII_OFFSET = 48; // 0
const UInt16 DESKTOP_LIMIT = 9;

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
	DesktopManager.VirtualDesktopManagerInternal.RemoveDesktop(
		DesktopManager.GetDesktop(desktops - 1),
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
while (User32.GetMessage(out var msg, IntPtr.Zero, MOD.HOTKEY, MOD.HOTKEY))
{
	var hotkey = (Int32) (msg.WParam - 1);
	if (hotkey < DESKTOP_LIMIT)
	{
		DesktopManager.SwitchDesktop(hotkey);
	} else
	{
		DesktopManager.MoveActiveWindow(hotkey - DESKTOP_LIMIT);
	}
}
