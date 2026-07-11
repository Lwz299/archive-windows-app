using System.Text.Json;
using System.Text.Json.Nodes;
using archive.Models;
using archive.Services;
using Microsoft.Web.WebView2.Core;

namespace archive
{
	public partial class Form1 : Form
	{
		private readonly BookService _bookService = new();
		private const string AppHost = "archive.app";
		private const string DataHost = "archive.data";

		public Form1()
		{
			InitializeComponent();
			Load += Form1_Load;
		}

		private async void Form1_Load(object? sender, EventArgs e)
		{
			var userDataFolder = Path.Combine(_bookService.AppDataFolder, "WebView2UserData");
			Directory.CreateDirectory(userDataFolder);

			var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
			await webView.EnsureCoreWebView2Async(environment);

			var wwwrootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
			webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
				AppHost, wwwrootPath, CoreWebView2HostResourceAccessKind.Allow);
			webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
				DataHost, _bookService.AppDataFolder, CoreWebView2HostResourceAccessKind.Allow);

			webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
			webView.CoreWebView2.Navigate($"https://{AppHost}/index.html");
		}

		private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
		{
			try
			{
				var json = e.TryGetWebMessageAsString();
				var request = JsonNode.Parse(json)!.AsObject();
				var action = request["action"]?.GetValue<string>() ?? string.Empty;
				var requestId = request["requestId"]?.GetValue<string>() ?? string.Empty;
				var payload = request["payload"]?.AsObject();

				JsonObject response = action switch
				{
					"list" => HandleList(payload),
					"add" => HandleAdd(payload),
					"update" => HandleUpdate(payload),
					"delete" => HandleDelete(payload),
					"pickFile" => HandlePickFile(payload),
					"openFile" => HandleOpenFile(payload),
					_ => new JsonObject()
				};

				response["requestId"] = requestId;
				webView.CoreWebView2.PostWebMessageAsJson(response.ToJsonString());
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, $"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private JsonObject HandleList(JsonObject? payload)
		{
			var search = payload?["search"]?.GetValue<string>();
			var category = payload?["category"]?.GetValue<string>();
			var books = _bookService.GetBooks(search, category);
			var categories = _bookService.GetCategories();

			var result = new JsonObject
			{
				["books"] = BooksToJsonArray(books),
				["categories"] = new JsonArray(categories.Select(c => JsonValue.Create(c)!).ToArray())
			};
			return result;
		}

		private JsonObject HandleAdd(JsonObject? payload)
		{
			var book = PayloadToBook(payload);
			var coverPath = payload?["coverPath"]?.GetValue<string>();
			var filePath = payload?["filePath"]?.GetValue<string>();
			var id = _bookService.AddBook(book, coverPath, filePath);
			return new JsonObject { ["success"] = true, ["id"] = id };
		}

		private JsonObject HandleUpdate(JsonObject? payload)
		{
			var book = PayloadToBook(payload);
			book.Id = payload?["id"]?.GetValue<int>() ?? 0;
			var coverPath = payload?["coverPath"]?.GetValue<string>();
			var filePath = payload?["filePath"]?.GetValue<string>();
			var success = _bookService.UpdateBook(book, coverPath, filePath);
			return new JsonObject { ["success"] = success };
		}

		private JsonObject HandleDelete(JsonObject? payload)
		{
			var id = payload?["id"]?.GetValue<int>() ?? 0;
			var success = _bookService.DeleteBook(id);
			return new JsonObject { ["success"] = success };
		}

		private JsonObject HandlePickFile(JsonObject? payload)
		{
			var filter = payload?["filter"]?.GetValue<string>();
			using var dialog = new OpenFileDialog
			{
				Filter = filter == "images"
					? "صور|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
					: "كتب ومستندات|*.pdf;*.epub;*.doc;*.docx;*.txt|كل الملفات|*.*"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				return new JsonObject
				{
					["path"] = dialog.FileName,
					["fileName"] = Path.GetFileName(dialog.FileName)
				};
			}

			return new JsonObject();
		}

		private JsonObject HandleOpenFile(JsonObject? payload)
		{
			var id = payload?["id"]?.GetValue<int>() ?? 0;
			_bookService.OpenBookFile(id);
			return new JsonObject { ["success"] = true };
		}

		private static Book PayloadToBook(JsonObject? payload)
		{
			return new Book
			{
				Title = payload?["title"]?.GetValue<string>() ?? string.Empty,
				Author = payload?["author"]?.GetValue<string>() ?? string.Empty,
				Category = payload?["category"]?.GetValue<string>() ?? string.Empty,
				Publisher = payload?["publisher"]?.GetValue<string>() ?? string.Empty,
				Year = payload?["year"]?.GetValue<int?>(),
				Language = payload?["language"]?.GetValue<string>() ?? string.Empty,
				Notes = payload?["notes"]?.GetValue<string>() ?? string.Empty
			};
		}

		private JsonArray BooksToJsonArray(List<Book> books)
		{
			var array = new JsonArray();
			foreach (var book in books)
			{
				array.Add(new JsonObject
				{
					["id"] = book.Id,
					["title"] = book.Title,
					["author"] = book.Author,
					["category"] = book.Category,
					["publisher"] = book.Publisher,
					["year"] = book.Year,
					["language"] = book.Language,
					["notes"] = book.Notes,
					["filePath"] = book.FilePath,
					["coverUrl"] = ToVirtualUrl(book.CoverPath)
				});
			}
			return array;
		}

		private string? ToVirtualUrl(string? absolutePath)
		{
			if (string.IsNullOrWhiteSpace(absolutePath))
			{
				return null;
			}

			var relative = Path.GetRelativePath(_bookService.AppDataFolder, absolutePath)
				.Replace(Path.DirectorySeparatorChar, '/');

			return $"https://{DataHost}/{relative}";
		}
	}
}
