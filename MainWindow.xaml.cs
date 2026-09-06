using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;

namespace DeepSeekDesktop;

public partial class MainWindow : Window
{
    private static readonly string SettingsPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "DeepSeekDesktop", "window.json");

    public MainWindow()
    {
        InitializeComponent();
        ApplySystemTheme();
        LoadPosition();
        Closing += OnClosing;
        SourceInitialized += OnSourceInitialized;
    }

    // --- Tema claro/oscuro de Windows ---

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

    private void ApplySystemTheme()
    {
        var dark = IsDarkMode();

        var bg = dark
            ? System.Windows.Media.Color.FromRgb(0x1F, 0x1F, 0x1F)
            : System.Windows.Media.Color.FromRgb(0xFF, 0xFF, 0xFF);

        Background = new SolidColorBrush(bg);

        // Solo el color de las letras de la barra de menú.
        // Los menús desplegables conservan su estilo nativo.
        var menuFg = dark
            ? System.Windows.Media.Color.FromRgb(0xE8, 0xEE, 0xF5)
            : System.Windows.Media.Color.FromRgb(0x1A, 0x1F, 0x26);
        Resources["MenuFgBrush"] = new SolidColorBrush(menuFg);
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        // Barra de título clara u oscura según el tema del sistema.
        var dark = IsDarkMode() ? 1 : 0;
        var hwnd = new WindowInteropHelper(this).Handle;
        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
    }

    private void LoadPosition()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return;

            var s = JsonSerializer.Deserialize<WindowSettings>(File.ReadAllText(SettingsPath));
            if (s is null) return;

            var wa = SystemParameters.WorkArea;

            // Asegurar que la ventana nunca quede fuera del área de trabajo,
            // de modo que la barra superior siempre sea visible/arrastrable.
            var left = s.Left is double l ? l : double.NaN;
            var top = s.Top is double t ? t : double.NaN;
            var w = Math.Clamp(s.Width ?? 1200, MinWidth, wa.Width);
            var h = Math.Clamp(s.Height ?? 800, MinHeight, wa.Height);

            left = double.IsNaN(left) ? wa.Left + (wa.Width - w) / 2 : Math.Clamp(left, wa.Left, wa.Right - w);
            top = double.IsNaN(top) ? wa.Top + (wa.Height - h) / 2 : Math.Clamp(top, wa.Top, wa.Bottom - h);

            Left = left;
            Top = top;
            Width = w;
            Height = h;
        }
        catch
        {
            // Si hay cualquier problema, se usan los valores por defecto.
        }
    }

    private void SavePosition()
    {
        if (WindowState == WindowState.Maximized) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var s = new WindowSettings { Left = Left, Top = Top, Width = Width, Height = Height };
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(s));
        }
        catch
        {
            // fallo de escritura no crítico
        }
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SavePosition();
    }

    // --- Opciones del menú ---

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        if (WebView.CoreWebView2 is not null)
            WebView.CoreWebView2.Reload();
        else
            WebView.Source = new Uri("https://chat.deepseek.com/");
    }

    private void OpenInBrowser_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://chat.deepseek.com/",
                UseShellExecute = true
            });
        }
        catch
        {
            // si falla al abrir el navegador, no hacer nada
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        new AboutWindow { Owner = this }.ShowDialog();
    }

    private void Support_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = @"file:///C:/Users/nicot/OneDrive/Desktop/Empresa/index.html",
                UseShellExecute = true
            });
        }
        catch
        {
            // si falla al abrir la página, no hacer nada
        }
    }

    private void WebView_InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (e.IsSuccess && WebView.CoreWebView2 is not null)
        {
            WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;

            // Fondo del WebView (área en blanco antes de cargar) según el tema.
            WebView.DefaultBackgroundColor = IsDarkMode()
                ? System.Drawing.Color.FromArgb(0xFF, 0x1F, 0x1F, 0x1F)
                : System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);

            // Que el título de la ventana refleje el título de la página.
            WebView.CoreWebView2.DocumentTitleChanged += (_, _) =>
            {
                if (!string.IsNullOrWhiteSpace(WebView.CoreWebView2.DocumentTitle))
                    Dispatcher.Invoke(() => Title = WebView.CoreWebView2.DocumentTitle);
            };
        }
    }
}

internal class WindowSettings
{
    public double? Left { get; set; }
    public double? Top { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
}