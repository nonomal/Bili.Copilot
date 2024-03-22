// Copyright (c) Bili Copilot. All rights reserved.

using System.Runtime.InteropServices;
using MpvPlayer.Core.Args;
using MpvPlayer.Core.Interop;
using MpvPlayer.Core.Structs.Render;
using MpvPlayer.Core.Structs.RenderGL;

namespace MpvPlayer.Core;

/// <summary>
/// Mpv player.
/// </summary>
public sealed partial class Player : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    public Player(string libmpvPath)
    {
        Resolver.SetResolver(libmpvPath);
        Client = new MpvClientNative();
        Dependencies = new Structs.Player.LibMpvDependencies();
    }

    /// <inheritdoc/>
    public async void Dispose()
    {
        _isDisposed = true;
        Client.UnObserveProperties();
        RenderContext?.Destroy();
        await Client.DestroyAsync();
    }

    /// <summary>
    /// Initializes the player.
    /// </summary>
    /// <param name="argument">Arguments.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task InitializeAsync(InitializeArgument? argument = null)
    {
        if (Client.IsInitialized)
        {
            return;
        }

        AutoPlay = argument?.AutoPlay ?? true;
        await Client.InitializeAsync();
        _ = Task.Run(EventLoop);
        if (argument?.ConfigFile is not null)
        {
            await Client.LoadConfigAsync(argument.ConfigFile);
        }

        if (argument?.OpenGLGetProcAddress is not null)
        {
            var glParams = new MpvOpenGLInitParams
            {
                GetProcAddrFn = (ctx, name) =>
                {
                    return argument!.OpenGLGetProcAddress!(name);
                },

                GetProcAddressCtx = IntPtr.Zero,
            };

            var glParamsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(glParams));
            Marshal.StructureToPtr(glParams, glParamsPtr, false);
            var glStringPtr = Marshal.StringToCoTaskMemUTF8("opengl");
            var para = new MpvRenderParam[]
            {
                new MpvRenderParam { Type = Enums.Render.MpvRenderParamType.ApiType, Data = glStringPtr },
                new MpvRenderParam { Type = Enums.Render.MpvRenderParamType.OpenGLInitParams, Data = glParamsPtr },
                new MpvRenderParam { Type = Enums.Render.MpvRenderParamType.Invalid, Data = IntPtr.Zero },
            };
            RenderContext = new MpvRenderContextNative(Client.Handle, para);

            Marshal.FreeHGlobal(glParamsPtr);
            Marshal.FreeCoTaskMem(glStringPtr);
        }

        Client.ObserveProperty(PauseProperty, Enums.Client.MpvFormat.Flag);
        Client.ObserveProperty(DurationProperty, Enums.Client.MpvFormat.Int64);
        Client.ObserveProperty(PositionProperty, Enums.Client.MpvFormat.Int64);
    }

    /// <summary>
    /// Render the video to the OpenGL context.
    /// </summary>
    public void RenderGL(int width, int height, int fboInt)
    {
        var fbo = new MpvOpenGLFBO
        {
            Fbo = fboInt,
            W = width,
            H = height,
        };

        var fboPtr = Marshal.AllocHGlobal(Marshal.SizeOf(fbo));
        Marshal.StructureToPtr(fbo, fboPtr, false);

        var flipY = 0;
        var flipYPtr = Marshal.AllocHGlobal(flipY);
        Marshal.WriteInt32(flipYPtr, 0);
        RenderContext!.Render([
            new MpvRenderParam { Type = Enums.Render.MpvRenderParamType.Fbo, Data = fboPtr },
            new MpvRenderParam { Type = Enums.Render.MpvRenderParamType.FlipY, Data = flipYPtr },
            new MpvRenderParam { Type = Enums.Render.MpvRenderParamType.Invalid, Data = IntPtr.Zero },
            ]);

        Marshal.FreeHGlobal(fboPtr);
        Marshal.FreeHGlobal(flipYPtr);
    }

    /// <summary>
    /// Render the video to the window.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <returns><see cref="Task"/>.</returns>
    public async Task OpenAsync(string path)
    {
        await Client.ExecuteAsync(new[] { "loadfile", path, "replace" });
        Client.SetProperty(PauseProperty, !AutoPlay);
    }

    /// <summary>
    /// Is media loaded.
    /// </summary>
    /// <returns>Loaded or not.</returns>
    public bool IsMediaLoaded()
    {
        try
        {
            var duration = Client.GetPropertyToLong(DurationProperty);
            return duration > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Play the media.
    /// </summary>
    public void Play()
    {
        if (IsMediaLoaded())
        {
            if (_currentPosition == _currentDuration)
            {
                Seek(TimeSpan.Zero);
            }

            Client.SetProperty(PauseProperty, false);
        }
    }

    /// <summary>
    /// Pause the media.
    /// </summary>
    public void Pause()
    {
        if (IsMediaLoaded())
        {
            Client.SetProperty(PauseProperty, true);
        }
    }

    /// <summary>
    /// Toggle play/pause.
    /// </summary>
    public async void TogglePlayPauseAsync()
    {
        if (IsMediaLoaded())
        {
            await Client.ExecuteAsync("cycle pause");
        }
    }

    /// <summary>
    /// Jump to the specified position.
    /// </summary>
    /// <param name="ts">Time position.</param>
    public void Seek(TimeSpan ts)
    {
        var pos = ts.TotalSeconds;
        if (pos >= 0 && pos <= _currentDuration)
        {
            // Seek to the position.
            Client.SetProperty(PositionProperty, pos);
        }
        else if (pos > _currentDuration && _currentDuration > 0)
        {
            Client.SetProperty(PositionProperty, Math.Max(0, _currentDuration - 1));
        }
        else if (pos < 0)
        {
            Client.SetProperty(PositionProperty, 0);
        }
    }

    /// <summary>
    /// Set the play rate.
    /// </summary>
    /// <param name="rate">Play rate.</param>
    public void SetPlayRate(double rate)
    {
        if (IsMediaLoaded())
        {
            Client.SetProperty(SpeedProperty, rate);
        }
    }
}
