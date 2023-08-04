// Copyright (c) Bili Copilot. All rights reserved.

using System;
using System.Runtime.InteropServices;

namespace Bili.Copilot.App.NativeServices;

internal static class HelperMethods
{
    public static Guid GetGuidFromType(Type t)
    {
        if (t == null)
        {
            throw new ArgumentNullException("Type object cannot be null.", "t");
        }

        var attributes = t.GetCustomAttributes(typeof(GuidAttribute), false);

        if (attributes.Length < 1)
        {
            throw new ArgumentException("No GUID attribute specified on the type " + t.Name);
        }

        var attr = (GuidAttribute)attributes[0];
        return new Guid(attr.Value);
    }
}
