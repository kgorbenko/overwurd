using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Overwurd.Model.Models;
using Overwurd.Model.Repositories;

namespace Overwurd.Web.Services.Auth.Stores
{
    public class UserPasswordStore : IUserPasswordStore<User>
    {
        private readonly IOverwurdRepository<User> userRepository;
        private readonly ILogger<UserPasswordStore> logger;

        public UserPasswordStore([NotNull] IOverwurdRepository<User> userRepository,
                                 [NotNull] ILogger<UserPasswordStore> logger)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Dispose() { }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Id.ToString());

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Login);

        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Password);

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Password != null);

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.Login = userName;

            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedLogin);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedLogin = normalizedName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                await userRepository.AddAsync(user, cancellationToken);

                return IdentityResult.Success;
            } catch (Exception exception)
            {
                logger.LogError(exception, "");
                return IdentityResult.Failed();
            }
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                await userRepository.UpdateAsync(user, cancellationToken);

                return IdentityResult.Success;
            } catch (Exception exception)
            {
                logger.LogError(exception, "");
                return IdentityResult.Failed();
            }
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            try
            {
                await userRepository.RemoveAsync(user, cancellationToken);

                return IdentityResult.Success;
            } catch (Exception exception)
            {
                logger.LogError(exception, "");
                return IdentityResult.Failed();
            }
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await userRepository.FindByIdAsync(long.Parse(userId), cancellationToken);
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return (await userRepository.FindByAsync(x => x.NormalizedLogin == normalizedUserName, cancellationToken)).SingleOrDefault();
        }

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.Password = passwordHash;

            return Task.CompletedTask;
        }
    }
}