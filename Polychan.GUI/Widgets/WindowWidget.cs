using Polychan.Framework.Platform;

namespace Polychan.GUI.Widgets;

public class WindowWidget : Widget
{
    protected WindowFlags WindowFlags = WindowFlags.None;

    /// <summary>
    /// The title of the window.
    /// </summary>
    public string Title
    {
        get => NativeWindow?.WindowHolder.Window.Title ?? string.Empty;
        set
        {
            if (NativeWindow != null)
            {
                NativeWindow.WindowHolder.Window.Title = value;
            }
        }
    }

    public WindowWidget(WindowType type, Widget? parent = null) : base(parent, type)
    {
        CreateWinID();
    }
}