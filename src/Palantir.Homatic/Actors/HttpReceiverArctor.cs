using Microsoft.Extensions.Logging;
using Proto;
using System.Net.Http.Json;

namespace Palantir.Homatic.Actors;

public class HttpReceiverArctor : IActor
{
    private readonly HttpClient http;
    private readonly ILogger<HttpReceiverArctor> logger;

    public HttpReceiverArctor(HttpClient http, ILogger<HttpReceiverArctor> logger)
    {
        this.http = http ?? throw new ArgumentNullException(nameof(http));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            this.logger.LogInformation($"Http receiver started from '{context.Parent?.Address}'");
        }
        if (context.Message is HttpRequest request)
        {
            var result = await this.http.GetAsync(request.Uri);

            if (result.IsSuccessStatusCode)
            {
                var resultInstance = await result.Content.ReadFromJsonAsync(request.ResultType, context.CancellationToken);

                context.Respond(new HttpResponse(request.Uri, resultInstance));
            }
        }
        if (context.Message is Stopped)
        {
            this.logger.LogInformation($"Http receiver stopped from '{context.Parent?.Address}'");
        }
    }
}

public record HttpRequest(Uri Uri, Type ResultType);

public record HttpResponse(Uri Uri, object Result);
