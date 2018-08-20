using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Fe.Users
{
    /// <summary>
    /// Interface to be implemented be a service to manager users.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get a list of all users
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<User>> Get();

        /// <summary>
        /// Creates the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        Task Create(NewUser user);

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">user</exception>
        /// <exception cref="ArgumentException">Name cannot be empty - Name
        /// or
        /// Password cannot be empty - Password</exception>
        Task Update(string userName, UpdateUser user);

        /// <summary>
        /// Deletes the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">user</exception>
        /// <exception cref="BusinessException"></exception>
        Task Delete(string user);
    }
}