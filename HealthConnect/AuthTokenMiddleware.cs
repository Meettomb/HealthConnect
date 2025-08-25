using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace HealthConnect
{
    public class AuthTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _connectionString;

        public AuthTokenMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check the authentication token
            if (!CheckAuthToken(context))
            {
                // If token is not found or invalid, don't redirect to Sign_in page
                // You can log the event or take some other action if needed
                // If needed, you can just proceed without setting user-specific session data
            }

            await _next(context); // Continue processing the request
        }

        private bool CheckAuthToken(HttpContext context)
        {
            string deviceId = context.Request.Cookies["deviceUniqueId"];
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            using SqlConnection con = new SqlConnection(_connectionString);
            string query = @"SELECT id, role, isactive 
                             FROM User_Table 
                             WHERE ',' + auth_token + ',' LIKE '%,' + @DeviceId + ',%'";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@DeviceId", deviceId);
            con.Open();
            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int userId = Convert.ToInt32(reader["id"]);
                string role = reader["role"].ToString();
                bool isActive = Convert.ToBoolean(reader["isactive"]);

                if (isActive)
                {
                    context.Session.SetInt32("Id", userId);
                    context.Session.SetString("UserRole", role);
                    return true; 
                }
            }

            return false; // Invalid or inactive token
        }
    }

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthTokenMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthTokenMiddleware>();
        }
    }
}
