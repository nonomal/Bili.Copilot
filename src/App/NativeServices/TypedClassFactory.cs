// Copyright (c) Bili Copilot. All rights reserved.

using System;
using System.Runtime.InteropServices;
using WinRT;

namespace Bili.Copilot.App.NativeServices;

internal class TypedClassFactory<TInterface, TImplementation> : IClassFactory
    where TImplementation : TInterface, new()
{
    public int CreateInstance(
        IntPtr outer,
        ref Guid iid,
        out IntPtr result)
    {
        if (outer != IntPtr.Zero)
        {
            Marshal.ThrowExceptionForHR(NativeMethods.CLASS_E_NOAGGREGATION);
        }

        result = MarshalInspectable<TInterface>.FromManaged(new TImplementation());

        return 0;   // S_OK
    }

    public int LockServer(bool fLock) => 0;   // S_OK
}
