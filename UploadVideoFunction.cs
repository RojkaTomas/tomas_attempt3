using Azure.Storage.Blobs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public static class UploadVideoFunction
{
    [Function("UploadVideo")]
    public static async Task<HttpResponseData> UploadVideo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UploadVideo")] HttpRequestData req,
        FunctionContext context)
    {
        var logger = context.GetLogger("UploadVideo");
        logger.LogInformation("Video upload initiated.");

        try
        {
            // Parse the request body for file data
            var boundary = req.Headers.GetValues("Content-Type").FirstOrDefault()?.Split("boundary=")?.Last();
            if (string.IsNullOrEmpty(boundary))
            {
                logger.LogError("No boundary found in the request.");
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid form-data request.");
                return badRequestResponse;
            }

            var formDataReader = new MultipartReader(boundary, req.Body);
            var section = await formDataReader.ReadNextSectionAsync();
            if (section == null || !section.Headers.ContainsKey("Content-Disposition"))
            {
                logger.LogError("No file found in the request.");
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("No file uploaded.");
                return badRequestResponse;
            }

            var contentDisposition = section.Headers["Content-Disposition"];
            var fileName = GetFileName(contentDisposition);

            if (string.IsNullOrEmpty(fileName))
            {
                logger.LogError("No valid file name found in the uploaded file.");
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid file.");
                return badRequestResponse;
            }

            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobContainerName = "uploads"; // Ensure the container exists in Azure
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);

            // Ensure the container exists
            await blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = blobContainerClient.GetBlobClient(fileName);

            using (var fileStream = section.Body)
            {
                await blobClient.UploadAsync(fileStream, true);
            }

            logger.LogInformation($"File '{fileName}' uploaded successfully.");
            var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await successResponse.WriteStringAsync($"File '{fileName}' uploaded successfully.");
            return successResponse;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error uploading video: {ex.Message}");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred while uploading the video.");
            return errorResponse;
        }
    }

    private static string GetFileName(string contentDisposition)
    {
        var fileNameKey = "filename=";
        var fileNameIndex = contentDisposition.IndexOf(fileNameKey, StringComparison.OrdinalIgnoreCase);
        if (fileNameIndex >= 0)
        {
            var startIndex = fileNameIndex + fileNameKey.Length;
            var endIndex = contentDisposition.IndexOf(';', startIndex);
            endIndex = endIndex == -1 ? contentDisposition.Length : endIndex;

            // Trim the filename and remove quotes if any
            return contentDisposition[startIndex..endIndex].Trim('"');
        }
        return null;
    }
}
