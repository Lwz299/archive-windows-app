namespace Archive.Infrastructure.Storage
{
    public class LocalStorageService
    {
        private readonly string _rootPath;

        public LocalStorageService(string rootPath)
        {
            _rootPath = rootPath;
            Directory.CreateDirectory(_rootPath);
        }

        public string SaveFile(string sourcePath, string destinationFolder)
        {
            var folder = Path.Combine(_rootPath, destinationFolder);
            Directory.CreateDirectory(folder);
            var extension = Path.GetExtension(sourcePath);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var destinationPath = Path.Combine(folder, fileName);
            File.Copy(sourcePath, destinationPath, overwrite: true);
            return destinationPath;
        }

        public bool DeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return false;
            }

            File.Delete(path);
            return true;
        }
    }
}
