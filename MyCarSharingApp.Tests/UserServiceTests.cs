using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using MyCarSharingApp.Application.Services;
using Xunit;

namespace MyCarSharingApp.Tests
{
    public class IdentityUserServiceTests
    {
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<SignInManager<IdentityUser>> _mockSignInManager;
        private Mock<ILogger<IdentityUserService>> _mockLogger;
        private IdentityUserService _service;

        public IdentityUserServiceTests()
        {
            var userStore = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            _mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                _mockUserManager.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                null, null, null, null);

            _mockLogger = new Mock<ILogger<IdentityUserService>>();
            _service = new IdentityUserService(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUserAndAssignRole()
        {
            // Arrange
            var user = new IdentityUser { UserName = "test", Email = "t@e.com" };
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), "pass"))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.RegisterAsync("test", "t@e.com", "pass");

            // Assert
            Assert.Equal("test", result.UserName);
            Assert.Equal("t@e.com", result.Email);
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<IdentityUser>(), "pass"), Times.Once);
            _mockUserManager.Verify(x => x.AddToRoleAsync(result, "User"), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnUser_WhenPasswordValid()
        {
            // Arrange
            var email = "t@e.com";
            var password = "pass";
            var user = new IdentityUser { UserName = "test", Email = email };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);
            _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
                .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _service.AuthenticateAsync(email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((IdentityUser)null);

            // Act
            var result = await _service.AuthenticateAsync("x", "y");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenPasswordInvalid()
        {
            // Arrange
            var email = "t@e.com";
            var user = new IdentityUser { Email = email };
            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);
            _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "wrong", false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _service.AuthenticateAsync(email, "wrong");

            // Assert
            Assert.Null(result);
        }
    }
}