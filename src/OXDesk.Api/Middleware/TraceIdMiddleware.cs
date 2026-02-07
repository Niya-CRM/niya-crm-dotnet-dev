using System.Diagnostics;

namespace OXDesk.Api.Middleware
{
    public class TraceIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string TraceIdHeader = "X-Trace-Id";

        public TraceIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Use the W3C TraceId from the current Activity (set by ASP.NET Core)
            string traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

            // Store in HttpContext for downstream usage
            context.Items[TraceIdHeader] = traceId;

            // Add to response header
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[TraceIdHeader] = traceId;
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
