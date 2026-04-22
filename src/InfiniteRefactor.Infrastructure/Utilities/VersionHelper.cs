using System;

namespace AirMaster.Infrastructure.Utilities;
public static class VersionHelper
{
    public static bool TryParseVersion(string version, out Version parsed)
    {
        // 去掉前缀 "v"，只保留数字部分
        var clean = version.TrimStart('v');
        return Version.TryParse(clean, out parsed);
    }

    public static int CompareVersions(string v1, string v2)
    {
        if (TryParseVersion(v1, out var ver1) && TryParseVersion(v2, out var ver2))
            return ver1.CompareTo(ver2);
        return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase);
    }
}
