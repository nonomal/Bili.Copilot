// Copyright (c) Bili Copilot. All rights reserved.

using System;
using Bili.Copilot.Libs.Mpv;
using Bili.Copilot.Libs.Mpv.Args;
using Bili.Copilot.Models.App.Args;
using Bili.Copilot.Models.App.Other;
using Bili.Copilot.Models.Constants.App;
using Bili.Copilot.Models.Data.Player;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace Bili.Copilot.ViewModels.Components;

/// <summary>
/// Mpv 播放器视图模型.
/// </summary>
public sealed partial class MpvPlayerViewModel
{
    private readonly DispatcherQueue _dispatcherQueue;

    private SegmentInformation _video;
    private SegmentInformation _audio;
    private WebDavVideoInformation _webDavVideo;
    private InitializeArgument _initializeArgument;

    private bool _isStopped;

    [ObservableProperty]
    private bool _isLoop;

    [ObservableProperty]
    private object _player;

    [ObservableProperty]
    private bool _isRecording;

    /// <inheritdoc/>
    public event EventHandler MediaOpened;

    /// <inheritdoc/>
    public event EventHandler MediaEnded;

    /// <inheritdoc/>
    public event EventHandler<MediaStateChangedEventArgs> StateChanged;

    /// <inheritdoc/>
    public event EventHandler<MediaPositionChangedEventArgs> PositionChanged;

    /// <inheritdoc/>
    public event EventHandler<WebDavSubtitleListChangedEventArgs> WebDavSubtitleListChanged;

    /// <inheritdoc/>
    public event EventHandler<string> WebDavSubtitleChanged;

    /// <inheritdoc/>
    public TimeSpan Position => Player == null ? TimeSpan.Zero : ((Player)Player).Position ?? TimeSpan.Zero;

    /// <inheritdoc/>
    public TimeSpan Duration => Player == null ? TimeSpan.Zero : ((Player)Player).Duration ?? TimeSpan.Zero;

    /// <inheritdoc/>
    public double Volume => Player is Player player ? player.GetVolume() : 0d;

    /// <inheritdoc/>
    public double PlayRate => Player is Player player ? player.GetPlayRate() : 1d;

    /// <inheritdoc/>
    public string LastError => string.Empty;

    /// <inheritdoc/>
    public PlayerStatus Status { get; set; }

    /// <inheritdoc/>
    public bool IsPlayerReady => Player is Player player && player.IsMediaLoaded();

    /// <inheritdoc/>
    public bool IsRecordingSupported => false;

    /// <inheritdoc/>
    public bool IsMediaStatsSupported => false;

    /// <inheritdoc/>
    public bool IsStatsUpdated { get; set; }
}
