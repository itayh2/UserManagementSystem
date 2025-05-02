namespace UserManagementSystem.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private const string HeaderName = "X-API-KEY";
        private readonly string _validApiKey;

        public ApiKeyMiddleware(RequestDelegate requestDelegate, IConfiguration configuration)
        {
            _requestDelegate = requestDelegate;
            _validApiKey = configuration["ApiSettings:ApiKey"] ?? throw new InvalidOperationException("API Key is missing from configuration");
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(HeaderName, out var extractedKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing API Key");
                return;
            }

            if (!_validApiKey.Equals(extractedKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            await _requestDelegate(context);
        }
    }
}
