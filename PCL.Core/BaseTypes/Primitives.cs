namespace PCL;

/// <summary>
/// 支持负数与浮点数的矩形。
/// </summary>
public class MyRect {
    /// <summary>宽度。</summary>
    public double Width { get; set; } = 0;
    /// <summary>高度。</summary>
    public double Height { get; set; } = 0;
    /// <summary>左边距。</summary>
    public double Left { get; set; } = 0;
    /// <summary>上边距。</summary>
    public double Top { get; set; } = 0;
    public MyRect() { }
    public MyRect(double left, double top, double width, double height) {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }
}

/// <summary>
/// 模块加载状态枚举。
/// </summary>
public enum LoadState {
    Waiting,
    Loading,
    Finished,
    Failed,
    Canceled,
}

/// <summary>
/// 执行返回值。
/// </summary>
public enum ProcessReturnValues {
    /// <summary>执行成功。</summary>
    Success = 0,
    /// <summary>执行失败。</summary>
    Fail = 1,
    /// <summary>执行时出现未经处理的异常。</summary>
    Exception = 2,
    /// <summary>执行超时。</summary>
    Timeout = 3,
    /// <summary>取消执行。可能是由于不满足执行的前置条件。</summary>
    Cancel = 4,
    /// <summary>任务成功完成。</summary>
    TaskDone = 5,
}

// 注：GetResultWithTimeout 与 SortByComparison 是扩展方法，若在此实现会与 VB ModBase 中
// 的同名模块函数产生二义性，故这两个函数暂保留在 ModBase.vb，后续整体迁移 ModBase 时再迁入。
