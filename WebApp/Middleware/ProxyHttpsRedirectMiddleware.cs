using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace apps.vaicys.com.Middleware
{
    public static class ProxyHttpsRedirectMiddlewareExtensions
    {
        public static IApplicationBuilder UseProxyHttpsRedirect(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ProxyHttpsRedirectMiddleware>();
        }
    }

    public class ProxyHttpsRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public ProxyHttpsRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers["X-Forwarded-Proto"] != "http")
            {
                await _next.Invoke(context);
                return;
            }

            var redirectUrl = UriHelper.BuildAbsolute(
                "https",
                context.Request.Host,
                context.Request.PathBase,
                context.Request.Path,
                context.Request.QueryString);

            context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
            context.Response.Headers[HeaderNames.Location] = redirectUrl;
        }
    }
}
