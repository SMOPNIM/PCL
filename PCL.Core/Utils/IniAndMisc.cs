using System.Globalization;

namespace PCL;

/// <summary>
/// ini 文件读写（带运行时缓存）。
/// </summary>
public static class IniUtils {
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> IniCache = new();

    /// <summary>
    /// 清除某 ini 文件的运行时缓存。
    /// </summary>
    /// <param name="FileName">文件完整路径或简写文件名。简写将会使用“ApplicationName\文件名.ini”作为路径。</param>
    public static void IniClearCache(string FileName) {
        if (!FileName.Contains(":\\")) FileName = $@"{Paths.Base}PCL\{FileName}.ini";
        IniCache.TryRemove(FileName, out _);
    }

    /// <summary>
    /// 获取 ini 文件缓存。如果没有，则新读取 ini 文件内容。
    /// 在文件不存在或读取失败时返回 null。
    /// </summary>
    private static ConcurrentDictionary<string, string>? IniGetContent(string FileName) {
        try {
            //还原文件路径
            if (!FileName.Contains(":\\")) FileName = $@"{Paths.Base}PCL\{FileName}.ini";
            //检索缓存
            if (IniCache.TryGetValue(FileName, out var Cache)) return Cache;
            //读取文件
            if (!FileUtils.Exists(FileName)) return null;
            var Ini = new ConcurrentDictionary<string, string>();
            foreach (var Line in FileUtils.ReadAsLines(FileName, true)) {
                var Index = Line.IndexOfF(":");
                if (Index > 0) Ini[Line.Substring(0, Index)] = Line.Substring(Index + 1); //可能会有重复键，见 #3616
            }
            IniCache[FileName] = Ini;
            return Ini;
        } catch (Exception ex) {
            Logger.Error(ex, $"生成 ini 文件缓存失败（{FileName}）", LogBehavior.Toast);
            return null;
        }
    }

    /// <summary>
    /// 读取 ini 文件。这可能会使用到缓存。
    /// </summary>
    /// <param name="FileName">文件完整路径或简写文件名。简写将会使用“ApplicationName\文件名.ini”作为路径。</param>
    /// <param name="Key">键。</param>
    /// <param name="DefaultValue">没有找到键时返回的默认值。</param>
    public static string ReadIni(string FileName, string Key, string DefaultValue = "") {
        var Content = IniGetContent(FileName);
        if (Content is null) return DefaultValue;
        return Content.GetOrDefault(Key, DefaultValue);
    }

    /// <summary>
    /// 判断 ini 文件中是否包含某个键。这可能会使用到缓存。
    /// </summary>
    public static bool HasIniKey(string FileName, string Key) {
        var Content = IniGetContent(FileName);
        return Content is not null && Content.ContainsKey(Key);
    }

    /// <summary>
    /// 从 ini 文件中移除某个键。这会更新缓存。
    /// </summary>
    public static void DeleteIniKey(string FileName, string Key) {
        WriteIni(FileName, Key, null);
    }

    /// <summary>
    /// 写入 ini 文件，这会更新缓存。
    /// 若 Value 为 null，则删除该键。
    /// </summary>
    /// <param name="FileName">文件完整路径或简写文件名。简写将会使用“ApplicationName\文件名.ini”作为路径。</param>
    /// <param name="Key">键。</param>
    /// <param name="Value">值。</param>
    public static void WriteIni(string FileName, string Key, string? Value) {
        try {
            //预处理
            if (Key.Contains(":")) throw new Exception($"尝试写入 ini 文件 {FileName} 的键名中包含了冒号：{Key}");
            Key = Key.ReplaceLineEndings("");
            Value = Value?.ReplaceLineEndings("");
            //获取目前文件
            var Content = IniGetContent(FileName) ?? new ConcurrentDictionary<string, string>();
            //更新值
            if (Value is null) {
                if (!Content.TryRemove(Key, out _)) return; //无需处理
            } else {
                if (Content.TryGetValue(Key, out var ExistingValue) && ExistingValue == Value) return; //无需处理
                Content[Key] = Value;
            }
            //写入文件
            var FileContent = new StringBuilder();
            foreach (var Pair in Content) {
                FileContent.Append(Pair.Key);
                FileContent.Append(':');
                FileContent.Append(Pair.Value);
                FileContent.Append(Environment.NewLine);
            }
            if (!FileName.Contains(":\\")) FileName = $@"{Paths.Base}PCL\{FileName}.ini";
            //处理相对路径
            FileName = FileName.Contains(":\\") ? FileName : Paths.Base + FileName;
            FileUtils.Write(FileName, FileContent.ToString());
        } catch (Exception ex) {
            Logger.Error(ex, $"写入文件失败（{FileName} → {Key}:{Value}）", LogBehavior.Toast);
        }
    }
}

