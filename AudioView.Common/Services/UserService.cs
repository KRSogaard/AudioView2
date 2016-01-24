using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.DataAccess;
using System.Security.Cryptography;
using AudioView.Common.Data;
using User = AudioView.Common.Data.User;

namespace AudioView.Common.Services
{
    public class UserService : IUserService
    {
        public async Task<User> Validate(string username, string password)
        {
            using (var audioViewEntities = new AudioViewEntities())
            {
                var user = await audioViewEntities.Users.FirstOrDefaultAsync(x => x.username.ToLower() == username.ToLower());
                if (user == null)
                    return null;

                var hash = generateHash(password, user.passwordSalt);
                if (user.password != hash)
                    return null;

                return user.ToInternal();
            }
        }

        public async Task<List<User>> GetUsers()
        {
            using (var audioViewEntities = new AudioViewEntities())
            {
                var result = await audioViewEntities.Users.OrderBy(x => x.username).ToListAsync();
                return result.Select(x => x.ToInternal()).ToList();
            }
        }

        public async Task<User> GetUser(string username)
        {
            using (var audioViewEntities = new AudioViewEntities())
            {
                return (await audioViewEntities.Users.FirstOrDefaultAsync(x => x.username.ToLower() == username.ToLower()))?.ToInternal();
            }
        }

        public async Task CreateUser(User user, string password)
        {
            using (var audioViewEntities = new AudioViewEntities())
            {
                var userObject = user.ToDatabase();
                userObject.passwordSalt = getRandomSalt();
                userObject.password = generateHash(password, userObject.passwordSalt);
                audioViewEntities.Users.Add(userObject);
                await audioViewEntities.SaveChangesAsync();
            }
        }

        public async Task UpdatePassword(string username, string newPassword)
        {
            using (var audioViewEntities = new AudioViewEntities())
            {
                var user = await audioViewEntities.Users.FirstOrDefaultAsync(x => x.username.ToLower() == username.ToLower());
                if (user == null)
                    return;
                user.passwordSalt = getRandomSalt();
                user.password = generateHash(newPassword, user.passwordSalt);
                await audioViewEntities.SaveChangesAsync();
            }
        }

        public async Task DeleteUser(string username)
        {
            using (var audioViewEntities = new AudioViewEntities())
            {
                var user = await audioViewEntities.Users.FirstOrDefaultAsync(x => x.username.ToLower() == username.ToLower());
                if (user == null)
                    return;
                audioViewEntities.Users.Remove(user);
                await audioViewEntities.SaveChangesAsync();
            }
        }

        private string generateHash(string password, int salt)
        {
            using (var sha1 = new SHA1Managed())
            {
                return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(salt + password)));
            }
        }

        public int getRandomSalt()
        {
            var rnd = new Random();
            return rnd.Next(10000000, 99999999);
        }
    }
}
