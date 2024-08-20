using WinAPI;

namespace VDM;

internal static class CacheManager
{
	internal static IObjectArray Desktops { get; set; }
	internal static ProgramManager ProgramManager { get; set; }
}

internal sealed record ProgramManager(Int32 Handle, UInt32 ThreadId);
