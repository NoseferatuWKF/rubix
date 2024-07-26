using System.Runtime.InteropServices;
using System.Text;

namespace WinAPI;

internal static class Guids
{
	internal static readonly Guid CLSID_ImmersiveShell = new Guid(
		"C2F03A33-21F5-47FA-B4BB-156362A2F239");
	internal static readonly Guid CLSID_VirtualDesktopManagerInternal = new Guid(
		"C5E0CDCA-7B6E-41B2-9FC4-D93975CC467B");
	internal static readonly Guid CLSID_VirtualDesktopManager = new Guid(
		"AA509086-5CA9-4C25-8F95-589D3C07B48A");
	internal static readonly Guid CLSID_VirtualDesktopPinnedApps = new Guid(
		"B5A399E7-1C87-46B8-88E9-FC5747B171BD");
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("372E1D3B-38D3-42E4-A15B-8AB2B178F513")]
internal interface IApplicationView
{
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("1841C6D7-4F9D-42C0-AF41-8747538F10E5")]
internal interface IApplicationViewCollection
{
	Int32 GetViews(out IObjectArray array);
	Int32 GetViewsByZOrder(out IObjectArray array);
	Int32 GetViewsByAppUserModelId(string id, out IObjectArray array);
	Int32 GetViewForHwnd(IntPtr hwnd, out IApplicationView view);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("3F07F4BE-B107-441A-AF0F-39D82529072C")]
internal interface IVirtualDesktop
{
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("53F5CA0B-158F-4124-900C-057158060B27")]
internal interface IVirtualDesktopManagerInternal
{
	Int32 GetCount();
	void MoveViewToDesktop(IApplicationView view, IVirtualDesktop desktop);
	Boolean CanViewMoveDesktops(IApplicationView view);
	IVirtualDesktop GetCurrentDesktop();
	void GetDesktops(out IObjectArray desktops);
	[PreserveSig]
	Int32 GetAdjacentDesktop(IVirtualDesktop from, Int32 direction, out IVirtualDesktop desktop);
	void SwitchDesktop(IVirtualDesktop desktop);
	IVirtualDesktop CreateDesktop();
	void RemoveDesktop(IVirtualDesktop desktop, IVirtualDesktop fallback);
}

// https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-ivirtualdesktopmanager
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("A5CD92FF-29BE-454C-8D04-D82879FB3F1B")]
internal interface IVirtualDesktopManager
{
}

// https://learn.microsoft.com/en-us/windows/win32/api/objectarray/nn-objectarray-iobjectarray
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9")]
internal interface IObjectArray
{
	void GetCount(out Int32 count);
	void GetAt(Int32 index, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out Object obj);
}

// https://learn.microsoft.com/en-us/windows/win32/api/servprov/nn-servprov-iserviceprovider
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
internal interface IServiceProvider
{
	[return: MarshalAs(UnmanagedType.IUnknown)]
	Object QueryService(ref Guid service, ref Guid riid);
}

internal enum MOD : UInt32
{
	ALT = 0x0001,
	SHIFT = 0x0004,
	NOREPEAT = 0x4000,
	HOTKEY = 0x0312,
}

internal sealed class User32
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct Msg 
	{
		internal IntPtr Hwnd;
		internal UInt32 Message;
		internal IntPtr WParam;
		internal IntPtr LParam;
	}

	internal delegate Boolean CallBackPtr(Int32 hwnd, Int32 lParam);

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
	[DllImport("user32.dll")]
	internal static extern Boolean RegisterHotKey(IntPtr hWnd, UInt32 id, MOD fsModifiers, UInt32 vk);

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
	internal static extern Boolean EnumWindows(CallBackPtr lpEnumFunc, IntPtr lParam);

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowtextlengtha
	[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern Int32 GetWindowTextLength(IntPtr hWnd);

	// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowtexta
	[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern Int32 GetWindowText(IntPtr hWnd, StringBuilder lpString, Int32 nMaxCount);
}

internal sealed class Kernel32
{
	// https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getcurrentthreadid
	[DllImport("kernel32.dll")]
	internal static extern UInt32 GetCurrentThreadId();
}
