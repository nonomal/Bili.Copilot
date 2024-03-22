// Copyright (c) Bili Copilot. All rights reserved.

namespace MpvPlayer.Core.Structs.Client;

/// <summary>
/// Represents a handle to an mpv instance.
/// </summary>
public struct MpvHandle
{
    /// <summary>
    /// Pointer to this struct. This is used as a unique identifier.
    /// </summary>
    public IntPtr Handle;

    /// <summary>
    /// Gets the none handle.
    /// </summary>
    public static MpvHandle None => new MpvHandle { Handle = IntPtr.Zero };

    public static implicit operator bool(MpvHandle handle) => handle.Handle != IntPtr.Zero;
}
