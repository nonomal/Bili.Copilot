﻿// Copyright (c) Bili Copilot. All rights reserved.

using System.Runtime.InteropServices;

namespace Bili.Copilot.Libs.Mpv.Structs.RenderGL;

/// <summary>For Mpv_RENDER_PARAM_DRM_DRAW_SURFACE_SIZE.</summary>
[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct MpvOpenGLDRMDrawSurfaceSize
{
    /// <summary>size of the draw plane surface in pixels.</summary>
    public int Width;

    /// <summary>size of the draw plane surface in pixels.</summary>
    public int Height;
}
