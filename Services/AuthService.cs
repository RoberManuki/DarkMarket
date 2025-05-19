using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace DarkMarket.Services
{
    public class AuthService
    {
        private readonly CustomAuthStateProvider _authProvider;

        public AuthService(CustomAuthStateProvider authProvider)
        {
            _authProvider = authProvider;
        }

        public Task<bool> Login(string username, string password)
        {
            var success = username == "admin" && password == "password";
            if (success)
                _authProvider.MarkUserAsAuthenticated(username);
            return Task.FromResult(success);
        }

        public Task Logout()
        {
            _authProvider.MarkUserAsLoggedOut();
            return Task.CompletedTask;
        }

        public Task<bool> Register(string username, string password)
        {
            // Implement registration logic here
            return Task.FromResult(true);
        }
    }
}