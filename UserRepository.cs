using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository()
    {
        _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
    }

    public async Task AddUserAsync(User newUser)
        {
            using SqlConnection sqlConnection = new SqlConnection(_connectionString);
            await sqlConnection.OpenAsync();

            string query = "INSERT INTO Users (Username, PasswordHash, Role) VALUES (@Username, @PasswordHash, @Role)";
            using SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@Username", newUser.Username);
            sqlCommand.Parameters.AddWithValue("@PasswordHash", newUser.PasswordHash);
            sqlCommand.Parameters.AddWithValue("@Role", newUser.Role);

            await sqlCommand.ExecuteNonQueryAsync();
        }


    public async Task<bool> CheckUserExistsAsync(string username)
    {
        using SqlConnection sqlConnection = new SqlConnection(_connectionString);
        await sqlConnection.OpenAsync();

        string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
        using SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
        sqlCommand.Parameters.AddWithValue("@Username", username);

        int userCount = (int)await sqlCommand.ExecuteScalarAsync();
        return userCount > 0;
    }

}