/// <summary>
/// 文件夹 I/O 权限检查。
/// </summary>
public static class PermissionUtils {
    /// <summary>
    /// 检查是否拥有某一文件夹的 I/O 权限。如果文件夹不存在，会返回 False。
    /// </summary>
    public static bool CheckPermission(string Folder) {
        try {
            if (string.IsNullOrEmpty(Folder)) return false;
            Folder = PathUtils.AddSlashSuffix(Folder);
            if (Folder.EndsWithF(":\\System Volume Information\\", true) || Folder.EndsWithF(":\\$RECYCLE.BIN\\", true)) return false;
            if (!DirectoryUtils.Exists(Folder)) return false;
            var TestFilePath = $@"{Folder}CheckPermission{UuidGenerator.GetUuid()}.txt";
            FileUtils.Write(TestFilePath, "临时文件，用于检测该文件夹的权限是否正常。");
            FileUtils.Delete(TestFilePath);
            return true;
        } catch (Exception ex) {
            Logger.Warn(ex, $"没有对文件夹 {Folder} 的权限，请尝试以管理员权限运行 PCL");
            return false;
        }
    }

    /// <summary>
    /// 检查是否拥有某一文件夹的 I/O 权限。如果出错，则抛出异常。
    /// </summary>
    public static void CheckPermissionWithException(string Folder) {
        if (string.IsNullOrWhiteSpace(Folder)) throw new ArgumentNullException("文件夹名不能为空！");
        Folder = PathUtils.AddSlashSuffix(Folder);
        if (Folder.EndsWithF(":\\System Volume Information\\", true) || Folder.EndsWithF(":\\$RECYCLE.BIN\\", true)) throw new UnauthorizedAccessException("没有对系统文件夹的权限！");
        if (!DirectoryUtils.Exists(Folder)) throw new DirectoryNotFoundException("文件夹不存在！");
        var TestFilePath = $@"{Folder}CheckPermission{UuidGenerator.GetUuid()}.txt";
        FileUtils.Write(TestFilePath, "临时文件，用于检测该文件夹的权限是否正常。");
        FileUtils.Delete(TestFilePath);
    }
}

/// <summary>
/// 文本辅助函数。
/// </summary>
public static class TextUtils {
    /// <summary>
    /// 不会报错的 Val。
    /// 如果输入有误，返回 0。
    /// </summary>
    public static double Val(object? Str) {
        if (Str is null) return 0;
        try {
            //非字符串先统一转为字符串（与 VB Conversion.Val 的类型转换行为一致）
            var s = Str is string str ? str : Convert.ToString(Str, CultureInfo.InvariantCulture);
            if (s == "&") return 0;
            //与 VB 的 Conversion.Val 行为一致：跳过前导空白，仅解析开头的符号、数字与小数点
            var Trimmed = s.Trim();
            var i = 0;
            var Sign = 1d;
            if (i < Trimmed.Length && (Trimmed[i] == '+' || Trimmed[i] == '-')) {
                if (Trimmed[i] == '-') Sign = -1;
                i++;
            }
            var Num = new StringBuilder();
            var HasDot = false;
            while (i < Trimmed.Length) {
                var c = Trimmed[i];
                if (char.IsDigit(c)) {
                    Num.Append(c);
                    i++;
                } else if (c == '.' && !HasDot) {
                    HasDot = true;
                    Num.Append('.');
                    i++;
                } else {
                    break;
                }
            }
            if (Num.Length == 0) return 0;
            if (Num.Length == 1 && Num[0] == '.') return 0;
            return Sign * double.Parse(Num.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
        } catch {
            return 0;
        }
    }
}

/// <summary>
/// 全程序内不会重复的数字（伪 Uuid）生成器。
/// </summary>
public static class UuidGenerator {
    private static int Uuid = 1;
    private static readonly object UuidLock = new();

    /// <summary>
    /// 获取一个全程序内不会重复的数字（伪 Uuid）。
    /// </summary>
    public static int GetUuid() {
        lock (UuidLock) {
            return ++Uuid;
        }
    }
}
