// Copyright (c) Bili Copilot. All rights reserved.

using System.Runtime.InteropServices;

namespace MpvPlayer.Core.Structs.Render;

/// <summary>
/// Represents a render context.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MpvRenderContextHandle
{
    /// <summary>
    /// Handle to the render context.
    /// </summary>
    public IntPtr Handle;
}
