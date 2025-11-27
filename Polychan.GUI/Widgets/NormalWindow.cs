using System.Diagnostics;
using SkiaSharp;

namespace Polychan.GUI.Widgets;

public class NormalWindow : WindowWidget, IPaintHandler, IResizeHandler
{
    public MenuBar? MenuBar { get; set; }
    public ToolBar? ToolBar { get; set; }
    public Widget? CentralWidget = null;

    public NormalWindow(Widget? parent = null) : base(WindowType.Window, parent)
    {

    }

    #region Events

    public void OnPaint(SKCanvas canvas)
    {
        canvas.Clear(EffectivePalette.Get(ColorGroup.Active, ColorRole.Window));
    }

    public void OnResize(int width, int height)
    {
        MenuBar?.Resize(width, MenuBar.Height);
        ToolBar?.Resize(width, ToolBar.Height);
    }

    public void SetIconFromStream(Stream imageStream)
    {
        Debug.Assert(NativeWindow != null);
        NativeWindow.WindowHolder.Window.SetIconFromStream(imageStream);
    }

    #endregion
}