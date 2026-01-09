using SistemaTurnos.Application.Exceptions;
using SistemaTurnos.Domain.Exceptions;
using System.Text.Json;

namespace SistemaTurnos.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BusinessException ex)
            {
                var statusCode = ex.Message.Contains("pasado") 
                    ? StatusCodes.Status409Conflict 
                    : StatusCodes.Status400BadRequest;

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        error = ex.Message
                    })
                );
            }
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        error = ex.Message
                    })
                );
            }
            catch (NotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        error = ex.Message
                    })
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stack = ex.StackTrace
                }));
            }
        }
    }
}
