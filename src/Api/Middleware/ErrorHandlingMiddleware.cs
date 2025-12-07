using System.Net;
using System.Text.Json;
namespace Api.Middleware;
public class ErrorHandlingMiddleware : IMiddleware {
  private readonly IWebHostEnvironment _env;
  public ErrorHandlingMiddleware(IWebHostEnvironment env) { _env = env; }
  public async Task InvokeAsync(HttpContext ctx, RequestDelegate next) {
    try { await next(ctx); }
    catch (Exception ex) {
      var traceId = Guid.NewGuid().ToString("N");
      ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      ctx.Response.ContentType = "application/json";
      var payload = new {
        status = ctx.Response.StatusCode,
        code = "server_error",
        message = "An unexpected error occurred.",
        details = _env.IsDevelopment() ? ex.Message : null,
        traceId
      };
      await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
  }
}
