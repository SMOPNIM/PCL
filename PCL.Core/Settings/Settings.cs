using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PCL;

/// <summary>
/// 提供访问设置所需的最低信息的版本实例接口。
/// 在 UI 层由 McInstance 等具体类型实现。
/// </summary>
public interface IVersionInstance {
    /// <summary>
    /// 版本文件夹的完整路径（含尾部反斜线）。
    /// </summary>
    string PathVersion { get; }
}

/// <summary>
/// 单个设置项的定义与运行时缓存。
/// </summary>
public class Setting {
    public string? Key;
    public bool Encrypted;
    public object? DefaultValue;
    public Settings.Sources Source;
    /// <summary>
    /// 当设置项的值实际被改变时触发，参数为新值。
    /// </summary>
    public Action<object>? OnChanged;
    /// <summary>
    /// 该设置的值的实际类型。
    /// </summary>
    public Type? Type;
    /// <summary>
    /// 读取过的设置的缓存，若从未读取过则不包含对应键。
    /// 若为版本独立设置，键为版本路径；否则，键为空字符串。
    /// </summary>
    public ConcurrentDictionary<string, object?> ValueCache { get; } = new();

    public Setting(string Key, object? Value, Settings.Sources Source = Settings.Sources.Normal, bool Encrypted = false, Action<object>? OnChanged = null) {
        try {
            this.Key = Key;
            DefaultValue = Value;
            this.Encrypted = Encrypted;
            this.Source = Source;
            Type = (Value ?? "").GetType();
            this.OnChanged = OnChanged;
        } catch (Exception ex) {
            Logger.Error(ex, "初始化设置项失败"); // #5095 的 fallback
        }
    }

    /// <summary>
    /// 获取缓存中的值。若从未读取过则返回 null。
    /// </summary>
    public object? GetCache(IVersionInstance? Instance) {
        var Key2 = Source == Settings.Sources.Instance ? Instance!.PathVersion : "";
        return ValueCache.TryGetValue(Key2, out var Cached) ? Cached : null;
    }

    /// <summary>
    /// 更新缓存中的值。
    /// </summary>
    public void Set(object? Value, IVersionInstance? Instance) {
        var Key2 = Source == Settings.Sources.Instance ? Instance!.PathVersion : "";
        ValueCache[Key2] = Value;
    }

    /// <summary>
    /// 立即将当前的 Value 写入对应的注册表或文件。
    /// </summary>
    public void Save(IVersionInstance? Instance = null) {
        //与 VB 的隐式转换行为一致：对任意类型统一转为字符串
        var Value = GetCache(Instance)?.ToString() ?? "";
        Logger.Trace($"保存设置：{Key} {(Encrypted ? "" : "= " + Value)}{(Instance is null ? "" : $"（实例：{Instance.PathVersion}）")}");
        if (Encrypted) {
            try {
                Value = SettingCrypto.DesEncrypt(Value, "PCL" + Settings.Identify);
            } catch (Exception ex) {
                Logger.Warn(ex, $"加密设置失败：{Key}");
            }
        }
        switch (Source) {
            case Settings.Sources.Normal:
                IniUtils.WriteIni("Setup", Key, Value);
                break;
            case Settings.Sources.Registry:
                try {
                    RegistryUtils.Write($"HKEY_CURRENT_USER\\Software\\{Settings.RegFolder}", Key, Value);
                } catch (Exception ex) {
                    Logger.Warn(ex, $"写入注册表失败（{Key} → {Value}）", LogBehavior.Toast);
                }
                break;
            case Settings.Sources.Instance:
                if (Instance is null) throw new Exception($"保存版本独立设置 {Key} 时未提供目标版本");
                IniUtils.WriteIni(Instance.PathVersion + @"PCL\Setup.ini", Key, Value);
                break;
        }
    }
}

/// <summary>
/// 程序设置项的存取入口。
/// 设置项的元数据由启动代码（VB 的 ModSettings.Init）在首次读取前通过 Register 注册，
/// 因为其中包含依赖 UI 层的 OnChanged 回调与登录类型等主工程类型。
/// </summary>
public class Settings {
    /// <summary>
    /// 设置项的存储来源。
    /// </summary>
    public enum Sources {
        /// <summary>常规设置（PCL 目录下的 Setup.ini）。</summary>
        Normal,
        /// <summary>注册表设置（HKCU\Software\{RegFolder}）。</summary>
        Registry,
        /// <summary>版本独立设置（版本文件夹下的 PCL\Setup.ini）。</summary>
        Instance,
    }

    /// <summary>
    /// 全部已注册的设置项。
    /// </summary>
    public static readonly Dictionary<string, Setting> Entries = new();

    /// <summary>
    /// 设备识别码。用于加密设置项的 DES 密钥。由启动代码回填。
    /// </summary>
    public static string? Identify { get; set; } = "";

    /// <summary>
    /// 注册表设置项所在的键名（HKCU\Software\{RegFolder}）。由启动代码回填。
    /// </summary>
    public static string? RegFolder { get; set; } = "PCL";

