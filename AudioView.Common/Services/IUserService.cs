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
    }
}
