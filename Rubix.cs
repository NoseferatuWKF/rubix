using System.Runtime.InteropServices;
using WinAPI;

namespace Rubix;

internal static class DesktopManager
{
	internal static readonly IVirtualDesktopManagerInternal VirtualDesktopManagerInternal;
	internal static readonly IVirtualDesktopManager VirtualDesktopManager;
	internal static readonly IApplicationViewCollection ApplicationViewCollection;

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

	internal static IVirtualDesktop GetDesktop(Byte index)
	{
		var desktops = CacheManager.Desktops;
		IVirtualDesktop objdesktop;
		if (desktops == null)
		{
			VirtualDesktopManagerInternal.GetDesktops(out desktops);
			CacheManager.Desktops = desktops;
		}
		desktops.GetAt(index, typeof(IVirtualDesktop).GUID, out objdesktop);

		return objdesktop;
	}

	internal static void SwitchDesktop(Byte index)
	{
		Desktop.FromIndex(index).MakeVisible();
	}

	internal static void MoveActiveWindow(Byte index)
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

	private sealed class Desktop
	{
		private static IVirtualDesktop Instance;

		private Desktop(IVirtualDesktop desktop) 
		{
			Desktop.Instance = desktop; 
		}

		internal static Desktop FromIndex(Byte index)
		{
			return new Desktop(DesktopManager.GetDesktop(index));
		}

		internal void MakeVisible()
		{ 
			var programManager = CacheManager.ProgramManager;

			if (programManager == null)
			{
				programManager = LocateProgramManager();
			}

			var desktopThreadId = programManager.ThreadId;
			var foregroundThreadId = User32.GetWindowThreadProcessId(
				User32.GetForegroundWindow(), out var _lpdwProcessId);
			var currentThreadId = Kernel32.GetCurrentThreadId();

			// activate window in new virtual desktop
			if (foregroundThreadId != 0 &&
				foregroundThreadId != currentThreadId)
			{
				User32.AttachThreadInput(desktopThreadId, currentThreadId, true);
				User32.AttachThreadInput(foregroundThreadId, currentThreadId, true);
				User32.SetForegroundWindow(programManager.Handle);
				User32.AttachThreadInput(foregroundThreadId, currentThreadId, false);
				User32.AttachThreadInput(desktopThreadId, currentThreadId, false);
			}

			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(Instance);
		}

		// set cache and return
		private static ProgramManager LocateProgramManager()
		{
			var hWnd = User32.FindWindow("Progman", null);
			var desktopThreadId = User32.GetWindowThreadProcessId(
					hWnd, out var _lpdwProcessId);
			var programManager = new ProgramManager(
					hWnd, desktopThreadId);
			CacheManager.ProgramManager = programManager;

			return programManager;
		}
	}
}

