﻿// Copyright (c) Bili Copilot. All rights reserved.

using System.Runtime.InteropServices;

namespace MpvPlayer.Core.Structs.Client;

/// <summary>
/// Represents a client message event.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct MpvEventClientMessage
{
    /// <summary>
    /// <para>Arbitrary arguments chosen by the sender of the message. If num_args &gt; 0.</para>
    /// <para>you can access args[0] through args[num_args - 1] (inclusive). What</para>
    /// <para>these arguments mean is up to the sender and receiver.</para>
    /// <para>None of the valid items are NULL.</para>
    /// </summary>
    public int NumArgs;

    /// <summary>
    /// Array of IntPtrs/UTF8 strings.
    /// </summary>
    internal IntPtr _argsPtr;

    /// <summary>
    /// Gets the arguments.
    /// </summary>
    public string[] Args => GetArgs();

    private string[] GetArgs()
    {
        var args = new string[NumArgs];
        for (var i = 0; i < NumArgs; i++)
        {
            args[i] = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(_argsPtr, i * IntPtr.Size)) ?? string.Empty;
        }

        return args;
    }
}
