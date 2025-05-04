public class CookieHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Add the X-Requested-With header
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");

        // Forward cookies from the incoming request to the outgoing request
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            var cookies = context.Request.Cookies;
            if (cookies.TryGetValue("X-Access-Token", out var authCookie))
            {
                request.Headers.Add("Cookie", $"X-Access-Token={authCookie}");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