    /// <summary>
    /// 注册一个设置项。
    /// </summary>
    public static void Register(Setting Entry) {
        Entries[Entry.Key] = Entry;
    }

    /// <summary>
    /// 改变某个设置项的值。
    /// </summary>
    public static void Set(string Key, object? Value, IVersionInstance? Instance = null) {
        if (!Entries.TryGetValue(Key, out var Entry)) throw new KeyNotFoundException("未找到设置项：" + Key);
        try {
            //判断是否一致
            Value = ConvertDynamic(Value, Entry.Type);
            if (Entry.GetCache(Instance) is null) Get(Key, Instance);
            if (EqualsDynamic(Entry.GetCache(Instance), Value)) return;
            //设置新值
            Entry.Set(Value, Instance);
            Entry.Save(Instance);
            //触发改变事件（必须在保存之后，以保证 VersionServerLogin 之类的在事件中读取到的是最新的值）
            Entry.OnChanged?.Invoke(Value!);
        } catch (Exception ex) {
            Logger.Error(ex, $"设置设置项时出错（{Key}, {Value}）");
        }
    }

    /// <summary>
    /// 写入某个未经加密的设置项。
    /// 若该设置项经过了加密，则会抛出异常。
    /// </summary>
    public static void SetSafe(string Key, object? Value, IVersionInstance? Instance = null) {
        if (!Entries.TryGetValue(Key, out var Entry)) throw new KeyNotFoundException("未找到设置项：" + Key);
        if (Entry.Encrypted) throw new InvalidOperationException("禁止写入加密设置项：" + Key);
        Set(Key, Value, Instance);
    }

    /// <summary>
    /// 获取某个设置项的值。
    /// </summary>
    public static object? Get(string Key, IVersionInstance? Instance = null) {
        if (!Entries.TryGetValue(Key, out var Entry)) throw new KeyNotFoundException("未找到设置项：" + Key);
        var Value = Entry.GetCache(Instance);
        if (Value is not null) return Value;
        //对部分设置强制赋值
        var ForcedSettings = new Dictionary<string, object?> {
            { "UiHiddenPageLink", true },
            { "UiHiddenSetupLink", true }
        };
        if (ForcedSettings.TryGetValue(Key, out var ForcedSetting)) {
            Entry.Set(ForcedSetting, Instance);
            return ForcedSetting;
        }
        //正常读取
        try {
            string GotValue; //先用 String 储存，避免类型转换
            var DefaultValue = Entry.Encrypted ? SettingCrypto.DesEncrypt(Entry.DefaultValue.ToString(), "PCL" + Identify) : Entry.DefaultValue.ToString();
            switch (Entry.Source) {
                case Sources.Normal:
                    GotValue = IniUtils.ReadIni("Setup", Key, DefaultValue);
                    break;
                case Sources.Registry:
                    GotValue = RegistryUtils.TryRead($"HKEY_CURRENT_USER\\Software\\{RegFolder}", Key, (object?)DefaultValue) as string ?? DefaultValue;
                    break;
                case Sources.Instance:
                    if (Instance is null) throw new Exception($"读取版本设置 {Key} 时未提供目标版本");
                    GotValue = IniUtils.ReadIni(Instance.PathVersion + @"PCL\Setup.ini", Key, DefaultValue);
                    break;
                default:
                    throw new InvalidOperationException($"未知的设置项来源：{Entry.Source}");
            }
            if (Entry.Encrypted) {
                if (GotValue == DefaultValue) {
                    GotValue = Entry.DefaultValue.ToString();
                } else {
                    try {
                        GotValue = SettingCrypto.DesDecrypt(GotValue, "PCL" + Identify);
                    } catch (Exception ex) {
                        Logger.Warn(ex, $"解密设置失败：{Key}");
                        GotValue = Entry.DefaultValue.ToString();
                        Entry.Set(Entry.DefaultValue, Instance);
                        Entry.Save(Instance);
                    }
                }
            }
            Entry.Set(ConvertDynamic(GotValue, Entry.Type), Instance);
        } catch (Exception ex) {
            Logger.Error(ex, $"读取设置失败：{Key}", LogBehavior.Toast);
            Entry.Set(ConvertDynamic(Entry.DefaultValue, Entry.Type), Instance);
        }
        return Entry.GetCache(Instance);
    }

    /// <summary>
    /// 获取某个设置项的值（指定类型）。
    /// </summary>
    public static T Get<T>(string Key, IVersionInstance? Instance = null) {
        var Value = ConvertDynamic(Get(Key, Instance), typeof(T));
        return Value is null ? default! : (T)Value;
    }

    /// <summary>
    /// 获取某个未经加密的设置项的值。
    /// 若该设置项经过了加密，则会抛出异常。
    /// </summary>
    public static object? GetSafe(string Key, IVersionInstance? Instance = null) {
        if (!Entries.TryGetValue(Key, out var Entry)) throw new KeyNotFoundException("未找到设置项：" + Key);
        if (Entry.Encrypted) throw new InvalidOperationException("禁止读取加密设置项：" + Key);
        return Get(Key, Instance);
    }

