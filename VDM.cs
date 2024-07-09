using System.Runtime.InteropServices;
using System.Text;

namespace VDM;

internal static class Guids
{
	public static readonly Guid CLSID_ImmersiveShell = new Guid("C2F03A33-21F5-47FA-B4BB-156362A2F239");
	public static readonly Guid CLSID_VirtualDesktopManagerInternal = new Guid("C5E0CDCA-7B6E-41B2-9FC4-D93975CC467B");
	public static readonly Guid CLSID_VirtualDesktopManager = new Guid("AA509086-5CA9-4C25-8F95-589D3C07B48A");
	public static readonly Guid CLSID_VirtualDesktopPinnedApps = new Guid("B5A399E7-1C87-46B8-88E9-FC5747B171BD");
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
[Guid("372E1D3B-38D3-42E4-A15B-8AB2B178F513")]
internal interface IApplicationView
{
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("3F07F4BE-B107-441A-AF0F-39D82529072C")]
internal interface IVirtualDesktop
{
	bool IsViewVisible(IApplicationView view);
	Guid GetId();
	[return: MarshalAs(UnmanagedType.HString)]
	string GetName();
	[return: MarshalAs(UnmanagedType.HString)]
	string GetWallpaperPath();
	bool IsRemote();
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("53F5CA0B-158F-4124-900C-057158060B27")]
internal interface IVirtualDesktopManagerInternal
{
	int GetCount();
	void MoveViewToDesktop(IApplicationView view, IVirtualDesktop desktop);
	bool CanViewMoveDesktops(IApplicationView view);
	IVirtualDesktop GetCurrentDesktop();
	void GetDesktops(out IObjectArray desktops);
	[PreserveSig]
	int GetAdjacentDesktop(IVirtualDesktop from, int direction, out IVirtualDesktop desktop);
	void SwitchDesktop(IVirtualDesktop desktop);
	IVirtualDesktop CreateDesktop();
	void MoveDesktop(IVirtualDesktop desktop, int nIndex);
	void RemoveDesktop(IVirtualDesktop desktop, IVirtualDesktop fallback);
	IVirtualDesktop FindDesktop(ref Guid desktopid);
	void GetDesktopSwitchIncludeExcludeViews(IVirtualDesktop desktop, out IObjectArray unknown1, out IObjectArray unknown2);
	void SetDesktopName(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string name);
	void SetDesktopWallpaper(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string path);
	void UpdateWallpaperPathForAllDesktops([MarshalAs(UnmanagedType.HString)] string path);
	void CopyDesktopState(IApplicationView pView0, IApplicationView pView1);
	void CreateRemoteDesktop([MarshalAs(UnmanagedType.HString)] string path, out IVirtualDesktop desktop);
	void SwitchRemoteDesktop(IVirtualDesktop desktop, IntPtr switchtype);
	void SwitchDesktopWithAnimation(IVirtualDesktop desktop);
	void GetLastActiveDesktop(out IVirtualDesktop desktop);
	void WaitForAnimationToComplete();
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("A5CD92FF-29BE-454C-8D04-D82879FB3F1B")]
internal interface IVirtualDesktopManager
{
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9")]
internal interface IObjectArray
{
	void GetCount(out int count);
	void GetAt(int index, ref Guid iid, [MarshalAs(UnmanagedType.Interface)]out object obj);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
internal interface IServiceProvider10
{
	[return: MarshalAs(UnmanagedType.IUnknown)]
	object QueryService(ref Guid service, ref Guid riid);
}

internal static class DesktopManager
{
	static DesktopManager()
	{
		var shell = (IServiceProvider10)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell));
		VirtualDesktopManagerInternal = (IVirtualDesktopManagerInternal)shell.QueryService(
				Guids.CLSID_VirtualDesktopManagerInternal,
				typeof(IVirtualDesktopManagerInternal).GUID);
		VirtualDesktopManager = (IVirtualDesktopManager)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_VirtualDesktopManager));
	}

	internal static IVirtualDesktopManagerInternal VirtualDesktopManagerInternal;
	internal static IVirtualDesktopManager VirtualDesktopManager;

	internal static IVirtualDesktop GetDesktop(int index)
	{	// get desktop with index
		int count = VirtualDesktopManagerInternal.GetCount();
		if (index < 0 || index >= count) throw new ArgumentOutOfRangeException("index");
		IObjectArray desktops;
		VirtualDesktopManagerInternal.GetDesktops(out desktops);
		object objdesktop;
		desktops.GetAt(index, typeof(IVirtualDesktop).GUID, out objdesktop);
		Marshal.ReleaseComObject(desktops);
		return (IVirtualDesktop)objdesktop;
	}
}

