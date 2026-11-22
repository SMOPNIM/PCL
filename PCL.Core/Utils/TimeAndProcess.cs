using System.Diagnostics;

namespace PCL;

/// <summary>
/// 时间相关辅助函数。
/// </summary>
public static class TimeUtils {
    /// <summary>
    /// 获取一个单调递增时间值（毫秒）。
    /// </summary>
    public static long GetTimeMs() {
        return Stopwatch.GetTimestamp() / (Stopwatch.Frequency / 1000L);
    }

    /// <summary>
    /// 获取十进制 Unix 时间戳。
    /// </summary>
    public static long GetUnixTimestampUtc() {
        return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000L) / 10000000;
    }

    /// <summary>
    /// 获取十进制 Unix 时间戳。
    /// </summary>
    public static long ToUnixTimestampUtc(DateTime Time) {
        return (Time.ToUniversalTime().Ticks - 621355968000000000L) / 10000000;
    }

    /// <summary>
    /// 时间戳转化为日期。
    /// </summary>
    public static DateTime GetDate(int timeStamp) {
        return DateTimeOffset.FromUnixTimeSeconds(timeStamp).LocalDateTime;
    }

    /// <summary>
    /// 将 UTC 时间转化为当前时区的时间。
    /// </summary>
    public static DateTime GetLocalTime(DateTime UtcDate) {
        return DateTime.SpecifyKind(UtcDate, DateTimeKind.Utc).ToLocalTime();
    }
}

/// <summary>
/// 进程启动辅助。
/// </summary>
public static class ProcessUtils {
    /// <summary>
    /// 启动进程并返回进程实例，若失败则抛出异常。
    /// 会自动进行长路径处理。
    /// </summary>
    public static Process StartProcess(string FileName, string Arguments = "") {
        return StartProcess(new ProcessStartInfo { FileName = FileName, Arguments = Arguments });
    }

    /// <summary>
    /// 启动进程并返回进程实例，若失败则抛出异常。
    /// 会自动进行长路径处理。
    /// </summary>
    public static Process StartProcess(ProcessStartInfo StartInfo) {
        StartInfo.FileName = PathUtils.ToShortPath(StartInfo.FileName);
        if (!string.IsNullOrEmpty(StartInfo.WorkingDirectory)) StartInfo.WorkingDirectory = PathUtils.ToShortPath(StartInfo.WorkingDirectory);
        Logger.Info($"启动进程：{StartInfo.FileName} {StartInfo.Arguments}");
        return Process.Start(StartInfo);
    }
}

/// <summary>
/// 指示接取到这个异常的函数进行重试。
/// </summary>
public class RestartException : Exception {
}
