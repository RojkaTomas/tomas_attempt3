using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class BlobFunction
{
    private readonly ILogger _logger;

    public BlobFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<BlobFunction>();
    }

    [Function("BlobTriggerFunction")]
    public void Run([BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] string blobContent, string name)
    {
        _logger.LogInformation($"Blob trigger executed for file: {name}");
    }
}
