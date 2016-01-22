using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.Common.Services
{
    public class UserService : IUserService
    {
        public Task<User> Validate(string username, string password)
        {
            return Task.Factory.StartNew(() =>
            {
                return new User();
            });
        }
    }
}
