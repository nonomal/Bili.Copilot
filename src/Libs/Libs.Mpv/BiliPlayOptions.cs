// Copyright (c) Bili Copilot. All rights reserved.

namespace Bili.Copilot.Libs.Mpv;

/// <summary>
/// Bili 播放器选项.
/// </summary>
public sealed class BiliPlayOptions
{
    /// <summary>
    /// 视频地址.
    /// </summary>
    public string VideoUrl { get; set; }

    /// <summary>
    /// 音频地址.
    /// </summary>
    public string AudioUrl { get; set; }

    /// <summary>
    /// 是否仅音频.
    /// </summary>
    public bool OnlyAudio { get; set; }

    /// <summary>
    /// 请求头.
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// 来源.
    /// </summary>
    public string Referer { get; set; }

    /// <summary>
    /// Cookies.
    /// </summary>
    public string Cookies { get; set; }

    /// <summary>
    /// 是否直播.
    /// </summary>
    public bool IsLive { get; set; }
}
