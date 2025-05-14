using System.Collections.Generic;
using System.Threading.Tasks;
using DarkMarket.Models;

namespace DarkMarket.Data
{
    public class UserService
    {
        private readonly List<User> users = new List<User>();

        public Task<User?> GetUserByIdAsync(int id)
        {
            var user = users.Find(u => u.Id == id);
            return Task.FromResult(user);
        }

        public Task<List<User>> GetAllUsersAsync()
        {
            return Task.FromResult(users);
        }

        public Task AddUserAsync(User user)
        {
            users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateUserAsync(User user)
        {
            var existingUser = users.Find(u => u.Id == user.Id);
            if (existingUser != null)
            {
                existingUser.Username = user.Username;
                existingUser.Password = user.Password; // In a real application, use hashed passwords
            }
            return Task.CompletedTask;
        }

        public Task DeleteUserAsync(int id)
        {
            var user = users.Find(u => u.Id == id);
            if (user != null)
            {
                users.Remove(user);
            }
            return Task.CompletedTask;
        }
    }
}