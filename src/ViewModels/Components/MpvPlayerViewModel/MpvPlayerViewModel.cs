// Copyright (c) Bili Copilot. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Bili.Copilot.Libs.Mpv;
using Bili.Copilot.Libs.Mpv.Args;
using Bili.Copilot.Models.App.Other;
using Bili.Copilot.Models.Data.Player;
using Microsoft.UI.Dispatching;
using Windows.ApplicationModel;

namespace Bili.Copilot.ViewModels.Components;

/// <summary>
/// Mpv 播放器视图模型.
/// </summary>
public sealed partial class MpvPlayerViewModel : ViewModelBase, IPlayerViewModel
{
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="MpvPlayerViewModel"/> class.
    /// </summary>
    public MpvPlayerViewModel()
        => _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    /// <summary>
    /// 初始化.
    /// </summary>
    public async void Initialize()
    {
        var currentFolder = Package.Current.InstalledPath;
        var arch = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "x64";
        var libmpvFolder = System.IO.Path.Combine(currentFolder, "Assets", "libmpv", arch, "libmpv-2.dll");
        var player = new Player(libmpvFolder);
        player.Client.SetProperty("vo", "libmpv");
        player.PlaybackStateChanged += OnPlaybackStateChanged;
        player.PlaybackPositionChanged += OnPlaybackPositionChanged;
        player.PlaybackStopped += OnPlaybackStopped;

        if (_initializeArgument != null)
        {
            await player.InitializeAsync(_initializeArgument);
        }

        Player = player;
    }

    /// <summary>
    /// 设置初始化参数.
    /// </summary>
    /// <param name="arg">参数.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task SetInitializeArgumentsAsync(InitializeArgument arg)
    {
        _initializeArgument = arg;
        if (Player is Player player && !player.Client.IsInitialized)
        {
            await player.InitializeAsync(arg);
        }
    }

    /// <inheritdoc/>
    public async Task SetSourceAsync(SegmentInformation video, SegmentInformation audio, bool audioOnly)
    {
        _video = video;
        _audio = audio;
        _isStopped = false;
        await LoadDashVideoSourceAsync(audioOnly);
    }

    /// <inheritdoc/>
    public async void SetLiveSource(string url, bool audioOnly)
    {
        _video = null;
        _audio = null;
        _isStopped = false;
        await LoadDashLiveSourceAsync(url, audioOnly);
    }

    /// <inheritdoc/>
    public async Task SetWebDavAsync(WebDavVideoInformation video)
    {
        _webDavVideo = video;
        _isStopped = false;
        await LoadWebDavVideoAsync();
    }

    /// <inheritdoc/>
    public void Pause()
    {
        if (Player is Player player)
        {
            player.Pause();
        }
    }

    /// <inheritdoc/>
    public void Stop()
    {
        if (_isStopped)
        {
            return;
        }

        if (Player is Player player)
        {
            player.Stop();
        }

        _isStopped = true;
    }

    /// <inheritdoc/>
    public void Play()
    {
        if (Player is not Player player)
        {
            return;
        }

        if ((player.Duration ?? TimeSpan.Zero) - (player.Position ?? TimeSpan.Zero) < TimeSpan.FromSeconds(3))
        {
            SeekTo(TimeSpan.Zero);
        }

        player.Play();
    }

    /// <inheritdoc/>
    public void SeekTo(TimeSpan time)
    {
        if (time.TotalMilliseconds >= Duration.TotalMilliseconds
            || Math.Abs(time.TotalSeconds - Position.TotalSeconds) < 2)
        {
            return;
        }

        if (Player is Player player)
        {
            player.Seek(time);
        }
    }

    /// <inheritdoc/>
    public void SetPlayRate(double rate)
    {
        if (Player is Player player)
        {
            player.SetPlayRate(rate);
        }
    }

    /// <inheritdoc/>
    public void SetVolume(int volume)
    {
        if (Player is Player player)
        {
            player.SetVolume(volume);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Clear();
            }

            if (Player is Player player)
            {
                player.Dispose();
            }

            Player = null;
            _disposedValue = true;
        }
    }
}
