namespace UserManagementSystem.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private const string HeaderName = "X-API-KEY";
        private readonly string _validApiKey;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate requestDelegate, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
        {
            _requestDelegate = requestDelegate;
            _validApiKey = configuration["ApiSettings:ApiKey"] ?? throw new InvalidOperationException("API Key is missing from configuration");
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(HeaderName, out var extractedKey))
            {
                _logger.LogWarning("Request missing API key header");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing API Key");
                return;
            }

            if (!_validApiKey.Equals(extractedKey))
            {
                _logger.LogInformation("Invalid API key attempted: {AttemptedKey}", extractedKey);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            _logger.LogInformation("Valid API key received from: {IP}", context.Connection.RemoteIpAddress);

            try
            {
                await _requestDelegate(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred in API request");
                throw;
            }
        }
    }
}
