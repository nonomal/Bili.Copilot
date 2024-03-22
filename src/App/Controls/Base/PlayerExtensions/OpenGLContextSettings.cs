// Copyright (c) Bili Copilot. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using OpenTK.Windowing.Common;

namespace Bili.Copilot.Controls.Base.PlayerExtensions;

/// <summary>
/// OpenGL 上下文配置.
/// </summary>
public sealed class OpenGLContextSettings
{
    /// <summary>
    /// 主要版本.
    /// </summary>
    public int MajorVersion { get; set; } = 3;

    /// <summary>
    /// 次要版本.
    /// </summary>
    public int MinorVersion { get; set; } = 3;

    /// <summary>
    /// 上下文标志.
    /// </summary>
    public ContextFlags GraphicsContextFlags { get; set; } = ContextFlags.Default;

    /// <summary>
    /// 图形配置.
    /// </summary>
    public ContextProfile GraphicsProfile { get; set; } = ContextProfile.Core;

    /// <summary>
    /// 要使用的上下文.
    /// </summary>
    public IGraphicsContext ContextToUse { get; set; }

    /// <summary>
    /// 是否指向同一个上下文.
    /// </summary>
    /// <param name="a">上下文配置 A.</param>
    /// <param name="b">上下文配置 B.</param>
    /// <returns>是否相同.</returns>
    public static bool WouldResultInSameContext([NotNull] OpenGLContextSettings a, [NotNull] OpenGLContextSettings b)
    {
        if (a.MajorVersion != b.MajorVersion)
        {
            return false;
        }

        if (a.MinorVersion != b.MinorVersion)
        {
            return false;
        }

        if (a.GraphicsProfile != b.GraphicsProfile)
        {
            return false;
        }

        if (a.GraphicsContextFlags != b.GraphicsContextFlags)
        {
            return false;
        }

        return true;
    }
}
