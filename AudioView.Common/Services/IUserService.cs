using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.Common.Services
{
    public interface IUserService
    {
        Task<User> Validate(string username, string password);
        Task<List<User>> GetUsers();
        Task<User> GetUser(string username);
        Task CreateUser(User user, string password);
        Task UpdatePassword(string username, string newPassword);
        Task UpdateExpires(string username, DateTime? expires);
        Task DeleteUser(string username);
    }
}
