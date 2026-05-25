namespace TechMove.Web.ApiClients
{
    // keeps the logged in user's jwt in session so it survives between requests
    public class TokenStore
    {
        private const string Key = "access_token";
        private readonly IHttpContextAccessor _ctx;

        public TokenStore(IHttpContextAccessor ctx)
        {
            _ctx = ctx;
        }

        public string? Token => _ctx.HttpContext?.Session.GetString(Key);

        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

        public void Save(string token) => _ctx.HttpContext?.Session.SetString(Key, token);

        public void Clear() => _ctx.HttpContext?.Session.Remove(Key);
    }
}
