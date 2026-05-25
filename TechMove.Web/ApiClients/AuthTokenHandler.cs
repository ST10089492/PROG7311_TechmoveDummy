using System.Net.Http.Headers;

namespace TechMove.Web.ApiClients
{
    // attaches the jwt to every outgoing api call when the user is logged in
    // this is why the controllers never have to think about the token themselves
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly TokenStore _store;

        public AuthTokenHandler(TokenStore store)
        {
            _store = store;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_store.IsLoggedIn)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _store.Token);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
