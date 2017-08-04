using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class UserServiceTests
    {
        protected UserManager<ApplicationUser> UserManager { get; set; }
        protected ApplicationDbContext DbContext { get; set; }
        protected UserService UserService { get; set; }

        protected virtual async Task<UserServiceTests> Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var store = new ApplicationDbContext(options))
            {
                store.Database.EnsureCreated();
            }

            var mapperConfig = new MapperConfiguration(cfg => { cfg.AddProfile<UsersAutoMapperProfile>(); });

            var services = new ServiceCollection();
            services
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ApplicationDbContext>(dbOptions => dbOptions.UseInMemoryDatabase())
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            var provider = services.BuildServiceProvider();

            UserManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            DbContext = provider.GetRequiredService<ApplicationDbContext>();
            UserService = new UserService(provider.GetRequiredService<UserManager<ApplicationUser>>(), provider.GetRequiredService<ApplicationDbContext>(), mapperConfig);

            return await Task.FromResult(this);
        }

        public class Get : UserServiceTests
        {
            [Fact]
            public async Task Calls_ApplicationDbContext()
            {
                await Setup();

                DbContext.Users.Add(new ApplicationUser
                {
                    UserName = "test"
                });
                DbContext.SaveChanges();

                var result = (await UserService.Get()).ToList();

                Assert.True(result.First(x => x.Name == "test").Name == "test", "Expected user to have name 'test'");
            }
        }

        public class Create : UserServiceTests
        {
            [Fact]
            public async Task Creates_User()
            {
                await Setup();
                var username = Guid.NewGuid().ToString();

                await UserService.Create(new NewUser
                {
                    Name = username,
                    Password = "CZ#$So7OGoNb",
                    Roles = new[] { Roles.Admin }
                });

                var user = (await UserService.Get()).ToList();

                var search = user.FirstOrDefault(x => x.Name == username);
                Assert.NotNull(search);
                Assert.True(search.Name == username, $"Expected the created user to have '{username}' as username!");
                Assert.True(search.Roles.Contains(Roles.Admin), "Expected the user to be an admin!");
            }

            [Fact]
            public async Task CreatesUserWithReadonlyClaim_WhenIsAdminIsFalse()
            {
                var username = Guid.NewGuid().ToString();
                await Setup();
                await UserService.Create(new NewUser
                {
                    Name = username,
                    Password = "CZ#$So7OGoNb"
                });

                var user = (await UserService.Get()).ToList();

                var search = user.FirstOrDefault(name => name.Name == username);
                Assert.NotNull(search);
                Assert.True(search.Name == username, "Expected the created user to have 'test123' as username!");
                Assert.False(search.Roles.Contains(Roles.Admin), "Expected the user to be not be an admin!");
            }

            [Fact]
            public async Task ThrowsException_WhenParametersAreInvalid()
            {
                await Setup();

                await Assert.ThrowsAsync<ArgumentNullException>(() => UserService.Create(null));
                await Assert.ThrowsAsync<ArgumentException>(() => UserService.Create(new NewUser()));
                await Assert.ThrowsAsync<ArgumentException>(() => UserService.Create(new NewUser { Name = "test" }));
            }

            [Fact]
            public async Task ThrowsException_WhenPasswordSettingsDoNotMeetRequirements()
            {
                await Setup();

                await Assert.ThrowsAsync<BusinessException>(() => UserService.Create(new NewUser
                {
                    Name = "test",
                    Password = "test"
                }));
            }
        }

        [Collection("Update")]
        public class Update : UserServiceTests
        {
            private string user;

            protected override async Task<UserServiceTests> Setup()
            {
                user = Guid.NewGuid().ToString();
                var result = await base.Setup();
                await UserService.Create(new NewUser
                {
                    Name = user,
                    Password = "CZ#$So7OGoNb",
                    Roles = new[] { Roles.Admin }
                });
                return result;
            }

            [Fact]
            public async Task Updates_ExistingUser()
            {
                await Setup();

                await UserService.Update(this.user, new UpdateUser { Password = "9*SC!7i*wH3r" });

                var user = await UserManager.FindByNameAsync(this.user);
                var claims = await UserManager.GetClaimsAsync(user);
                var result = await UserManager.CheckPasswordAsync(user, "9*SC!7i*wH3r");
                Assert.True(result, "CheckPasswordAsync should have returned true");
                Assert.False(claims.Any(claim => claim.Value == Roles.Admin));
            }

            [Fact]
            public async Task DoesntUpdatePassword_WhenPasswordIsEmpty()
            {
                await Setup();

                await UserService.Update(this.user, new UpdateUser());

                var user = await UserManager.FindByNameAsync(this.user);
                var claims = await UserManager.GetClaimsAsync(user);
                var result = await UserManager.CheckPasswordAsync(user, "CZ#$So7OGoNb");
                Assert.True(result, "CheckPasswordAsync should have returned true");
            }

            [Fact]
            public async Task ThrowsBusinessException_WhenUserDoesntExist()
            {
                await Setup();

                await Assert.ThrowsAsync<BusinessException>(() => UserService.Update("fdsqfdfdsqfezrrzeaerzaerz", new UpdateUser()));
            }
        }

        public class Delete : UserServiceTests
        {
            [Fact]
            public async Task User_IsDeleted()
            {
                var username = Guid.NewGuid().ToString();
                await Setup();

                await UserService.Create(new NewUser
                {
                    Name = username,
                    Password = "CZ#$So7OGoNb"
                });

                await UserService.Delete(username);

                var user = (await UserService.Get()).ToList();
                Assert.True(user.All(find => find.Name != username));
            }

            [Fact]
            public async Task ThrowsException_WhenUserDoesntExist()
            {
                await Setup();

                await Assert.ThrowsAsync<BusinessException>(() => UserService.Delete("fdsqfdsqfdsqfeaq"));
            }
        }
    }
}