    /// <summary>
    /// 初始化某个设置项的值。
    /// </summary>
    public static void Reset(string Key, IVersionInstance? Instance = null) {
        if (!Entries.TryGetValue(Key, out var Entry)) throw new KeyNotFoundException("未找到设置项：" + Key);
        try {
            //判断是否一致
            if (Entry.GetCache(Instance) is null) Get(Key, Instance);
            if (EqualsDynamic(Entry.GetCache(Instance), Entry.DefaultValue)) return;
            //设置新值
            Entry.Set(Entry.DefaultValue, Instance);
            switch (Entry.Source) {
                case Sources.Normal:
                    IniUtils.DeleteIniKey("Setup", Key);
                    break;
                case Sources.Registry:
                    try {
                        RegistryUtils.Delete($"HKEY_CURRENT_USER\\Software\\{RegFolder}", Key);
                    } catch (Exception ex) {
                        Logger.Warn(ex, $"删除注册表失败（{Key}）", LogBehavior.Toast);
                    }
                    break;
                case Sources.Instance:
                    if (Instance is null) throw new Exception($"重置版本设置 {Key} 时未提供目标版本");
                    IniUtils.DeleteIniKey(Instance.PathVersion + @"PCL\Setup.ini", Key);
                    break;
            }
            //触发改变事件
            Entry.OnChanged?.Invoke(Entry.DefaultValue!);
        } catch (Exception ex) {
            Logger.Error(ex, $"重置设置项时出错（{Key}）");
        }
    }

    /// <summary>
    /// 获取某个设置项的默认值。
    /// </summary>
    public static string? GetDefault(string Key) {
        return Entries[Key].DefaultValue?.ToString();
    }

    /// <summary>
    /// 对应的注册表或文件是否已经储存了某个设置项。
    /// 若改成了和默认值一样的，在 2.12.3- 会储存更改，在 2.12.4+ 不会储存更改。
    /// </summary>
    public static bool HasSaved(string Key, IVersionInstance? Instance = null) {
        switch (Entries[Key].Source) {
            case Sources.Normal:
                return IniUtils.HasIniKey("Setup", Key);
            case Sources.Registry:
                try {
                    return RegistryUtils.Has($"HKEY_CURRENT_USER\\Software\\{RegFolder}", Key);
                } catch (Exception ex) {
                    Logger.Warn(ex, $"判断注册表 {Key} 是否存在时出错");
                    return false;
                }
            default: //Sources.Instance
                if (Instance is null) throw new Exception($"判断版本设置 {Key} 是否存在时未提供目标版本");
                return IniUtils.HasIniKey(Instance.PathVersion + @"PCL\Setup.ini", Key);
        }
    }

    /// <summary>
    /// 与 VB 的 CTypeDynamic 语义一致的类型转换。
    /// 无法转换时返回 null（而非抛出 InvalidCastException）。
    /// </summary>
    internal static object? ConvertDynamic(object? Value, Type? Type) {
        if (Value is null || Type is null) return null;
        if (Type.IsInstanceOfType(Value)) return Value;
        try {
            //枚举：与 VB 的 CType 语义一致，支持按名称或数值解析字符串
            if (Type.IsEnum) {
                if (Value is string EnumText) return Enum.Parse(Type, EnumText, true);
                return Enum.ToObject(Type, Value);
            }
            return Convert.ChangeType(Value, Type, CultureInfo.InvariantCulture);
        } catch {
            return null;
        }
    }

    /// <summary>
    /// 与 VB 的对象相等比较语义一致。
    /// </summary>
    internal static bool EqualsDynamic(object? A, object? B) {
        if (A is null || B is null) return ReferenceEquals(A, B);
        return A.Equals(B);
    }
}

/// <summary>
/// 设置项的 DES 对称加解密。
/// </summary>
internal static class SettingCrypto {
    private static readonly byte[] desInitialVector = Encoding.UTF8.GetBytes("95168702");

    /// <summary>
    /// 使用 DES 对称加密算法加密字符串。
    /// </summary>
    public static string DesEncrypt(string sourceString, string? key = null) {
        key = key is null ? "@;$ Abv2" : key.GetStableHashCode().ToString().EnsureLength('X', 8).Substring(0, 8);
        var btKey = Encoding.UTF8.GetBytes(key);
        using var des = new DESCryptoServiceProvider();
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, des.CreateEncryptor(btKey, desInitialVector), CryptoStreamMode.Write)) {
            var inData = Encoding.UTF8.GetBytes(sourceString);
            cs.Write(inData, 0, inData.Length);
            cs.FlushFinalBlock();
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// 使用 DES 对称加密算法解密字符串。
    /// </summary>
    public static string DesDecrypt(string encryptedString, string? key = null) {
        key = key is null ? "@;$ Abv2" : key.GetStableHashCode().ToString().EnsureLength('X', 8).Substring(0, 8);
        var btKey = Encoding.UTF8.GetBytes(key);
        using var des = new DESCryptoServiceProvider();
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, des.CreateDecryptor(btKey, desInitialVector), CryptoStreamMode.Write)) {
            var inData = Convert.FromBase64String(encryptedString);
            cs.Write(inData, 0, inData.Length);
            cs.FlushFinalBlock();
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
