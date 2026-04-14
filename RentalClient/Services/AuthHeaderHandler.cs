using System.Net.Http;
using System.Net.Http.Headers;

namespace RentalClient.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IUserSession _userSession;
    
    public AuthHeaderHandler(IUserSession userSession)
    {
        _userSession = userSession;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_userSession.IsAuthenticated)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userSession.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}