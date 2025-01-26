using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

public static class StaticContentFunction
{
    [Function("HomePage")]
    public static async Task<HttpResponseData> HomePage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html");
        var html = File.ReadAllText("wwwroot/index.html");
        await response.WriteStringAsync(html);
        return response;
    }
}
