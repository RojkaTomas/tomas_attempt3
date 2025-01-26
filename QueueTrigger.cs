using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Howest.Functions
{
    public class QueueFunctions
    {
        private readonly ILogger _logger;

        public QueueFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<QueueFunctions>();
        }

        [Function("QueueOption1")]
        public void RunOption1([QueueTrigger("option1queue", Connection = "AzureWebJobsStorage")] string message)
        {
            _logger.LogInformation($"Option 1 triggered with message: {message}");
        }
    }

}
