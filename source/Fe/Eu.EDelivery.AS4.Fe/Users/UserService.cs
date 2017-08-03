using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Eu.EDelivery.AS4.Fe.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Eu.EDelivery.AS4.Fe.Users
{
    /// <summary>
    /// UserService
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Users.IUserService" />
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly MapperConfiguration mapperConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="context">The context.</param>
        /// <param name="mapperConfiguration">The mapper configuration.</param>
        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, MapperConfiguration mapperConfiguration)
        {
            this.userManager = userManager;
            this.context = context;
            this.mapperConfiguration = mapperConfiguration;
        }

        /// <summary>
        /// Get a list of all users
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<User>> Get()
        {
            return await context.Users.ProjectTo<User>(mapperConfiguration).ToListAsync();
        }

        /// <summary>
        /// Creates the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public async Task Create(NewUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(user.Name)) throw new ArgumentException(@"Name field cannot be empty", nameof(user.Name));
            if (string.IsNullOrEmpty(user.Password)) throw new ArgumentException(@"Password field cannot be empty", nameof(user.Password));

            var result = await userManager.CreateAsync(new ApplicationUser
            {
                UserName = user.Name
            }, user.Password);

            if (result.Succeeded == false)
            {
                if (result.Errors.Any(err => err.Code == "DuplicateUserName")) throw new BusinessException(@"User already exists");
                throw new BusinessException(@"Could not create the new user please check that all requirements are met!");
            }

            var newUser = await userManager.FindByNameAsync(user.Name);
            if (user.IsAdmin) await userManager.AddClaimsAsync(newUser, new[] { new Claim(ClaimTypes.Role, Roles.Admin) });
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">user</exception>
        /// <exception cref="BusinessException"></exception>
        /// <exception cref="ArgumentException">Name cannot be empty - Name
        /// or
        /// Password cannot be empty - Password</exception>
        public async Task Update(string userName, UpdateUser user)
        {
            if (userName == null) throw new ArgumentNullException(nameof(userName));
            if (user == null) throw new ArgumentNullException(nameof(user));

            var existingUser = await userManager.FindByNameAsync(userName);
            if (existingUser == null) throw new BusinessException($"Could not found user {userName}");
            var claims = await userManager.GetClaimsAsync(existingUser);
            if (!string.IsNullOrEmpty(user.Password))
            {
                await userManager.RemovePasswordAsync(existingUser);
                await userManager.AddPasswordAsync(existingUser, user.Password);
            }
            await userManager.RemoveClaimsAsync(existingUser, claims);
            if (user.IsAdmin) await userManager.AddClaimAsync(existingUser, new Claim(ClaimTypes.Role, Roles.Admin));
            else await userManager.AddClaimAsync(existingUser, new Claim(ClaimTypes.Role, Roles.Readonly));
        }

        /// <summary>
        /// Deletes the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">user</exception>
        /// <exception cref="BusinessException"></exception>
        public async Task Delete(string user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var existingUser = await userManager.FindByNameAsync(user);
            if (existingUser == null) throw new BusinessException($"User {user} doesn't exist");
            await userManager.DeleteAsync(existingUser);
        }
    }
}