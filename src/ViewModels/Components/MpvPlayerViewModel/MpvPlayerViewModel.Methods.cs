// Copyright (c) Bili Copilot. All rights reserved.

using System;
using System.Threading.Tasks;
using Bili.Copilot.Libs.Mpv;
using Bili.Copilot.Libs.Mpv.Args;
using Bili.Copilot.Libs.Mpv.Enums.Player;
using Bili.Copilot.Libs.Provider;
using Bili.Copilot.Models.App.Args;
using Bili.Copilot.Models.App.Constants;
using Bili.Copilot.Models.App.Other;
using Bili.Copilot.Models.Constants.App;
using Bili.Copilot.Models.Data.Player;
using CommunityToolkit.Mvvm.Input;

namespace Bili.Copilot.ViewModels.Components;

/// <summary>
/// Mpv 播放器视图模型.
/// </summary>
public sealed partial class MpvPlayerViewModel
{
    /// <inheritdoc/>
    public MediaStats GetMediaInformation()
    {
        return default;
    }

    /// <inheritdoc/>
    public void ChangeLocalSubtitle(SubtitleMeta meta)
    {
        // TODO: Implement this method.
    }

    private static PlayerStatus ConvertPlayerStatus(PlaybackState state)
    {
        var s = state switch
        {
            PlaybackState.Paused => PlayerStatus.Pause,
            PlaybackState.Playing => PlayerStatus.Playing,
            PlaybackState.Buffering or PlaybackState.Opening => PlayerStatus.Buffering,
            _ => PlayerStatus.NotLoad,
        };

        return s;
    }

    [RelayCommand]
    private async Task TakeScreenshotAsync()
    {
        if (Player == null)
        {
            return;
        }

        // TODO: 截图.
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void StartRecording()
    {
        if (Player == null || IsRecording)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task StopRecordingAsync()
    {
        if (Player == null || !IsRecording)
        {
            return;
        }

        await Task.CompletedTask;
    }

    private async Task LoadDashVideoSourceAsync(bool onlyAudio)
    {
        await ((Player)Player).OpenBiliAsync(new BiliPlayOptions
        {
            AudioUrl = _audio?.BaseUrl,
            Cookies = AuthorizeProvider.GetCookieString(),
            UserAgent = ServiceConstants.DefaultUserAgentString,
            VideoUrl = _video?.BaseUrl,
            OnlyAudio = onlyAudio,
            Referer = "https://www.bilibili.com",
        });
    }

    private async Task LoadDashLiveSourceAsync(string url, bool onlyAudio)
    {
        await ((Player)Player).OpenBiliAsync(new BiliPlayOptions
        {
            AudioUrl = string.Empty,
            Cookies = AuthorizeProvider.GetCookieString(),
            UserAgent = "Mozilla/5.0 BiliDroid/1.12.0 (bbcallen@gmail.com)",
            VideoUrl = url,
            OnlyAudio = onlyAudio,
            Referer = "https://live.bilibili.com",
            IsLive = true,
        });
    }

    private async Task LoadWebDavVideoAsync()
    {
        if (Player == null || _webDavVideo == null)
        {
            return;
        }

        WebDavSubtitleListChanged?.Invoke(this, default);
        WebDavSubtitleChanged?.Invoke(this, string.Empty);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void Clear()
    {
        Status = PlayerStatus.NotLoad;
    }

    private void OnPlaybackStopped(object sender, PlaybackStoppedEventArgs e)
    {
        _ = _dispatcherQueue.TryEnqueue(() =>
        {
            if (Player == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(e.ErrorMessage))
            {
                Status = PlayerStatus.End;
                MediaEnded?.Invoke(this, EventArgs.Empty);

                var arg = new MediaStateChangedEventArgs(Status, string.Empty);
                StateChanged?.Invoke(this, arg);

                if (IsLoop && Status != PlayerStatus.Pause)
                {
                    SeekTo(TimeSpan.Zero);
                    Play();
                }
            }
            else
            {
                Status = PlayerStatus.Failed;
                var arg = new MediaStateChangedEventArgs(Status, e.ErrorMessage);
                StateChanged?.Invoke(this, arg);
                LogException(new Exception($"播放失败: {e.ErrorMessage}"));
            }
        });
    }

    private void OnPlaybackPositionChanged(object sender, PlaybackPositionChangedEventArgs e)
    {
        var duration = TimeSpan.FromSeconds(e.Duration);
        var position = TimeSpan.FromSeconds(e.Position);
        _dispatcherQueue.TryEnqueue(() =>
        {
            if (!IsPlayerReady)
            {
                OnPropertyChanged(nameof(IsPlayerReady));
            }

            PositionChanged?.Invoke(this, new MediaPositionChangedEventArgs(position, duration));
        });
    }

    private void OnPlaybackStateChanged(object sender, PlaybackStateChangedEventArgs e)
    {
        _ = _dispatcherQueue.TryEnqueue(() =>
        {
            if ((e.NewState == PlaybackState.Playing || e.NewState == PlaybackState.Paused) && e.OldState == PlaybackState.Opening)
            {
                MediaOpened?.Invoke(this, EventArgs.Empty);
            }

            Status = ConvertPlayerStatus(e.NewState);
            var arg = new MediaStateChangedEventArgs(Status, string.Empty);
            StateChanged?.Invoke(this, arg);
        });
    }
}
