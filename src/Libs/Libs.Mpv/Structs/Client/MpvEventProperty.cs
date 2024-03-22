// Copyright (c) Bili Copilot. All rights reserved.

using System.Runtime.InteropServices;
using MpvPlayer.Core.Enums.Client;

namespace MpvPlayer.Core.Structs.Client;

/// <summary>
/// Represents a property event.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MpvEventProperty
{
    /// <summary>
    /// Property name.
    /// </summary>
    public string Name;

    /// <summary>
    /// Property data.
    /// </summary>
    public MpvFormat Format;

    /// <summary>
    /// Pointer to the data. In what format the data is stored is up to whatever
    /// </summary>
    public IntPtr DataPtr;
}
