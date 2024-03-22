// Copyright (c) Bili Copilot. All rights reserved.

using System.Diagnostics;
using System.Runtime.InteropServices;
using MpvPlayer.Core.Args;
using MpvPlayer.Core.Enums.Client;
using MpvPlayer.Core.Enums.Player;
using MpvPlayer.Core.Structs.Client;

namespace MpvPlayer.Core;

/// <summary>
/// Represents a media player.
/// </summary>
public sealed partial class Player
{
    private void EventLoop()
    {
        while (!_isDisposed)
        {
            var clientEvent = Client.WaitEvent();
            switch (clientEvent.EventId)
            {
                case MpvEventId.Shutdown:
                    Dispose();
                    Destroyed?.Invoke(this, EventArgs.Empty);
                    return;
                case MpvEventId.LogMessage:
                    {
                        var logMessage = clientEvent.GetData<MpvEventLogMessage>();
                        var args = new LogMessageReceivedEventArgs(logMessage.Prefix, logMessage.Text, logMessage.Level.ToLogLevel());
                        TranslateLogMessage(logMessage);
                        LogMessageReceived?.Invoke(this, args);
                    }

                    break;
                case MpvEventId.StartFile:
                    {
                        // var startFileData = clientEvent.GetData<MpvEventStartFile>();
                        ChangeState(PlaybackState.Opening);
                    }

                    break;
                case MpvEventId.FileLoaded:
                    ChangeState(PlaybackState.Decoding);
                    break;
                case MpvEventId.EndFile:
                    {
                        var endFileData = clientEvent.GetData<MpvEventEndFile>();
                        ChangeState(PlaybackState.None);
                        RaiseEnd(endFileData);
                    }

                    break;
                case MpvEventId.PlaybackRestart:
                    {
                        ChangeState(PlaybackState.Playing);
                    }

                    break;
                case MpvEventId.PropertyChange:
                    {
                        var propData = clientEvent.GetData<MpvEventProperty>();
                        TranslateProperty(propData);
                    }

                    break;
            }

            if (clientEvent.EventId != MpvEventId.None)
            {
                Debug.WriteLine($"Event: {clientEvent.EventId}");
            }
        }
    }

    private void TranslateLogMessage(MpvEventLogMessage logMessage)
    {
        if (logMessage.Prefix == "cplayer")
        {
            var text = logMessage.Text.Trim();
            if (text.StartsWith("built on"))
            {
                var t = DateTimeOffset.Parse(text.Replace("built on", string.Empty).Trim());
                Dependencies.BuildTime = t;
            }
            else if (text.StartsWith("FFmpeg version"))
            {
                Dependencies.FFmpegVersion = text.Replace("FFmpeg version:", string.Empty).Trim();
            }
            else if (text.StartsWith("libplacebo version"))
            {
                Dependencies.LibplaceboVersion = text.Replace("libplacebo version:", string.Empty).Trim();
            }
            else if (text.StartsWith("libavutil"))
            {
                Dependencies.LibavutilVersion = text.Replace("libavutil", string.Empty).Trim();
            }
            else if (text.StartsWith("libavcodec"))
            {
                Dependencies.LibavcodecVersion = text.Replace("libavcodec", string.Empty).Trim();
            }
            else if (text.StartsWith("libavformat"))
            {
                Dependencies.LibavformatVersion = text.Replace("libavformat", string.Empty).Trim();
            }
            else if (text.StartsWith("libavfilter"))
            {
                Dependencies.LibavfilterVersion = text.Replace("libavfilter", string.Empty).Trim();
            }
            else if (text.StartsWith("libswscale"))
            {
                Dependencies.LibswscaleVersion = text.Replace("libswscale", string.Empty).Trim();
            }
            else if (text.StartsWith("libswresample"))
            {
                Dependencies.LibswresampleVersion = text.Replace("libswresample", string.Empty).Trim();
            }
        }
    }

    private void TranslateProperty(MpvEventProperty property)
    {
        Debug.WriteLine($"Property name:{property.Name}");
        if (property.DataPtr == IntPtr.Zero)
        {
            return;
        }

        if (property.Name == PauseProperty)
        {
            var isPaused = Marshal.ReadInt32(property.DataPtr) == 1;
            ChangeState(isPaused ? PlaybackState.Paused : PlaybackState.Playing);
        }
        else if (property.Name == PositionProperty)
        {
            var position = Marshal.ReadInt64(property.DataPtr);
            _currentPosition = position;
            RaisePositionChanged();
        }
        else if (property.Name == DurationProperty)
        {
            var duration = Marshal.ReadInt64(property.DataPtr);
            _currentDuration = duration;
            RaisePositionChanged();
        }
    }

    private void ChangeState(PlaybackState state)
    {
        if (state == PlaybackState)
        {
            return;
        }

        Debug.WriteLine($"StateChanged: {state}");
        var args = new PlaybackStateChangedEventArgs(PlaybackState, state);
        PlaybackState = state;
        PlaybackStateChanged?.Invoke(this, args);
    }

    private void RaiseEnd(MpvEventEndFile e)
    {
        if (e.Reason == MpvEndFileReason.Quit || e.Reason == MpvEndFileReason.Redirect)
        {
            // Do not raise the event if the player shutdown or redirected.
            return;
        }

        var isInterrupted = e.Reason == MpvEndFileReason.Error;
        var errorMessage = isInterrupted ? $"Playback stopped due to an error. {e.Error}" : string.Empty;
        var args = new PlaybackStoppedEventArgs(isInterrupted, errorMessage);
        PlaybackStopped?.Invoke(this, args);
    }

    private void RaisePositionChanged()
    {
        if (_currentDuration > 0 && _currentPosition >= 0)
        {
            var args = new PlaybackPositionChangedEventArgs(_currentDuration, _currentPosition);
            PlaybackPositionChanged?.Invoke(this, args);
        }
    }
}
