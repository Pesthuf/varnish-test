using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var lastModifiedDate = DateTime.UtcNow;
var backendHits = 0;
var backend304Hits = 0;
var varnishShouldCache = true;

app.MapGet("/", async (context) =>
{
    var responseHeaders = context.Response.GetTypedHeaders();
    responseHeaders.CacheControl = new CacheControlHeaderValue
    {
        Public = varnishShouldCache,
        Private = !varnishShouldCache,
        MaxAge = TimeSpan.Zero,
        SharedMaxAge = TimeSpan.Zero, // This represents the time varnish may cache the content - *without* having to validate.
        MustRevalidate = true,
    };

    var dateTimeOffset = responseHeaders.LastModified = lastModifiedDate;
    responseHeaders.Date = lastModifiedDate;
    
    var ifModifiedSince = context.Request.GetTypedHeaders().IfModifiedSince;

    Console.WriteLine(JsonSerializer.Serialize(context.Request.Headers));
    
    context.Response.Headers["X-Backend-Hits"] = backendHits.ToString();
    context.Response.Headers["X-Backend-304-Hits"] = backend304Hits.ToString();

    Console.WriteLine(ifModifiedSince?.ToUnixTimeSeconds().ToString() ?? "<No last modified>");
    Console.WriteLine(new DateTimeOffset(lastModifiedDate).ToUnixTimeSeconds());
    Console.WriteLine(ifModifiedSince?.ToUnixTimeSeconds() >= new DateTimeOffset(lastModifiedDate).ToUnixTimeSeconds());
    
    // a bit of grace time
    if (ifModifiedSince?.ToUnixTimeSeconds() >= new DateTimeOffset(lastModifiedDate).ToUnixTimeSeconds())
    {
        ++backend304Hits;
        // let's us count the number of cache hits on the backend. The number in the client should only change when varnish doesn't get a 304 back...
        context.Response.StatusCode = 304;
        return;
    }
    
    Thread.Sleep(1500);
    ++backendHits;

    await context.Response.WriteAsJsonAsync(new
    {
        date = lastModifiedDate,
        info = "To Change this, restart the web server. lool."
    });
    
    
});

app.Run();
