using PluriOne.Shared.Observability;

namespace PluriOne.Web.Services;

/// <summary>
/// Agrega el JWT del usuario (desde la cookie HttpOnly) y el correlation id
/// a las llamadas internas hacia los microservicios.
/// </summary>
public sealed class ForwardAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ForwardAuthHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is not null)
        {
            var token = context.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            if (context.Request.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var correlationId))
            {
                request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, correlationId.ToString());
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
