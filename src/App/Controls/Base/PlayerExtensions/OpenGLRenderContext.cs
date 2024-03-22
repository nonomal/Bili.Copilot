// Copyright (c) Bili Copilot. All rights reserved.

using System.Reflection;
using System.Threading;
using OpenTK.Graphics.Wgl;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Bili.Copilot.Controls.Base.PlayerExtensions;

/// <summary>
/// 渲染上下文.
/// </summary>
public unsafe class OpenGLRenderContext
{
    private static IGraphicsContext _sharedContext;
    private static OpenGLContextSettings _sharedContextSettings;
    private static int _sharedContextReferenceCount;
    private static OpenTK.IBindingsContext _sharedBindingContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLRenderContext"/> class.
    /// </summary>
    public OpenGLRenderContext(OpenGLContextSettings settings)
    {
        IDXGIFactory2* factory;
        ID3D11Device* device;
        ID3D11DeviceContext* devCtx;

        // Factory
        {
            var guid = typeof(IDXGIFactory2).GetTypeInfo().GUID;
            DXGI.GetApi(null).CreateDXGIFactory2(0, &guid, (void**)&factory);
        }

        // Device
        {
            var flags = CreateDeviceFlag.BgraSupport | CreateDeviceFlag.VideoSupport;
            D3D11.GetApi(null).CreateDevice(null, D3DDriverType.Hardware, 0, Convert.ToUInt32(flags), null, 0, D3D11.SdkVersion, &device, null, &devCtx);
        }

        DxDeviceFactory = (IntPtr)factory;
        DxDeviceHandle = (IntPtr)device;
        DxDeviceContext = (IntPtr)devCtx;

        GraphicsContext = GetOrCreateSharedOpenGLContext(settings);
        GlDeviceHandle = Wgl.DXOpenDeviceNV((IntPtr)device);
    }

    /// <summary>
    /// 获取渲染格式.
    /// </summary>
    public Format Format { get; }

    /// <summary>
    /// 获取渲染宽度.
    /// </summary>
    public IntPtr DxDeviceFactory { get; }

    /// <summary>
    /// 获取渲染高度.
    /// </summary>
    public IntPtr DxDeviceHandle { get; }

    /// <summary>
    /// 获取渲染上下文.
    /// </summary>
    public IntPtr DxDeviceContext { get; }

    /// <summary>
    /// 获取渲染设备句柄.
    /// </summary>
    public IntPtr GlDeviceHandle { get; }

    /// <summary>
    /// 获取图形上下文.
    /// </summary>
    public IGraphicsContext GraphicsContext { get; }

    /// <summary>
    /// 获取指定名称的函数指针.
    /// </summary>
    /// <param name="name">名称.</param>
    /// <returns>指针.</returns>
    public static IntPtr GetProcAddress(string name)
    {
        if(_sharedBindingContext == null)
        {
            return IntPtr.Zero;
        }

        return _sharedBindingContext.GetProcAddress(name);
    }

    private static IGraphicsContext GetOrCreateSharedOpenGLContext(OpenGLContextSettings settings)
    {
        if (_sharedContext == null)
        {
            var windowSettings = NativeWindowSettings.Default;
            windowSettings.StartFocused = false;
            windowSettings.StartVisible = false;
            windowSettings.NumberOfSamples = 0;
            windowSettings.APIVersion = new Version(settings.MajorVersion, settings.MinorVersion);
            windowSettings.Flags = ContextFlags.Offscreen | settings.GraphicsContextFlags;
            windowSettings.Profile = settings.GraphicsProfile;
            windowSettings.WindowBorder = WindowBorder.Hidden;
            windowSettings.WindowState = OpenTK.Windowing.Common.WindowState.Minimized;
            NativeWindow nativeWindow = new(windowSettings);

            _sharedBindingContext = new GLFWBindingsContext();
            Wgl.LoadBindings(_sharedBindingContext);

            _sharedContext = nativeWindow.Context;
            _sharedContextSettings = settings;

            _sharedContext.MakeCurrent();
        }
        else
        {
            if (!OpenGLContextSettings.WouldResultInSameContext(settings, _sharedContextSettings))
            {
                throw new ArgumentException($"The provided {nameof(OpenGLContextSettings)} would result" +
                                                $"in a different context creation to one previously created. To fix this," +
                                                $" either ensure all of your context settings are identical, or provide an " +
                                                $"external context via the '{nameof(OpenGLContextSettings.ContextToUse)}' field.");
            }
        }

        Interlocked.Increment(ref _sharedContextReferenceCount);

        return _sharedContext;
    }
}
