namespace PCL;

/// <summary>
/// 输入验证规则基类。要查看所有的输入验证规则，可在输入 Validate 后查看自动补全。
/// </summary>
public abstract class Validate {
    /// <summary>
    /// 验证某字符串是否符合验证要求。若符合，返回空字符串；若不符合，返回错误原因；若需要中断检查并直接通过，返回 null。
    /// 注：VB 原实现中此方法名为 Validate，C# 不允许成员名与类名相同，故改为 Check。
    /// </summary>
    public abstract string? Check(string? Str);
}

/// <summary>
/// 若为空则直接通过检查。
/// </summary>
public class ValidateNullable : Validate {
    public override string? Check(string? Str) {
        if (string.IsNullOrEmpty(Str)) return null;
        return "";
    }
}

/// <summary>
/// 不能为 null 或空字符串（不包括全空格检查）。
/// </summary>
public class ValidateNullOrEmpty : Validate {
    public override string? Check(string? Str) {
        if (string.IsNullOrEmpty(Str)) return "输入内容不能为空！";
        return "";
    }
}

/// <summary>
/// 不能为 null 或空字符串（包括全空格检查）。
/// </summary>
public class ValidateNullOrWhiteSpace : Validate {
    public override string? Check(string? Str) {
        if (string.IsNullOrWhiteSpace(Str)) return "输入内容不能为空！";
        return "";
    }
}

/// <summary>
/// 自定义的 Validate。
/// </summary>
public class ValidateFunc : Validate {
    private readonly Func<string?, string?> _func;
    public ValidateFunc(Func<string?, string?> F) {
        _func = F;
    }
    public override string? Check(string? Str) {
        return _func(Str);
    }
}

/// <summary>
/// 必须满足正则表达式。
/// </summary>
public class ValidateRegex : Validate {
    public string? Regex { get; set; }
    public string ErrorDescription { get; set; } = "正则检查失败！";
    public ValidateRegex() { } //用于 XAML 初始化
    public ValidateRegex(string Regex, string? ErrorDescription = "正则检查失败！") {
        this.Regex = Regex;
        this.ErrorDescription = ErrorDescription;
    }
    public override string? Check(string? Str) {
        if (!Str.RegexCheck(Regex)) return ErrorDescription;
        return "";
    }
}

/// <summary>
/// 必须是一个完整网址。
/// </summary>
public class ValidateHttp : Validate {
    public ValidateHttp() { } //用于 XAML 初始化
    public override string? Check(string? Str) {
        if (Str.EndsWithF('/')) Str = Str.Substring(0, Str.Length - 1);
        if (!Str.RegexCheck("^(http[s]?)\\://")) return "输入的网址无效！";
        return "";
    }
}

/// <summary>
/// 必须为整数。
/// </summary>
public class ValidateInteger : Validate {
    public int Min { get; set; }
    public int Max { get; set; } = int.MaxValue;
    public ValidateInteger() { } //用于 XAML 初始化
    public ValidateInteger(int Min, int Max) {
        this.Min = Min;
        this.Max = Max;
    }
    public override string? Check(string? Str) {
        if (Str.Length > 9) return "请输入一个大小合理的数字！";
        if (!int.TryParse(Str, out var Valed)) return "请输入一个整数！";
        if (Valed > Max) return $"不可超过 {Max}！";
        if (Valed < Min) return $"不可低于 {Min}！";
        return "";
    }
}

/// <summary>
/// 长度限制。
/// </summary>
public class ValidateLength : Validate {
    public int Min { get; set; } = 0;
    public int Max { get; set; } = int.MaxValue;
    public ValidateLength() { } //用于 XAML 初始化
    public ValidateLength(int Min, int Max = int.MaxValue) {
        this.Min = Min;
        this.Max = Max;
    }
    public override string? Check(string? Str) {
        if (Str is null) Str = "";
        if (Str.Length != Max && Max == Min) return $"长度必须为 {Max} 个字符！";
        if (Str.Length > Max) return $"长度最长为 {Max} 个字符！";
        if (Str.Length < Min) return $"长度至少需 {Min} 个字符！";
        return "";
    }
}

