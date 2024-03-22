// Copyright (c) Bili Copilot. All rights reserved.

namespace Bili.Copilot.Controls.Base.PlayerExtensions;

/// <summary>
/// 帧缓冲基类.
/// </summary>
public abstract class FrameBufferBase : IDisposable
{
    /// <summary>
    /// 获取或设置帧缓冲宽度.
    /// </summary>
    public abstract int BufferWidth { get; protected set; }

    /// <summary>
    /// 获取或设置帧缓冲高度.
    /// </summary>
    public abstract int BufferHeight { get; protected set; }

    /// <summary>
    /// 获取或设置帧缓冲句柄.
    /// </summary>
    public abstract IntPtr SwapChainHandle { get; protected set; }

    /// <inheritdoc/>
    public abstract void Dispose();
}
