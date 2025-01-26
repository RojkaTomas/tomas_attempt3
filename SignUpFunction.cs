using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

public class SignUpFunction
{
    private readonly ILogger _logger;
    private readonly UserRepository _userRepository;

    public SignUpFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SignUpFunction>();
        _userRepository = new UserRepository(); // Initialize the repository
    }

    [Function("Signup")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Signup")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("SignUpFunction invoked.");

            // Read and log the request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"Request Body: {requestBody}");

            var newUser = JsonSerializer.Deserialize<User>(requestBody);
            if (newUser == null || string.IsNullOrEmpty(newUser.Username) || string.IsNullOrEmpty(newUser.PasswordHash) || string.IsNullOrEmpty(newUser.Role))
            {
                _logger.LogWarning("Validation failed: Missing Username, PasswordHash, or Role.");
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Username, PasswordHash, and Role are required.");
                return badResponse;
            }

            // Check if the user already exists
            if (await _userRepository.CheckUserExistsAsync(newUser.Username))
            {
                _logger.LogWarning($"User with username {newUser.Username} already exists.");
                var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
                await conflictResponse.WriteStringAsync("User already exists.");
                return conflictResponse;
            }

            // Add the new user
            await _userRepository.AddUserAsync(newUser);
            _logger.LogInformation($"User {newUser.Username} successfully added.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("User successfully signed up.");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in SignUpFunction: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred.");
            return errorResponse;
        }
    }
}

public class User
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } // Include Role if needed
}

