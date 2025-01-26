using Azure.Storage;
using Azure.Storage.Sas;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

public static class GetSasToken
{
    [Function("GetSasToken")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var log = executionContext.GetLogger("GetSasToken");
        log.LogInformation("Generating SAS token.");

        string blobContainerName = "uploads"; // Replace with your container name
        string storageAccountName = "teamproject1"; // Replace with your storage account name
        string storageAccountKey = Environment.GetEnvironmentVariable("AzureWebJobsStorageAccountKey");

        // Validate the storage key
        if (string.IsNullOrEmpty(storageAccountKey))
        {
            log.LogError("AzureWebJobsStorageAccountKey environment variable is not set.");
            var badResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await badResponse.WriteStringAsync("Storage account key is missing.");
            return badResponse;
        }

        // Create SAS token builder
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobContainerName,
            StartsOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddHours(1)
        };

        // Grant write permissions
        sasBuilder.SetPermissions(BlobContainerSasPermissions.Write | BlobContainerSasPermissions.Read);
;

        // Generate the SAS token
        var sasToken = sasBuilder.ToSasQueryParameters(
            new StorageSharedKeyCredential(storageAccountName, storageAccountKey)
        ).ToString();

        // Build the full URL with SAS token
        string fullSasUrl = $"https://{storageAccountName}.blob.core.windows.net/{blobContainerName}?{sasToken}";

        // Return the SAS URL
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(fullSasUrl); // Use async method
        return response;
    }
}
