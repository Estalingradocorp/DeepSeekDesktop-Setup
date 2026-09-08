using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DeepSeekDesktop;

/// <summary>
/// Servidor HTTP local (solo localhost) que expone una API para que opencode
/// u otros agentes controlen la aplicacion.
/// </summary>
public sealed class LocalApiServer
{
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;

    // Callbacks inyectados por MainWindow (se ejecutan en el hilo de red;
    // MainWindow debe despachar a la UI).
    public Func<Task<object>>? GetStatus { get; set; }
    public Func<string, Task>? Navigate { get; set; }
    public Func<Task>? Reload { get; set; }
    public Func<string, Task<object>>? ExecuteScript { get; set; }
    public Func<string, Task<object>>? SetTheme { get; set; }

    public int Port { get; private set; }

    public void Start(int port)
    {
        Port = port;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _listener.Start();
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => LoopAsync(_cts.Token));
    }

    public void Stop()
    {
        _cts?.Cancel();
        try { _listener?.Stop(); } catch { }
        _listener?.Close();
    }

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await _listener!.GetContextAsync();
            }
            catch
            {
                break;
            }
            _ = Task.Run(() => HandleAsync(ctx));
        }
    }

    private async Task HandleAsync(HttpListenerContext ctx)
    {
        try
        {
            var req = ctx.Request;
            var path = req.Url?.AbsolutePath?.Trim('/') ?? "";
            var body = await ReadBodyAsync(req);
            object result;

            switch (req.HttpMethod + " " + path)
            {
                case "GET ":
                    result = new
                    {
                        name = "DeepSeek Desktop API",
                        version = "1.0.0",
                        endpoints = new[] { "/status", "/info", "/reload", "/navigate", "/execute", "/theme" }
                    };
                    break;

                case "GET status":
                    result = GetStatus != null ? await GetStatus() : new { };
                    break;

                case "GET info":
                    result = new
                    {
                        name = "DeepSeek Desktop",
                        company = "Estalingrado Corp",
                        url = "https://chat.deepseek.com/",
                        description = "Launcher de escritorio de Estalingrado Corp para chat.deepseek.com"
                    };
                    break;

                case "POST reload":
                    if (Reload != null) await Reload();
                    result = new { ok = true };
                    break;

                case "POST navigate":
                    var url = GetString(body, "url");
                    if (string.IsNullOrEmpty(url))
                    {
                        ctx.Response.StatusCode = 400;
                        result = new { error = "campo 'url' requerido" };
                    }
                    else
                    {
                        if (Navigate != null) await Navigate(url);
                        result = new { ok = true, url };
                    }
                    break;

                case "POST execute":
                    var script = GetString(body, "script");
                    if (string.IsNullOrEmpty(script))
                    {
                        ctx.Response.StatusCode = 400;
                        result = new { error = "campo 'script' requerido" };
                    }
                    else
                    {
                        result = ExecuteScript != null ? await ExecuteScript(script) : new { error = "no handler" };
                    }
                    break;

                case "POST theme":
                    var theme = GetString(body, "theme") ?? "auto";
                    result = SetTheme != null ? await SetTheme(theme) : new { };
                    break;

                default:
                    ctx.Response.StatusCode = 404;
                    result = new { error = "endpoint no encontrado" };
                    break;
            }

            await WriteJsonAsync(ctx, result);
        }
        catch (Exception ex)
        {
            ctx.Response.StatusCode = 500;
            await WriteJsonAsync(ctx, new { error = ex.Message });
        }
    }

    private static async Task<string> ReadBodyAsync(HttpListenerRequest req)
    {
        if (req.HttpMethod != "POST" || req.ContentLength64 <= 0) return "";
        using var reader = new StreamReader(req.InputStream, req.ContentEncoding ?? Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private static string? GetString(string body, string key)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty(key, out var v))
                return v.ValueKind == JsonValueKind.String ? v.GetString() : v.GetRawText();
        }
        catch { }
        return null;
    }

    private static async Task WriteJsonAsync(HttpListenerContext ctx, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);
        ctx.Response.ContentType = "application/json; charset=utf-8";
        ctx.Response.ContentLength64 = bytes.Length;
        await ctx.Response.OutputStream.WriteAsync(bytes);
        ctx.Response.Close();
    }
}