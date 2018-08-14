using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace apps.vaicys.com.Middleware
{
    public static class RedirectMiddlewareExtensions
    {
        public static IApplicationBuilder UseSchemeAndHostRedirect(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RedirectMiddleware>();
        }
    }

    public class RedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public RedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (SchemeNeedsUpdating(context, out var newScheme) ||
                HostNeedsUpdating(context, out var newHost))
            {
                var redirectUrl = UriHelper.BuildAbsolute(
                    newScheme,
                    newHost,
                    context.Request.PathBase,
                    context.Request.Path,
                    context.Request.QueryString);
                context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
                context.Response.Headers[HeaderNames.Location] = redirectUrl;
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        private bool HostNeedsUpdating(HttpContext context, out HostString host)
        {
            if (context.Request.Host.Value.StartsWith("localhost") ||
                context.Request.Host.Value.StartsWith("www."))
            {
                host = context.Request.Host;
                return false;
            }
            host = new HostString($"www.{context.Request.Host.Value}");
            return true;
        }

        private bool SchemeNeedsUpdating(HttpContext context, out string scheme)
        {
            if (context.Request.Headers["X-Forwarded-Proto"] != "http")
            {
                scheme = context.Request.Scheme;
                return false;
            }
            scheme = "https";
            return true;
        }
    }
}
