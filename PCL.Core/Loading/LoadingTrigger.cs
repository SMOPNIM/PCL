namespace PCL;

/// <summary>
/// 加载器 / 加载状态的状态枚举。
/// 原为 WPF 控件 MyLoading 的嵌套枚举；因其为纯状态机抽象、
/// 且被 LoaderBase（下载/启动任务栈的根）实现，故提取至 PCL.Core，
/// 枚举值与可见性保持不变。
/// </summary>
public enum MyLoadingState {
    Unloaded = -1,
    Run = 0,
    Stop = 1,
    Error = 2,
}

/// <summary>
/// 加载触发器接口。由 <see cref="LoaderBase"/> 等实现，
/// 让加载动画控件以统一方式订阅任意任务的状态与进度。
/// 事件采用 Action 委托，以便 VB 侧以等价的 Event（Implements / Handles）对接
/// —— 两种方向的互操作均已通过编译与运行时验证。
/// </summary>
public interface ILoadingTrigger {
    bool IsLoader { get; }
    MyLoadingState LoadingState { get; set; }
    event Action<MyLoadingState, MyLoadingState> LoadingStateChanged;
    event Action<double, double> ProgressChanged;
}

/// <summary>
/// 一个不绑定具体任务、可独立改动的加载状态模拟器，
/// 作为 MyLoading 控件在未指定 State 时的默认值。
/// </summary>
public class MyLoadingStateSimulator : ILoadingTrigger {
    private MyLoadingState _LoadingState = MyLoadingState.Unloaded;

    public MyLoadingState LoadingState {
        get => _LoadingState;
        set {
            if (_LoadingState == value) return;
            var OldState = _LoadingState;
            _LoadingState = value;
            LoadingStateChanged?.Invoke(value, OldState);
        }
    }

    public bool IsLoader => false;

    public event Action<MyLoadingState, MyLoadingState>? LoadingStateChanged;
    public event Action<double, double>? ProgressChanged;
}
