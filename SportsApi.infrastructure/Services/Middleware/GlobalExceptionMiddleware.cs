using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SportsApi.infrastructure.Services.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Acceso no autorizado: {Path}", context.Request.Path);
            await WriteResponse(context, HttpStatusCode.Unauthorized,
                "No autorizado. Verifique su token de autenticación.");
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "Violación de restricción única: {Path}", context.Request.Path);
            await WriteResponse(context, HttpStatusCode.Conflict,
                "El registro que intenta crear ya existe. Verifique los datos e intente nuevamente.");
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error al guardar en base de datos: {Path}", context.Request.Path);
            await WriteResponse(context, HttpStatusCode.BadRequest,
                "Ocurrió un error al guardar los datos. Verifique que la información sea correcta.");
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Argumento inválido: {Path}", context.Request.Path);
            await WriteResponse(context, HttpStatusCode.BadRequest, 
                "Los datos enviados no son válidos. Verifique e intente nuevamente.");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Operación inválida: {Path}", context.Request.Path);
            await WriteResponse(context, HttpStatusCode.BadRequest,
                "No se pudo completar la operación. Verifique los datos e intente nuevamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado: {Path}", context.Request.Path);
            await WriteResponse(context, HttpStatusCode.InternalServerError,
                "Ocurrió un error inesperado. Intente nuevamente más tarde.");
        }
    }

    private static async Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            error = message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // SQL Server error 2601 = unique index violation, 2627 = unique constraint violation
        var inner = ex.InnerException?.Message ?? "";
        return inner.Contains("Cannot insert duplicate key") ||
               inner.Contains("UNIQUE KEY constraint") ||
               inner.Contains("unique index");
    }
}

