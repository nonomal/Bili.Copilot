// Copyright (c) Bili Copilot. All rights reserved.

using System.Reflection;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Wgl;
using OpenTK.Platform.Windows;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Bili.Copilot.Controls.Base.PlayerExtensions;

/// <summary>
/// 帧缓冲.
/// </summary>
public unsafe class FrameBuffer : FrameBufferBase
{
    /// <summary>
    /// 初始化帧缓冲.
    /// </summary>
    public FrameBuffer(
        OpenGLRenderContext context,
        int frameBufferWidth,
        int frameBufferHeight,
        double compositionScaleX,
        double compositionScaleY)
    {
        Context = context;
        BufferWidth = Convert.ToInt32(frameBufferWidth * compositionScaleX);
        BufferHeight = Convert.ToInt32(frameBufferHeight * compositionScaleY);

        IDXGISwapChain1* swapChain;

        // SwapChain
        {
            SwapChainDesc1 swapChainDesc = new()
            {
                Width = (uint)BufferWidth,
                Height = (uint)BufferHeight,
                Format = Format.FormatB8G8R8A8Unorm,
                Stereo = 0,
                SampleDesc = new SampleDesc()
                {
                    Count = 1,
                    Quality = 0,
                },
                BufferUsage = DXGI.UsageRenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipDiscard,
                Flags = 0,
                AlphaMode = AlphaMode.Ignore,
            };

            ((IDXGIFactory2*)Context.DxDeviceFactory)->CreateSwapChainForComposition((IUnknown*)Context.DxDeviceHandle, &swapChainDesc, null, &swapChain);

            SwapChainHandle = (IntPtr)swapChain;
        }

        GLFrameBufferHandle = GL.GenFramebuffer();
    }

    /// <summary>
    /// 渲染上下文.
    /// </summary>
    public OpenGLRenderContext Context { get; }

    /// <summary>
    /// OpenGL 颜色渲染缓冲区句柄.
    /// </summary>
    public IntPtr GLColorRenderBufferHandle { get; set; }

    /// <summary>
    /// OpenGL 深度渲染缓冲区句柄.
    /// </summary>
    public IntPtr GLDepthRenderBufferHandle { get; set; }

    /// <summary>
    /// OpenGL 帧缓冲区句柄.
    /// </summary>
    public IntPtr GLFrameBufferHandle { get; set; }

    /// <summary>
    /// DirectX 交互颜色句柄.
    /// </summary>
    public IntPtr DxInteropColorHandle { get; set; }

    /// <summary>
    /// 缓冲区宽度.
    /// </summary>
    public override int BufferWidth { get; protected set; }

    /// <summary>
    /// 缓冲区高度.
    /// </summary>
    public override int BufferHeight { get; protected set; }

    /// <summary>
    /// 交换链句柄.
    /// </summary>
    public override IntPtr SwapChainHandle { get; protected set; }

    /// <summary>
    /// 开始渲染.
    /// </summary>
    public void Begin()
    {
        ID3D11Texture2D* colorbuffer;

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, GLFrameBufferHandle.ToInt32());

        // Texture2D
        {
            var guid = typeof(ID3D11Texture2D).GetTypeInfo().GUID;
            ((IDXGISwapChain1*)SwapChainHandle)->GetBuffer(0, &guid, (void**)&colorbuffer);
        }

        // GL
        {
            GLColorRenderBufferHandle = GL.GenRenderbuffer();
            GLDepthRenderBufferHandle = GL.GenRenderbuffer();

            DxInteropColorHandle = Wgl.DXRegisterObjectNV(Context.GlDeviceHandle, (nint)colorbuffer, (uint)GLColorRenderBufferHandle, (uint)RenderbufferTarget.Renderbuffer, WGL_NV_DX_interop.AccessReadWrite);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, (uint)GLColorRenderBufferHandle);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, GLDepthRenderBufferHandle.ToInt32());
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, BufferWidth, BufferHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, (uint)GLDepthRenderBufferHandle);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, (uint)GLDepthRenderBufferHandle);
        }

        colorbuffer->Release();

        Wgl.DXLockObjectsNV(Context.GlDeviceHandle, 1, new[] { DxInteropColorHandle });

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, GLFrameBufferHandle.ToInt32());
        GL.Viewport(0, 0, BufferWidth, BufferHeight);
    }

    /// <summary>
    /// 结束渲染.
    /// </summary>
    public void End()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        Wgl.DXUnlockObjectsNV(Context.GlDeviceHandle, 1, new[] { DxInteropColorHandle });

        Wgl.DXUnregisterObjectNV(Context.GlDeviceHandle, DxInteropColorHandle);

        GL.DeleteRenderbuffer(GLColorRenderBufferHandle.ToInt32());
        GL.DeleteRenderbuffer(GLDepthRenderBufferHandle.ToInt32());

        ((IDXGISwapChain1*)SwapChainHandle)->Present(0, 0);
    }

    /// <summary>
    /// 更新大小.
    /// </summary>
    public void UpdateSize(
        int framebufferWidth,
        int framebufferHeight,
        double compositionScaleX,
        double compositionScaleY)
    {
        BufferWidth = Convert.ToInt32(framebufferWidth * compositionScaleX);
        BufferHeight = Convert.ToInt32(framebufferHeight * compositionScaleY);

        ((IDXGISwapChain1*)SwapChainHandle)->ResizeBuffers(2, (uint)BufferWidth, (uint)BufferHeight, Format.FormatUnknown, 0);
        ((IDXGISwapChain2*)SwapChainHandle)->SetMatrixTransform(new Matrix3X2F { DXGI11 = 1.0f / (float)compositionScaleX, DXGI22 = 1.0f / (float)compositionScaleY });
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        GL.DeleteFramebuffer(GLFrameBufferHandle.ToInt32());

        Wgl.DXUnregisterObjectNV(Context.GlDeviceHandle, DxInteropColorHandle);
        GL.DeleteRenderbuffer(GLColorRenderBufferHandle.ToInt32());
        GL.DeleteRenderbuffer(GLDepthRenderBufferHandle.ToInt32());

        GC.SuppressFinalize(this);
    }
}
