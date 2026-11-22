using System.Threading;

namespace PCL;

/// <summary>
/// UI 线程分发抽象。封装"在 UI 线程上执行代码"这一平台相关操作，
/// 使 PCL.Core 的核心逻辑（加载器、下载、启动等）不直接依赖 WPF 的
/// <c>Application.Current.Dispatcher</c>。
/// </summary>
/// <remarks>
/// WPF 宿主在程序启动时（主线程上）调用 <see cref="Set"/> 注入基于
/// <c>Dispatcher</c> 的实现；未来迁移到 Avalonia 时，宿主只需注入
/// <c>Dispatcher.UIThread</c> 的等价实现，核心代码无需任何改动。
/// 在未调用 <see cref="Set"/> 前，本类退化为"在当前线程直接执行"，
/// 以便在非 UI 环境（如单元测试）中安全使用。
/// </remarks>
public static class UiDispatcher {
    private static int _UiThreadId = 0;
    private static Func<bool> _IsUiThread = () => false;
    private static Func<Func<object>, object> _InvokeSynchronously = f => f();
    private static Action<Action> _InvokeAsync = action => action();

    /// <summary>
    /// 当前线程是否为 UI（主）线程。
    /// 语义与原 VB 的 <c>RunInUi()</c>（比较托管线程 ID）完全一致。
    /// </summary>
    public static bool IsUiThread => _IsUiThread();

    /// <summary>
    /// 在 UI 线程上同步执行 <paramref name="func"/>（阻塞当前线程直至完成）并返回其结果。
    /// 对应原 VB <c>RunInUiWait(Of Output)</c> 所使用的 <c>Dispatcher.Invoke</c>。
    /// </summary>
    public static T InvokeSynchronously<T>(Func<T> func) {
        var Boxed = _InvokeSynchronously(() => (object)func());
        return (T)Boxed;
    }

    /// <summary>
    /// 在 UI 线程上同步执行 <paramref name="action"/>（阻塞当前线程直至完成）。
    /// 对应原 VB <c>RunInUiWait(Action)</c> 所使用的 <c>Dispatcher.Invoke</c>。
    /// </summary>
    public static void InvokeSynchronously(Action action) {
        if (action is null) return;
        _InvokeSynchronously(() => { action(); return null; });
    }

    /// <summary>
    /// 将 <paramref name="action"/> 排队到 UI 线程异步执行（不阻塞当前线程）。
    /// 对应原 VB <c>RunInUi</c>（非 ForceWaitUntilLoaded 分支）所使用的
    /// <c>Dispatcher.InvokeAsync</c>。
    /// </summary>
    public static void InvokeAsync(Action action) => _InvokeAsync(action);

    /// <summary>
    /// 注册 UI 线程分发实现。必须在程序启动时、且位于 UI（主）线程上调用，
    /// 以便正确捕获 UI 线程 ID（供 <see cref="IsUiThread"/> 判定）。
    /// 同步回调接收一个返回 <c>object</c> 的函数（值的返回值经装箱传递），
    /// 并应将其结果返回。
    /// </summary>
    public static void Set(Func<Func<object>, object> invokeSynchronously, Action<Action> invokeAsync) {
        _UiThreadId = Thread.CurrentThread.ManagedThreadId;
        _IsUiThread = () => Thread.CurrentThread.ManagedThreadId == _UiThreadId;
        _InvokeSynchronously = invokeSynchronously ?? (f => f());
        _InvokeAsync = invokeAsync ?? (a => a());
    }
}