public class WindowInformation
{ // stores window informations
	public string Title { get; set; }
	public int Handle { get; set; }
}

public class Desktop
{
	// get process id to window handle
	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

	// get thread id of current process
	[DllImport("kernel32.dll")]
	static extern uint GetCurrentThreadId();

	// attach input to thread
	[DllImport("user32.dll")]
	static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

	// get handle of active window
	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	// try to set foreground window
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]static extern bool SetForegroundWindow(IntPtr hWnd);

	private static readonly Guid AppOnAllDesktops = new Guid("BB64D5B7-4DE3-4AB2-A87C-DB7601AEA7DC");
	private static readonly Guid WindowOnAllDesktops = new Guid("C2DDEA68-66F2-4CF9-8264-1BFD00FBBBAC");

	private IVirtualDesktop ivd;
	private Desktop(IVirtualDesktop desktop) { this.ivd = desktop; }

	public static Desktop FromIndex(int index)
	{ // return desktop object from index (-> index = 0..Count-1)
		return new Desktop(DesktopManager.GetDesktop(index));
	}

	public static Desktop Create()
	{ // create a new desktop
		return new Desktop(DesktopManager.VirtualDesktopManagerInternal.CreateDesktop());
	}

	public void MakeVisible()
	{ 
		WindowInformation wi = FindWindow("Program Manager");

		// activate desktop to prevent flashing icons in taskbar
		int dummy;
		uint DesktopThreadId = GetWindowThreadProcessId(new IntPtr(wi.Handle), out dummy);
		uint ForegroundThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out dummy);
		uint CurrentThreadId = GetCurrentThreadId();

		if ((DesktopThreadId != 0) && (ForegroundThreadId != 0) && (ForegroundThreadId != CurrentThreadId))
		{
			AttachThreadInput(DesktopThreadId, CurrentThreadId, true);
			AttachThreadInput(ForegroundThreadId, CurrentThreadId, true);
			SetForegroundWindow(new IntPtr(wi.Handle));
			AttachThreadInput(ForegroundThreadId, CurrentThreadId, false);
			AttachThreadInput(DesktopThreadId, CurrentThreadId, false);
		}

		DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(ivd);
	}

	// prepare callback function for window enumeration
	private delegate bool CallBackPtr(int hwnd, int lParam);
	private static CallBackPtr callBackPtr = Callback;
	// list of window informations
	private static List<WindowInformation> WindowInformationList = new List<WindowInformation>();

	// enumerate windows
	[DllImport("User32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EnumWindows(CallBackPtr lpEnumFunc, IntPtr lParam);

	// get window title length
	[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowTextLength(IntPtr hWnd);

	// get window title
	[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	// callback function for window enumeration
	private static bool Callback(int hWnd, int lparam)
	{
		int length = GetWindowTextLength((IntPtr)hWnd);
		if (length > 0)
		{
			StringBuilder sb = new StringBuilder(length + 1);
			if (GetWindowText((IntPtr)hWnd, sb, sb.Capacity) > 0)
			{ WindowInformationList.Add(new WindowInformation {Handle = hWnd, Title = sb.ToString()}); }
		}
		return true;
	}

	// find first window with string in title
	public static WindowInformation FindWindow(string WindowTitle)
	{
		WindowInformationList = new List<WindowInformation>();
		EnumWindows(callBackPtr, IntPtr.Zero);
		WindowInformation result = WindowInformationList.Find(x => x.Title.IndexOf(WindowTitle, StringComparison.OrdinalIgnoreCase) >= 0);
		return result;
	}
}