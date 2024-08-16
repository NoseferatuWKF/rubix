using WinAPI;

namespace VDM;

internal static class CacheManager
{
	private static IObjectArray _desktops;
	private static uint _desktopThreadId;
	private static DesktopManager.WindowInformation _programManager;

	public static IObjectArray GetDesktops() =>
		_desktops;

	public static void SetDesktops(IObjectArray desktops) =>
		_desktops = desktops;

	public static uint GetDesktopThreadId() =>
		_desktopThreadId;

	public static void SetDesktopThreadId(uint desktopThreadId) =>
		_desktopThreadId = desktopThreadId;

	public static DesktopManager.WindowInformation GetProgramManager() =>
		_programManager;

	public static void SetProgramManager(DesktopManager.WindowInformation programManager) =>
		_programManager = programManager;
}
