using System.Runtime.InteropServices;
using System.Text;
using WinAPI;

namespace VDM;

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

	internal static IVirtualDesktop GetDesktop(int index)
	{
		var count = VirtualDesktopManagerInternal.GetCount();
		
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);

		var desktops = CacheManager.GetDesktops();
		object objdesktop;
		if (desktops == null)
		{
			VirtualDesktopManagerInternal.GetDesktops(out desktops);
			CacheManager.SetDesktops(desktops);
		}
		desktops.GetAt(index, typeof(IVirtualDesktop).GUID, out objdesktop);

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

	public sealed record WindowInformation(int Handle, string Title);

	private sealed class Desktop
	{
		private static IVirtualDesktop Instance;

		private Desktop(IVirtualDesktop desktop) 
		{
			Desktop.Instance = desktop; 
		}

		internal static Desktop FromIndex(int index)
		{
			return new Desktop(DesktopManager.GetDesktop(index));
		}

		internal void MakeVisible()
		{ 
			var programManager = CacheManager.GetProgramManager();
			var desktopThreadId = CacheManager.GetDesktopThreadId();
			int dummy;

			if (programManager == null)
			{
				programManager = FindFirstWindow("Program Manager");
				CacheManager.SetProgramManager(programManager);
			}

			if (desktopThreadId == 0)
			{
				desktopThreadId = User32.GetWindowThreadProcessId(
					new IntPtr(programManager.Handle), out dummy);
				CacheManager.SetDesktopThreadId(desktopThreadId);
			}

			var foregroundThreadId = User32.GetWindowThreadProcessId(
				User32.GetForegroundWindow(), out dummy);
			var currentThreadId = Kernel32.GetCurrentThreadId();

			// activate window in new virtual desktop
			if (foregroundThreadId != 0 &&
				foregroundThreadId != currentThreadId)
			{
				User32.AttachThreadInput(desktopThreadId, currentThreadId, true);
				User32.AttachThreadInput(foregroundThreadId, currentThreadId, true);
				User32.SetForegroundWindow(new IntPtr(programManager.Handle));
				User32.AttachThreadInput(foregroundThreadId, currentThreadId, false);
				User32.AttachThreadInput(desktopThreadId, currentThreadId, false);
			}

			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(Instance);
		}

		private static WindowInformation FindFirstWindow(string title)
		{
			var WindowInformationList = new List<WindowInformation>();
			User32.EnumWindows((hWnd, lParam) => 
			{
				int length = User32.GetWindowTextLength((IntPtr) hWnd);
				if (length > 0)
				{
					StringBuilder sb = new StringBuilder(length + 1);
					if (User32.GetWindowText((IntPtr) hWnd, sb, sb.Capacity) > 0)
					{ 
						WindowInformationList.Add(
							new WindowInformation(hWnd, sb.ToString()));
					}
				}
				return true;
			}, IntPtr.Zero);
			WindowInformation result = WindowInformationList.Find(
				x => x.Title == title);
			return result;
		}
	}
}

