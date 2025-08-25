using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HealthConnect
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class UserProfileMiddleware
    {
        private readonly RequestDelegate _next;

        public UserProfileMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class UserProfileMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserProfileMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserProfileMiddleware>();
        }
    }
}
