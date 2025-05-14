using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace WebApi.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionHandlingMiddleware(RequestDelegate next) =>
        _next = next;

        public async Task Invoke(HttpContext http)
        {
            try
            {
                await _next(http);
            }
            catch (Exception ex)
            {
                http.Response.ContentType = "application/json";
                http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var payload = JsonSerializer.Serialize(new { Message = ex.Message });
                await http.Response.WriteAsync(payload);
            }
        }
    }
}
