using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Threading.Tasks;

namespace NiyaCRM.Api.Middleware
{
    public class DomainMiddleware
    {
        private readonly RequestDelegate _next;

        public DomainMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Get the domain from the request
            string domain = context.Request.Host.Host;

            // Add domain to the log context
            using (LogContext.PushProperty("Domain", domain))
            {
                await _next(context);
            }
        }
    }
}
