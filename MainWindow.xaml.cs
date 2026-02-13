using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using LinksAndMore.Views;

namespace LinksAndMore;

public partial class MainWindow
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private const int WM_SETICON = 0x80;
    private const int ICON_SMALL = 0;
    private const int ICON_BIG = 1;

    public MainWindow()
    {
        InitializeComponent();
        
        Loaded += (s, e) => 
        {
            RootNavigation.Navigate(typeof(DashboardPage));
            SetTaskbarIcon();
        };
    }

    private void SetTaskbarIcon()
    {
        var icoPath = System.IO.Path.Combine(
            AppContext.BaseDirectory, "assets", "app_icon.ico");

        if (!System.IO.File.Exists(icoPath))
            return;

        var icon = new Icon(icoPath);
        var hwnd = new WindowInteropHelper(this).Handle;

        SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_BIG, icon.Handle);
        SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_SMALL, icon.Handle);
    }
}