/// <summary>
/// 不能包含某些特定字符串。忽略大小写。
/// </summary>
public class ValidateExcept : Validate {
    public Collection<string> Excepts { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public ValidateExcept() {
        ErrorMessage = "输入内容不能包含 %";
    } //用于 XAML 初始化
    public ValidateExcept(Collection<string> Excepts, string? ErrorMessage = "输入内容不能包含 %") {
        this.Excepts = Excepts;
        this.ErrorMessage = ErrorMessage;
    }
    public ValidateExcept(IEnumerable<string> Excepts, string? ErrorMessage = "输入内容不能包含 %") : this(new Collection<string>(Excepts.ToList()), ErrorMessage) {
    }
    public ValidateExcept(IEnumerable<char> Excepts, string? ErrorMessage = "输入内容不能包含 %") : this(Excepts.Select(c => c.ToString()).ToList(), ErrorMessage) {
    }
    public override string? Check(string? Str) {
        foreach (var Ch in Excepts) {
            if (Str.IndexOfF(Ch, true) >= 0) {
                ErrorMessage ??= "";
                return ErrorMessage.Replace("%", Ch);
            }
        }
        return "";
    }
}

/// <summary>
/// 不能与某些特定字符串相同。
/// </summary>
public class ValidateExceptSame : Validate {
    public Collection<string> Excepts { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public bool IgnoreCase { get; set; } = false;
    public ValidateExceptSame() { }
    public ValidateExceptSame(Collection<string> Excepts, string? ErrorMessage = "输入内容不能为 %", bool IgnoreCase = false) {
        this.Excepts = Excepts;
        this.ErrorMessage = ErrorMessage;
        this.IgnoreCase = IgnoreCase;
    }
    public ValidateExceptSame(IEnumerable<string> Excepts, string? ErrorMessage = "输入内容不能为 %", bool IgnoreCase = false) : this(new Collection<string>(Excepts.ToList()), ErrorMessage, IgnoreCase) {
    }
    public ValidateExceptSame(string Except, string? ErrorMessage = "输入内容不能为 %", bool IgnoreCase = false) : this(new Collection<string> { Except }, ErrorMessage, IgnoreCase) {
    }
    public override string? Check(string? Str) {
        if (Str is null) return ErrorMessage?.Replace("%", "null");
        foreach (var Ch in Excepts) {
            if (IgnoreCase) {
                if (Str.Lower() == Ch.Lower()) return ErrorMessage?.Replace("%", Ch);
            } else {
                if (Str.Equals(Ch)) return ErrorMessage?.Replace("%", Ch); //使用 = 不确定是否会忽略大小写
            }
        }
        return "";
    }
}

/// <summary>
/// 必须为特定字符串。
/// </summary>
public class ValidateSame : Validate {
    public string? Required { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IgnoreCase { get; set; } = false;
    public ValidateSame() { }
    public ValidateSame(string? Required, string? ErrorMessage = "输入内容有误", bool IgnoreCase = false) {
        this.Required = Required;
        this.ErrorMessage = ErrorMessage;
        this.IgnoreCase = IgnoreCase;
    }
    public override string? Check(string? Str) {
        return string.Compare(Str, Required, IgnoreCase) == 0 ? "" : ErrorMessage;
    }
}

/// <summary>
/// 对文件夹名的粗略的特化检测。
/// </summary>
public class ValidateFolderName : Validate {
    public string? Folder { get; set; }
    public bool UseMinecraftCharCheck { get; set; } = true;
    private readonly IEnumerable<string>? PathIgnore;
    private readonly List<string>? IgnoreList;
    public ValidateFolderName() { }
    public ValidateFolderName(string? Folder, bool UseMinecraftCharCheck = true, List<string>? IgnoreList = null) {
        this.Folder = Folder;
        this.UseMinecraftCharCheck = UseMinecraftCharCheck;
        try {
            PathIgnore = DirectoryUtils.EnumerateDirectories(Folder);
        } catch { }
        this.IgnoreList = IgnoreList;
    }
    public override string? Check(string? Str) {
        try {
            //检查是否为空
            var LengthCheck = new ValidateNullOrWhiteSpace().Check(Str);
            if (LengthCheck != "") return LengthCheck;
            //检查空格
            if (Str.StartsWithF(' ')) return "文件夹名不能以空格开头！";
            if (Str.EndsWithF(' ')) return "文件夹名不能以空格结尾！";
            //检查长度
            LengthCheck = new ValidateLength(1, 100).Check(Str);
            if (LengthCheck != "") return LengthCheck;
            //检查尾部小数点
            if (Str.EndsWithF('.')) return "文件夹名不能以小数点结尾！";
            //检查特殊字符
            var CharactCheck = new ValidateExcept(Path.GetInvalidFileNameChars() + (UseMinecraftCharCheck ? "!;" : ""), "文件夹名不可包含 % 字符！").Check(Str);
            if (CharactCheck != "") return CharactCheck;
            //检查特殊字符串
            var InvalidStrCheck = new ValidateExceptSame(new[] { "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" }, "文件夹名不可为 %！", true).Check(Str);
            if (InvalidStrCheck != "") return InvalidStrCheck;
            //检查 NTFS 8.3 文件名（#4505）
            if (Str.RegexCheck(".{2,}~\\d")) return "文件夹名不能包含这一特殊格式！";
            //检查文件夹重名
            var Arr = new List<string>();
            string folderName;
            if (PathIgnore != null) {
                foreach (var FolderPath in PathIgnore) {
                    folderName = PathUtils.GetLastPart(FolderPath);
                    if (IgnoreList == null || !IgnoreList.Contains(folderName, StringComparer.OrdinalIgnoreCase)) Arr.Add(folderName);
                }
            }
            var SameNameCheck = new ValidateExceptSame(Arr, "不可与现有文件夹重名！", true).Check(Str);
            if (SameNameCheck != "") return SameNameCheck;
            return "";
        } catch (Exception ex) {
            Logger.Warn(ex, "检查文件夹名出错");
            return "错误：" + ex.Message;
        }
    }
}

/// <summary>
/// 对文件名的粗略的特化检测。
/// </summary>
public class ValidateFileName : Validate {
    public string? Name { get; set; }
    public bool UseMinecraftCharCheck { get; set; } = true;
    public bool IgnoreCase { get; set; } = true;
    public string? ParentFolder { get; set; } = null;
    public bool RequireParentFolderExists { get; set; } = true;
    public ValidateFileName() { }
    public ValidateFileName(string? Name, bool UseMinecraftCharCheck = true, bool IgnoreCase = true) {
        this.Name = Name;
        this.IgnoreCase = IgnoreCase;
        this.UseMinecraftCharCheck = UseMinecraftCharCheck;
    }
    public override string? Check(string? Str) {
        try {
            //检查是否为空
            var LengthCheck = new ValidateNullOrWhiteSpace().Check(Str);
            if (LengthCheck != "") return LengthCheck;
            //检查空格
            if (Str.StartsWithF(' ')) return "文件名不能以空格开头！";
            if (Str.EndsWithF(' ')) return "文件名不能以空格结尾！";
            //检查长度
            LengthCheck = new ValidateLength(1, 253).Check(Str + (ParentFolder ?? ""));
            if (LengthCheck != "") return LengthCheck;
            //检查尾部小数点
            if (Str.EndsWithF('.')) return "文件名不能以小数点结尾！";
            //检查特殊字符
            var CharactCheck = new ValidateExcept(Path.GetInvalidFileNameChars() + (UseMinecraftCharCheck ? "!;" : ""), "文件名不可包含 % 字符！").Check(Str);
            if (CharactCheck != "") return CharactCheck;
            //检查特殊字符串
            var InvalidStrCheck = new ValidateExceptSame(new[] { "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" }, "文件名不可为 %！", true).Check(Str);
            if (InvalidStrCheck != "") return InvalidStrCheck;
            //检查 NTFS 8.3 文件名（#4505）
            if (Str.RegexCheck(".{2,}~\\d")) return "文件名不能包含这一特殊格式！";
            //检查文件重名
            if (ParentFolder != null) {
                var DirInfo = DirectoryUtils.GetInfo(ParentFolder);
                if (DirInfo.Exists) {
                    var SameNameCheck = new ValidateExceptSame(DirInfo.EnumerateFiles("*").Select(f => f.Name),
                        "不可与现有文件重名！", IgnoreCase).Check(Str);
                    if (SameNameCheck != "") return SameNameCheck;
                } else {
                    if (RequireParentFolderExists) return $"父文件夹不存在：{ParentFolder}";
                }
            }
            return "";
        } catch (Exception ex) {
            Logger.Warn(ex, "检查文件名出错");
            return "错误：" + ex.Message;
        }
    }
}

/// <summary>
/// 要求输入一个可用的文件夹路径。
/// </summary>
public class ValidateFolderPath : Validate {
    public bool UseMinecraftCharCheck { get; set; } = true;
    public ValidateFolderPath() { }
    public ValidateFolderPath(bool UseMinecraftCharCheck) {
        this.UseMinecraftCharCheck = UseMinecraftCharCheck;
    }
    public override string? Check(string? Str) {
        //去除尾部斜线，统一为 \
        Str = Str.Replace('/', '\\');
        if (!PathUtils.RemoveSlashSuffix(Str).EndsWithF(':')) Str = PathUtils.RemoveSlashSuffix(Str);
        //检查是否为空
        var LengthCheck = new ValidateNullOrWhiteSpace().Check(Str);
        if (LengthCheck != "") return LengthCheck;
        //检查长度
        LengthCheck = new ValidateLength(1, 254).Check(Str);
        if (LengthCheck != "") return LengthCheck;
        //检查 NTFS 8.3 文件名（#4505）
        var Match = Str.RegexSeek(".{2,}~\\d");
        if (Match != null) return $"文件夹名不能包含 {Match} 这一特殊格式！";
        //检查开头
        if (Str.StartsWithF("\\Mac\\")) goto Fin;
        foreach (var Drive in Environment.GetLogicalDrives().Where(p => Directory.Exists(p))) {
            var DriveName = Drive.Upper()[0] + ":\\";
            if (Str.Upper() == DriveName) return "";
            if (Str.StartsWithF(DriveName, true)) goto Fin;
        }
        return "文件夹路径头存在错误！";
    Fin:
        //对首层以外的路径检查
        var Parts = Str.Split('\\');
        for (var i = Str.StartsWithF("\\Mac\\") ? 2 : 1; i < Parts.Length; i++) {
            var SubStr = Parts[i];
            //检查是否为空
            var SubLengthCheck = new ValidateNullOrWhiteSpace().Check(SubStr);
            if (SubLengthCheck != "") return "文件夹路径存在错误！";
            //检查特殊字符
            var CharactCheck = new ValidateExcept(Path.GetInvalidFileNameChars() + (UseMinecraftCharCheck ? "!;" : ""), "路径中存在无效字符！").Check(SubStr);
            if (CharactCheck != "") return CharactCheck;
            //检查头部空格
            if (SubStr.StartsWithF(' ')) return "文件夹名不能以空格开头！";
            if (SubStr.EndsWithF(' ')) return "文件夹名不能以空格结尾！";
            //检查尾部小数点
            if (SubStr.EndsWithF('.')) return "文件夹名不能以小数点结尾！";
            //检查特殊字符串
            var InvalidStrCheck = new ValidateExceptSame(new[] { "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" }, "文件夹名不可为 %！").Check(SubStr);
            if (InvalidStrCheck != "") return InvalidStrCheck;
        }
        return "";
    }
}
