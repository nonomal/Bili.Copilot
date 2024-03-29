// Copyright (c) Bili Copilot. All rights reserved.

using System.Runtime.InteropServices;
using Bili.Copilot.Libs.Mpv.Enums.Client;

namespace Bili.Copilot.Libs.Mpv.Structs.Client;

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
