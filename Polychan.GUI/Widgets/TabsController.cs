using Polychan.GUI.Layouts;
using SkiaSharp;

namespace Polychan.GUI.Widgets;

public class TabsController : Widget, IPaintHandler
{
    private const int TAB_HEIGHT = 30;

    class TabWidget : Widget, IPaintHandler, IMouseClickHandler, IMouseDownHandler
    {
        public string Title;
        public Widget? Content = null;

        public Action? OnClick;
        
        public TabWidget(Widget? content, string title, Widget? parent = null) : base(parent)
        {
            this.Content = content;
            this.Title = title;

            Width = 200;
            Height = TAB_HEIGHT;
        }
        
        public void OnPaint(SKCanvas canvas)
        {
            using var paint = new SKPaint();
            paint.Color = SKColors.Blue;
            canvas.DrawRect(0, 0, Width, Height, paint);

            paint.Color = Application.Palette.Get(ColorRole.Text);
            canvas.DrawText(Title, 8, Height * 0.5f, SKTextAlign.Left, Application.DefaultFont, paint);
        }

        public bool OnMouseClick(MouseEvent evt)
        {
            return true;
        }

        public bool OnMouseDown(MouseEvent evt)
        {
            OnClick?.Invoke();
            return true;
        }
    }

    private readonly NullWidget m_tabsHolder;
    private readonly NullWidget m_contentHolder;
    private readonly List<TabWidget> m_tabs = [];
    private int m_currentTab;
    
    public TabsController(Widget? parent = null) : base(parent)
    {
        CatchCursorEvents = false;
        Layout = new VBoxLayout()
        {
            Spacing = 0,
        };
        m_tabsHolder = new NullWidget(this)
        {
            Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Fixed),
            Height = TAB_HEIGHT,
            Layout = new HBoxLayout(),
            CatchCursorEvents = false
        };
        m_contentHolder = new NullWidget(this)
        {
            Fitting = new FitPolicy(FitPolicy.Policy.Expanding, FitPolicy.Policy.Expanding),
            Layout = new HBoxLayout()
            {
                Padding = new Padding(0)
            },
            CatchCursorEvents = false
        };
    }

    public int AddTab(Widget pageWidget, string label)
    {
        var tabIndex = m_tabs.Count;
        
        // Create tab content
        /*
        var frame = new ShapedFrame(m_contentHolder)
        {
            Fitting = FitPolicy.ExpandingPolicy,
            Layout = new HBoxLayout()
            {
                Padding = new Padding(0)
            }
        };
        */
        pageWidget.SetParent(m_contentHolder);

        // Create tab widget
        var tabWidget = new TabWidget(pageWidget, label, m_tabsHolder);
        tabWidget.X = tabIndex * 200;
        tabWidget.OnClick = () => switchTab(tabIndex);
        m_tabs.Add(tabWidget);
        
        disableAllTabsExcept(tabIndex);
        return tabIndex;
    }

    public void OnPaint(SKCanvas canvas)
    {
        return;
        using var paint = new SKPaint();
        paint.Color = SKColors.Green;
        canvas.DrawRect(0, 0, Width, Height, paint);
    }

    private void switchTab(int index)
    {
        Console.WriteLine(index);
        disableAllTabsExcept(index);
        
        var content = m_tabs[index].Content;
        if (content != null)
            content.Visible = true;
    }

    private void disableAllTabsExcept(int except)
    {
        for (var i = 0; i < m_tabs.Count; i++)
        {
            if (i == except)
                continue;

            var content = m_tabs[i].Content;
            if (content != null) content.Visible = false;
        }
    }
}