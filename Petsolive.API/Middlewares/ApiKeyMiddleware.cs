namespace Petsolive.API.Middlewares;

using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string APIKEYNAME = "x-api-key";
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
        _apiKey = Environment.GetEnvironmentVariable("API_KEY");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key is missing");
            return;
        }

        if (!_apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized client");
            return;
        }

        await _next(context);
    }
} 