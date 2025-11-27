using SDL;
using SkiaSharp;
using System.Drawing;
using Polychan.GUI;
using Polychan.GUI.Widgets;
using static SDL.SDL3;
#pragma warning disable CS0162 // Unreachable code detected

namespace Polychan.Framework.Platform.Skia;

internal unsafe class SkiaWindow
{
    internal readonly Widget ParentWidget;
    internal readonly SkiaWindow? ParentWindow;

    internal OSWindow WindowHolder { get; }

    private SDL_Window* SdlWindowHandle => WindowHolder.SDLWindowHandle;
    internal SDL_WindowID SdlWindowId => WindowHolder.SDLWindowID;

    #region Hardware Acceleration

    private SDL_GLContextState* SDLGLContext { get; }
    private GRGlInterface? InterfaceGL { get; }
    internal GRContext? GRContext { get; }
    internal GRBackendRenderTarget? RenderTarget { get; private set; }

    #endregion

    #region Software Rendering

    internal SDL_Renderer* SdlRenderer { get; }
    internal SDL_Texture* SdlTexture { get; private set; }
    internal SDL_Surface* SdlSurface { get; private set; }

    internal SKImageInfo ImageInfo { get; private set; }

    #endregion

    private MouseCursor.CursorType? m_currentCursor = null;
    private MouseCursor.CursorType? m_lastCursorShape = null;

    static GRGlInterface m_createdInterface;
    static bool m_createdBaseGLContext;

    public SkiaWindow(Widget parent, string title, WindowFlags flags, SkiaWindow? parentWindow)
    {
        WindowHolder = new OSWindow();
        
        ParentWindow = parentWindow;

        if (Config.HardwareAccel)
        {
            flags |= WindowFlags.OpenGL;
        }
        
        WindowHolder.Window.Create(parentWindow?.WindowHolder.Window ?? null, flags);

        Center();

        WindowHolder.Window.ExitRequested += delegate ()
        {
            ParentWidget?.RequestWindowClose();
        };

        ParentWidget = parent;

        WindowHolder.Window.Title = title;

        if (Config.HardwareAccel)
        {
            if (Config.ShareGlContexts)
            {
                SDLGLContext = SDL_GL_CreateContext(SdlWindowHandle);

                if (!m_createdBaseGLContext)
                {
                    m_createdBaseGLContext = true;
                    m_createdInterface = GRGlInterface.Create();
                    SDL.SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_SHARE_WITH_CURRENT_CONTEXT, 1);
                }

                InterfaceGL = m_createdInterface;
                GRContext = GRContext.CreateGl(m_createdInterface);
            }
            else
            {
                SDLGLContext = SDL_GL_CreateContext(SdlWindowHandle);
                // SDL_GL_MakeCurrent(SDLWindowHandle, SDLGLContext);

                InterfaceGL = GRGlInterface.Create();
                GRContext = GRContext.CreateGl(InterfaceGL);
            }
        }
        else
        {
            SdlRenderer = SDL_CreateRenderer(SdlWindowHandle, (byte*)null);
        }

        WindowRegistry.Register(this);
    }

    public void CreateFrameBuffer(int w, int h)
    {
        ImageInfo = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());

        if (Config.HardwareAccel)
        {
            RenderTarget?.Dispose();

            // SDL_GetWindowSizeInPixels(SDLWindowHandle, &w, &h);
            // SDL_GL_GetIntegerv(SDL.SDL.GLAttribute.FramebufferBinding, out int framebuffer);

            var glInfo = new GRGlFramebufferInfo(0, SKColorType.Rgba8888.ToGlSizedFormat());
            RenderTarget = new GRBackendRenderTarget(w, h, 0, 0, glInfo);
        }
        else
        {
            if (SdlTexture != null)
            {
                SDL_DestroyTexture(SdlTexture);
            }

            // Create SDL texture as the drawing target
            SdlTexture = SDL_CreateTexture(SdlRenderer,
                SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888,
                SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
                w, h);

            if (SdlSurface != null)
            {
                SDL_DestroySurface(SdlSurface);
            }
            // SDLSurface = SDL_CreateSurface(w, h, SDL_GetPixelFormatForMasks(32, 0, 0, 0, 0));
            SdlSurface = SDL_CreateSurface(w, h, SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888);
        }
    }

    public void Dispose()
    {
        WindowRegistry.Remove(this);

        if (!Config.ShareGlContexts)
        {
            GRContext?.Dispose();
            InterfaceGL?.Dispose();
        }

        if (SdlSurface != null)
        {
            SDL_DestroySurface(SdlSurface);
        }
        if (SdlTexture != null)
        {
            SDL_DestroyTexture(SdlTexture);
        }
        if (SDLGLContext != null)
        {
            SDL_GL_DestroyContext(SDLGLContext);
        }
        if (SdlRenderer != null)
        {
            SDL_DestroyRenderer(SdlRenderer);
        }

        WindowHolder.Dispose();
    }

    public void BeginPresent()
    {
        if (Config.HardwareAccel)
        {
            SDL_GL_MakeCurrent(SdlWindowHandle, SDLGLContext);
        }
    }

    public void EndPresent()
    {
        if (Config.HardwareAccel)
        {
            SDL_GL_SwapWindow(SdlWindowHandle);
        }
        else
        {
            SDL_RenderTexture(SdlRenderer, SdlTexture, null, null);
            SDL_RenderPresent(SdlRenderer);
            // SDL_RenderPresent(popupRenderer);
        }
    }

    internal static void PollEvents()
    {
        OSWindow.PollEvents();
    }

    internal void RunCommands()
    {
        WindowHolder.RunCommands();
    }

    /// <summary>
    /// Centers the window.
    /// </summary>
    public void Center()
    {
        // Get the window's current display index
        var displayIndex = SDL_GetDisplayForWindow(SdlWindowHandle);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (displayIndex < 0)
        {
            throw new InvalidOperationException($"Failed to get window display index: {SDL_GetError()}");
        }

        // Get the bounds of the display
        SDL_Rect displayBounds;
        if (SDL_GetDisplayBounds(displayIndex, &displayBounds) != true)
        {
            throw new InvalidOperationException($"Failed to get display bounds: {SDL_GetError()}");
        }

        // Get the window size
        int windowWidth, windowHeight;
        SDL_GetWindowSize(SdlWindowHandle, &windowWidth, &windowHeight);

        // Calculate the centered position
        int centeredX = displayBounds.x + (displayBounds.w - windowWidth) / 2;
        int centeredY = displayBounds.y + (displayBounds.h - windowHeight) / 2;

        // Set the window position
        // SDL_SetWindowPosition(SDLWindowHandle, centeredX, centeredY);
        // (WindowHolder as SDL3Window)!.Position = new(centeredX, centeredY);
        WindowHolder.Window.Position = new Point(centeredX, centeredY);
    }
}