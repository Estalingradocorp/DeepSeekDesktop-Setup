using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace DeepSeekDesktop;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        ApplyTheme();
    }

    private static bool IsDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
        }
        catch
        {
            return false;
        }
    }

    private void ApplyTheme()
    {
        var dark = IsDarkMode();

        var bg = dark ? System.Windows.Media.Color.FromRgb(0x1B, 0x1F, 0x24) : System.Windows.Media.Color.FromRgb(0xFF, 0xFF, 0xFF);
        var header = dark ? System.Windows.Media.Color.FromRgb(0x21, 0x27, 0x2E) : System.Windows.Media.Color.FromRgb(0xF4, 0xF6, 0xF8);
        var text = dark ? System.Windows.Media.Color.FromRgb(0xE8, 0xEE, 0xF5) : System.Windows.Media.Color.FromRgb(0x1B, 0x22, 0x2A);
        var soft = dark ? System.Windows.Media.Color.FromRgb(0x9A, 0xA8, 0xB6) : System.Windows.Media.Color.FromRgb(0x6A, 0x7A, 0x8A);
        var accent = dark ? System.Windows.Media.Color.FromRgb(0x5E, 0xA8, 0xE8) : System.Windows.Media.Color.FromRgb(0x0B, 0x66, 0xC3);
        var divider = dark ? System.Windows.Media.Color.FromRgb(0x2C, 0x33, 0x3B) : System.Windows.Media.Color.FromRgb(0xE2, 0xE6, 0xEA);

        Resources["BgBrush"] = new SolidColorBrush(bg);
        Resources["HeaderBgBrush"] = new SolidColorBrush(header);
        Resources["TextMainBrush"] = new SolidColorBrush(text);
        Resources["TextSoftBrush"] = new SolidColorBrush(soft);
        Resources["AccentBrush"] = new SolidColorBrush(accent);
        Resources["DividerBrush"] = new SolidColorBrush(divider);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}