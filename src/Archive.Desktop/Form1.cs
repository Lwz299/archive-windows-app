using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Archive.Contracts.Requests;
using Archive.Contracts.Responses;
using Archive.Desktop.ApiClients;
using Archive.Desktop.Services;
using Microsoft.Web.WebView2.Core;

namespace Archive.Desktop;

public partial class Form1 : Form
{
    private readonly HttpClient _httpClient;
    private readonly AuthApiClient _authClient;
    private readonly BookApiClient _bookClient;
    private readonly CategoryApiClient _categoryClient;
    private readonly UserApiClient _userClient;
    private readonly SessionService _session;

    public Form1()
    {
        InitializeComponent();

        var apiBaseUrl = LoadApiBaseUrl();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl),
            // Render free tier may sleep; first request can take up to ~60s
            Timeout = TimeSpan.FromSeconds(100)
        };
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        _authClient = new AuthApiClient(_httpClient);
        _bookClient = new BookApiClient(_httpClient);
        _categoryClient = new CategoryApiClient(_httpClient);
        _userClient = new UserApiClient(_httpClient);
        _session = new SessionService();

        Text = $"أرشيف الكتب — {apiBaseUrl.TrimEnd('/')}";
        Load += Form1_Load;
    }

    private static string LoadApiBaseUrl()
    {
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        const string fallback = "https://archive-windows-app.onrender.com/";
        try
        {
            if (!File.Exists(configPath))
            {
                return fallback;
            }

            using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
            if (doc.RootElement.TryGetProperty("ApiBaseUrl", out var url)
                && url.GetString() is { Length: > 0 } value)
            {
                value = value.Trim();
                if (!value.EndsWith('/'))
                {
                    value += "/";
                }

                return value;
            }
        }
        catch
        {
            // Fall back to production API
        }

        return fallback;
    }

    private async void Form1_Load(object? sender, EventArgs e)
    {
        var webFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Web");
        var htmlPath = Path.Combine(webFolder, "login.html");
        if (!File.Exists(htmlPath))
        {
            MessageBox.Show($"تعذر العثور على الصفحة: {htmlPath}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "appassets.local", webFolder, CoreWebView2HostResourceAccessKind.Allow);
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            webView.CoreWebView2.Navigate("https://appassets.local/login.html");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"فشل تشغيل WebView2: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            // JS posts JSON.stringify(...); prefer string form like the legacy host
            string raw;
            try
            {
                raw = e.TryGetWebMessageAsString();
            }
            catch (ArgumentException)
            {
                raw = e.WebMessageAsJson;
            }

            var msg = JsonSerializer.Deserialize<HostMessage>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (msg == null || string.IsNullOrWhiteSpace(msg.Action)) return;

            switch (msg.Action)
            {
                case "checkSetup":
                    await SendResponse(msg.RequestId, new { hasUsers = await _authClient.CheckSetupAsync() });
                    break;
                case "login":
                    await HandleLogin(msg);
                    break;
                case "register":
                    await HandleRegister(msg);
                    break;
                case "logout":
                    _session.ClearSession();
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                    webView.CoreWebView2.Navigate("https://appassets.local/login.html");
                    await SendResponse(msg.RequestId, new { success = true });
                    break;
                case "session":
                    await SendResponse(msg.RequestId, new
                    {
                        success = _session.IsAuthenticated,
                        username = _session.Username,
                        role = _session.Role,
                        permissions = _session.Permissions
                    });
                    break;
                case "add":
                    await HandleAddBook(msg);
                    break;
                case "update":
                    await HandleUpdateBook(msg);
                    break;
                case "list":
                    await HandleListBooks(msg);
                    break;
                case "delete":
                    await HandleDeleteBook(msg);
                    break;
                case "pickFile":
                    await HandlePickFile(msg);
                    break;
                case "openFile":
                    await HandleOpenFile(msg);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            await SendResponse(null, new { error = ex.Message });
        }
    }

    private async Task HandleLogin(HostMessage msg)
    {
        var payload = msg.Payload.Deserialize<LoginRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (payload == null)
        {
            await SendResponse(msg.RequestId, new { success = false, error = "بيانات غير صحيحة" });
            return;
        }

        try
        {
            var result = await _authClient.LoginAsync(payload);
            _session.SetSession(result.Username, result.Role, result.Token, result.Permissions);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
            await SendResponse(msg.RequestId, new { success = true, role = result.Role, permissions = result.Permissions });
            // Navigate to main app after successful login
            webView.CoreWebView2.Navigate("https://appassets.local/index.html");
        }
        catch (Exception ex)
        {
            await SendResponse(msg.RequestId, new { success = false, error = ex.Message });
        }
    }

    private async Task HandleRegister(HostMessage msg)
    {
        var payload = msg.Payload.Deserialize<RegisterRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (payload == null)
        {
            await SendResponse(msg.RequestId, new { success = false, error = "بيانات غير صحيحة" });
            return;
        }
        try
        {
            await _authClient.RegisterAsync(payload);
            // After successful registration, navigate to main app (login page will treat registration as success)
            await SendResponse(msg.RequestId, new { success = true });
            webView.CoreWebView2.Navigate("https://appassets.local/index.html");
        }
        catch (Exception ex)
        {
            await SendResponse(msg.RequestId, new { success = false, error = ex.Message });
        }
    }

    private async Task HandleAddBook(HostMessage msg)
    {
        var payload = msg.Payload.Deserialize<CreateBookRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (payload == null)
        {
            await SendResponse(msg.RequestId, new { success = false, error = "بيانات غير صحيحة" });
            return;
        }

        try
        {
            await _bookClient.CreateBookAsync(payload);
            await SendResponse(msg.RequestId, new { success = true });
        }
        catch (Exception ex)
        {
            await SendResponse(msg.RequestId, new { success = false, error = ex.Message });
        }
    }

    private async Task HandleUpdateBook(HostMessage msg)
    {
        var payload = msg.Payload.Deserialize<UpdateBookRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (payload == null)
        {
            await SendResponse(msg.RequestId, new { success = false, error = "بيانات غير صحيحة" });
            return;
        }

        try
        {
            await _bookClient.UpdateBookAsync(payload);
            await SendResponse(msg.RequestId, new { success = true });
        }
        catch (Exception ex)
        {
            await SendResponse(msg.RequestId, new { success = false, error = ex.Message });
        }
    }

    private async Task HandleListBooks(HostMessage msg)
    {
        try
        {
            var filter = msg.Payload.ValueKind is JsonValueKind.Object
                ? msg.Payload.Deserialize<BookFilter>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                : null;
            var books = await _bookClient.GetBooksAsync(filter?.Search, filter?.Category) ?? [];
            var categories = await _categoryClient.GetCategoriesAsync() ?? [];
            await SendResponse(msg.RequestId, new
            {
                books,
                categories = categories.Select(c => c.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToArray()
            });
        }
        catch (Exception ex)
        {
            await SendResponse(msg.RequestId, new { books = Array.Empty<object>(), categories = Array.Empty<string>(), error = ex.Message });
        }
    }
    
    private async Task HandleDeleteBook(HostMessage msg)
    {
        var payload = msg.Payload.Deserialize<DeletePayload>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (payload == null || payload.Id <= 0)
        {
            await SendResponse(msg.RequestId, new { success = false, error = "بيانات غير صحيحة" });
            return;
        }

        try
        {
            await _bookClient.DeleteBookAsync(payload.Id);
            await SendResponse(msg.RequestId, new { success = true });
        }
        catch (Exception ex)
        {
            await SendResponse(msg.RequestId, new { success = false, error = ex.Message });
        }
    }

    private async Task HandlePickFile(HostMessage msg)
    {
        var payload = msg.Payload.Deserialize<PickFilePayload>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (payload == null)
        {
            await SendResponse(msg.RequestId, new { path = (string?)null });
            return;
        }

        using var dialog = new OpenFileDialog();
        dialog.Filter = payload.Filter == "images" ? "Image Files|*.png;*.jpg;*.jpeg;*.gif" : "Book Files|*.pdf;*.epub;*.txt;*.docx";
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            await SendResponse(msg.RequestId, new { path = dialog.FileName, fileName = Path.GetFileName(dialog.FileName) });
            return;
        }

        await SendResponse(msg.RequestId, new { path = (string?)null });
    }

    private async Task HandleOpenFile(HostMessage msg)
    {
        var payload = msg.Payload.Deserialize<DeletePayload>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (payload == null || payload.Id <= 0)
        {
            await SendResponse(msg.RequestId, new { success = false, error = "بيانات غير صحيحة" });
            return;
        }

        try
        {
            var books = await _bookClient.GetBooksAsync(null, null) ?? [];
            var match = books.FirstOrDefault(b => b.Id == payload.Id);
            var path = match?.FilePath ?? match?.FileUrl;
            if (match != null && !string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
                await SendResponse(msg.RequestId, new { success = true });
                return;
            }

            await SendResponse(msg.RequestId, new { success = false, error = "ملف غير موجود" });
        }
        catch (Exception ex)
        {
            await SendResponse(msg.RequestId, new { success = false, error = ex.Message });
        }
    }

    private Task SendResponse(string? requestId, object response)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var responseJson = JsonSerializer.Serialize(response, options);
        string finalJson;
        if (!string.IsNullOrWhiteSpace(responseJson) && responseJson.TrimStart().StartsWith("{"))
        {
            var trimmed = responseJson.TrimStart();
            finalJson = "{" + "\"requestId\":" + JsonSerializer.Serialize(requestId) + "," + trimmed.Substring(1);
        }
        else
        {
            finalJson = JsonSerializer.Serialize(new { requestId, body = response }, options);
        }

        webView.CoreWebView2.PostWebMessageAsJson(finalJson);
        return Task.CompletedTask;
    }

    private record HostMessage(string Action, JsonElement Payload, string RequestId);
    private record BookFilter(string? Search, string? Category);
    private record DeletePayload(int Id);
    private record PickFilePayload(string Filter);
}
