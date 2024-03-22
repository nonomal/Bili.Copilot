// Copyright (c) Bili Copilot. All rights reserved.

using WinRT;

namespace Bili.Copilot.Controls.Base.PlayerExtensions;

/// <summary>
/// OpenGL 渲染控件.
/// </summary>
public unsafe class OpenGLRenderControl : OpenGLRenderControlBase<FrameBuffer>
{
    private SwapChainPanel _swapChainPanel;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLRenderControl"/> class.
    /// </summary>
    public OpenGLRenderControl()
    {
        SizeChanged += OnSizeChanged;
        Unloaded += OnUnloaded;
    }

    /// <summary>
    /// 准备就绪时触发.
    /// </summary>
    public event EventHandler Ready;

    /// <summary>
    /// 渲染时触发.
    /// </summary>
    public event Action<TimeSpan> Render;

    /// <summary>
    /// OpenGL 上下文设置.
    /// </summary>
    public OpenGLContextSettings Setting { get; set; } = new OpenGLContextSettings();

    /// <summary>
    /// OpenGL 渲染上下文.
    /// </summary>
    public OpenGLRenderContext Context { get; private set; }

    /// <summary>
    /// 横向缩放比例.
    /// </summary>
    public double ScaleX => _swapChainPanel?.CompositionScaleX ?? 1;

    /// <summary>
    /// 纵向缩放比例.
    /// </summary>
    public double ScaleY => _swapChainPanel?.CompositionScaleY ?? 1;

    /// <summary>
    /// 初始化.
    /// </summary>
    public override void Initialize()
    {
        if (Context == null)
        {
            base.Initialize();
            Context = new OpenGLRenderContext(Setting);
            _swapChainPanel = new SwapChainPanel();
            _swapChainPanel.CompositionScaleChanged += OnCompositionScaleChanged;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;
            Content = _swapChainPanel;

            if (!TryLoadFrameBuffer())
            {
                UpdateFrameBufferSize();
            }

            Ready?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 获取缓冲区句柄.
    /// </summary>
    /// <returns>句柄.</returns>
    public IntPtr GetBufferHandle()
        => FrameBuffer.GLFrameBufferHandle;

    /// <summary>
    /// 绘制.
    /// </summary>
    protected override void Draw()
    {
        FrameBuffer.Begin();
        Render?.Invoke(_stopwatch.Elapsed - _lastFrameStamp);
        FrameBuffer.End();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
        => Release();

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (Context != null && e.NewSize.Width > 0 && e.NewSize.Height > 0)
        {
            if (!TryLoadFrameBuffer())
            {
                UpdateFrameBufferSize();
            }
        }
    }

    private void OnCompositionScaleChanged(SwapChainPanel sender, object args)
        => UpdateFrameBufferSize();

    private void UpdateFrameBufferSize()
        => FrameBuffer?.UpdateSize((int)ActualWidth, (int)ActualHeight, ScaleX, ScaleY);

    private bool TryLoadFrameBuffer()
    {
        if (FrameBuffer != null)
        {
            return false;
        }

        FrameBuffer = new FrameBuffer(Context, (int)ActualWidth, (int)ActualHeight, ScaleX, ScaleY);
        _swapChainPanel.As<ISwapChainPanelNative>().SetSwapChain(FrameBuffer.SwapChainHandle);
        return true;
    }
}
