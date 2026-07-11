namespace Archive.Desktop.Services
{
    public class TokenStorage
    {
        private string? _token;

        public string? Token => _token;

        public void SetToken(string token)
        {
            _token = token;
        }

        public void Clear() => _token = null;
    }
}
