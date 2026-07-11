using archive.Data;
using archive.Models;

namespace archive.Services
{
	public class BookService
	{
		private readonly BookRepository _repository;
		private readonly string _appDataFolder;
		private readonly string _coversFolder;
		private readonly string _filesFolder;

		public string AppDataFolder => _appDataFolder;

		public BookService()
		{
			_appDataFolder = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"ArchiveApp");

			_coversFolder = Path.Combine(_appDataFolder, "Covers");
			_filesFolder = Path.Combine(_appDataFolder, "Files");

			Directory.CreateDirectory(_appDataFolder);
			Directory.CreateDirectory(_coversFolder);
			Directory.CreateDirectory(_filesFolder);

			var dbPath = Path.Combine(_appDataFolder, "archive.db");
			_repository = new BookRepository(dbPath);
		}

		public List<Book> GetBooks(string? search, string? category) => _repository.GetAll(search, category);

		public Book? GetBook(int id) => _repository.GetById(id);

		public List<string> GetCategories() => _repository.GetCategories();

		public int AddBook(Book book, string? sourceCoverPath, string? sourceFilePath)
		{
			if (!string.IsNullOrWhiteSpace(sourceCoverPath) && File.Exists(sourceCoverPath))
			{
				book.CoverPath = CopyIntoStorage(sourceCoverPath, _coversFolder);
			}

			if (!string.IsNullOrWhiteSpace(sourceFilePath) && File.Exists(sourceFilePath))
			{
				book.FilePath = CopyIntoStorage(sourceFilePath, _filesFolder);
			}

			return _repository.Add(book);
		}

		public bool UpdateBook(Book book, string? sourceCoverPath, string? sourceFilePath)
		{
			if (!string.IsNullOrWhiteSpace(sourceCoverPath) && File.Exists(sourceCoverPath))
			{
				book.CoverPath = CopyIntoStorage(sourceCoverPath, _coversFolder);
			}

			if (!string.IsNullOrWhiteSpace(sourceFilePath) && File.Exists(sourceFilePath))
			{
				book.FilePath = CopyIntoStorage(sourceFilePath, _filesFolder);
			}

			return _repository.Update(book);
		}

		public bool DeleteBook(int id)
		{
			var book = _repository.GetById(id);
			if (book == null)
			{
				return false;
			}

			TryDeleteFile(book.CoverPath);
			TryDeleteFile(book.FilePath);

			return _repository.Delete(id);
		}

		public void OpenBookFile(int id)
		{
			var book = _repository.GetById(id);
			if (book?.FilePath != null && File.Exists(book.FilePath))
			{
				var psi = new System.Diagnostics.ProcessStartInfo(book.FilePath)
				{
					UseShellExecute = true
				};
				System.Diagnostics.Process.Start(psi);
			}
		}

		private static string CopyIntoStorage(string sourcePath, string destinationFolder)
		{
			var extension = Path.GetExtension(sourcePath);
			var fileName = $"{Guid.NewGuid()}{extension}";
			var destinationPath = Path.Combine(destinationFolder, fileName);
			File.Copy(sourcePath, destinationPath, overwrite: true);
			return destinationPath;
		}

		private static void TryDeleteFile(string? path)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch
			{
				// ignore cleanup failures
			}
		}
	}
}
