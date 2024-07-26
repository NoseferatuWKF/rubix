using System.Runtime.InteropServices;
using System.Text;
using WinAPI;

namespace VDM;

internal static class DesktopManager
{
	static DesktopManager()
	{
		var shell = 
			(WinAPI.IServiceProvider) Activator.CreateInstance(
				Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell));

		VirtualDesktopManagerInternal = 
			(IVirtualDesktopManagerInternal) shell.QueryService(
				Guids.CLSID_VirtualDesktopManagerInternal,
				typeof(IVirtualDesktopManagerInternal).GUID);

		VirtualDesktopManager = 
			(IVirtualDesktopManager) Activator.CreateInstance(
				Type.GetTypeFromCLSID(Guids.CLSID_VirtualDesktopManager));

		ApplicationViewCollection = 
			(IApplicationViewCollection) shell.QueryService(
				typeof(IApplicationViewCollection).GUID,
				typeof(IApplicationViewCollection).GUID);
	}

	internal static IVirtualDesktopManagerInternal VirtualDesktopManagerInternal;
	internal static IVirtualDesktopManager VirtualDesktopManager;
	internal static IApplicationViewCollection ApplicationViewCollection;

	internal static IVirtualDesktop GetDesktop(int index)
	{	// get desktop with index
		int count = VirtualDesktopManagerInternal.GetCount();
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);

		IObjectArray desktops;
		VirtualDesktopManagerInternal.GetDesktops(out desktops);
		object objdesktop;
		desktops.GetAt(index, typeof(IVirtualDesktop).GUID, out objdesktop);
		Marshal.ReleaseComObject(desktops);
		return (IVirtualDesktop) objdesktop;
	}

	internal static void SwitchDesktop(int index)
	{
		Desktop.FromIndex(index).MakeVisible();
	}

	internal static void MoveActiveWindow(int index)
	{
		var hWnd = User32.GetForegroundWindow();

		try 
		{
			ApplicationViewCollection.GetViewForHwnd(hWnd, out var view);
			VirtualDesktopManagerInternal.MoveViewToDesktop(view, GetDesktop(index));
		} catch (COMException)
		{
			// Element not found
			return;
		}
	}
}

internal sealed class Desktop
{
	private static IVirtualDesktop Instance;

	private Desktop(IVirtualDesktop desktop) 
	{
		Desktop.Instance = desktop; 
	}

	internal static Desktop FromIndex(int index)
	{ // return desktop object from index (-> index = 0..Count-1)
		return new Desktop(DesktopManager.GetDesktop(index));
	}

	private record WindowInformation(int Handle, string Title);

	internal void MakeVisible()
	{ 
		WindowInformation wi = FindFirstWindow("Program Manager");

		// activate desktop to prevent flashing icons in taskbar
		int dummy;
		uint DesktopThreadId = User32.GetWindowThreadProcessId(
			new IntPtr(wi.Handle), out dummy);
		uint ForegroundThreadId = User32.GetWindowThreadProcessId(
			User32.GetForegroundWindow(), out dummy);
		uint CurrentThreadId = Kernel32.GetCurrentThreadId();

		if ((DesktopThreadId != 0) &&
			(ForegroundThreadId != 0) &&
			(ForegroundThreadId != CurrentThreadId))
		{
			User32.AttachThreadInput(DesktopThreadId, CurrentThreadId, true);
			User32.AttachThreadInput(ForegroundThreadId, CurrentThreadId, true);
			User32.SetForegroundWindow(new IntPtr(wi.Handle));
			User32.AttachThreadInput(ForegroundThreadId, CurrentThreadId, false);
			User32.AttachThreadInput(DesktopThreadId, CurrentThreadId, false);
		}

		DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(Instance);
	}

	// prepare callback function for window enumeration
	private static User32.CallBackPtr callBackPtr = Callback;
	// list of window informations
	private static List<WindowInformation> WindowInformationList = 
		new List<WindowInformation>();

	// callback function for window enumeration
	private static bool Callback(int hWnd, int lparam)
	{
		int length = User32.GetWindowTextLength((IntPtr)hWnd);
		if (length > 0)
		{
			StringBuilder sb = new StringBuilder(length + 1);
			if (User32.GetWindowText((IntPtr)hWnd, sb, sb.Capacity) > 0)
			{ 
				WindowInformationList.Add(
					new WindowInformation(hWnd, sb.ToString()));
			}
		}
		return true;
	}

	// find first window with string in title
	private static WindowInformation FindFirstWindow(string title)
	{
		WindowInformationList = new List<WindowInformation>();
		User32.EnumWindows(callBackPtr, IntPtr.Zero);
		WindowInformation result = WindowInformationList.First(
			x => x.Title == title);
		return result;
	}
}
