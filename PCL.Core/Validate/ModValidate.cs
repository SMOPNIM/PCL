namespace PCL;

/// <summary>
/// 输入验证辅助函数。
/// </summary>
public static class ModValidate {
    /// <summary>
    /// 进行输入验证，并返回错误原因。
    /// 如果没有错误则返回空字符串。
    /// </summary>
    public static string? Validate(string? Text, IEnumerable<Validate> ValidateRules) {
        string? Result = "";
        foreach (var ValidateRule in ValidateRules) {
            Result = ValidateRule.Check(Text);
            if (Result is null) return "";
            if (Result != "") return Result;
        }
        return Result;
    }
}
