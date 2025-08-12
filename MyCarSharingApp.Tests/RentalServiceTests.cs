using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Application.Interfaces;
using MyCarSharingApp.Application.Services;
using MyCarSharingApp.Domain.Entities;
using Xunit;

namespace MyCarSharingApp.Tests
{
    public class RentalServiceTests
    {
        private readonly Mock<ICarRepository> _carRepoMock;
        private readonly Mock<IRentalRepository> _rentalRepoMock;
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<ILogger<RentalService>> _loggerMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly RentalService _service;

        public RentalServiceTests()
        {
            _carRepoMock = new Mock<ICarRepository>();
            _rentalRepoMock = new Mock<IRentalRepository>();
            _userManagerMock = CreateUserManagerMock();
            _loggerMock = new Mock<ILogger<RentalService>>();
            _uowMock = new Mock<IUnitOfWork>();

            // RentalService(ICarRepository carRepo, IRentalRepository rentalRepo, UserManager<IdentityUser> userManager, ILogger<RentalService> logger, IUnitOfWork uow)
            _service = new RentalService(
                _carRepoMock.Object,
                _rentalRepoMock.Object,
                _userManagerMock.Object,
                _loggerMock.Object,
                _uowMock.Object
            );
        }

        private static Mock<UserManager<IdentityUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task RentCarAsync_ShouldSucceed_WhenCarIsAvailable()
        {
            // Arrange
            var request = new RentalRequestDto
            {
                UserId = "user1",
                CarId = 10,
                RentalDate = DateTime.UtcNow.Date,
                ReturnDate = DateTime.UtcNow.Date.AddDays(1)
            };
            
            var user = new IdentityUser { Id = "user1" };
            var car = new Car { Id = 10, Inventory = 1 };

            _userManagerMock.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync(user);
            _rentalRepoMock.Setup(r => r.GetActiveRentalByUserIdAsync("user1"))
                           .ReturnsAsync(new List<Rental>()); 
            _carRepoMock.Setup(c => c.FindByIdAsync(10)).ReturnsAsync(car);
            _carRepoMock.Setup(c => c.UpdateAsync(It.IsAny<Car>())).ReturnsAsync((Car c) => c);

            _rentalRepoMock.Setup(r => r.AddAsync(It.IsAny<Rental>()))
                           .ReturnsAsync((Rental r) => { r.Id = 123; return r; });

            // Act
            var result = await _service.RentCarAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user1", result.UserId);
            Assert.Equal(10, result.CarId);
            _carRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Once);
            _rentalRepoMock.Verify(r => r.AddAsync(It.IsAny<Rental>()), Times.Once);
        }

        [Fact]
        public async Task RentCarAsync_ShouldThrowException_WhenCarNotAvailable()
        {
            // Arrange
            var request = new RentalRequestDto
            {
                UserId = "user1",
                CarId = 999999,
                RentalDate = DateTime.UtcNow.Date,
                ReturnDate = DateTime.UtcNow.Date.AddDays(1)
            };

            var user = new IdentityUser { Id = "user1" };
            var car = new Car { Id = 999999, Inventory = 0 }; 

            _userManagerMock.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync(user);
            _rentalRepoMock.Setup(r => r.GetActiveRentalByUserIdAsync("user1"))
                           .ReturnsAsync(new List<Rental>());
            _carRepoMock.Setup(c => c.FindByIdAsync(999999)).ReturnsAsync(car);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.RentCarAsync(request));
            Assert.Contains("not available for rent", ex.Message, StringComparison.OrdinalIgnoreCase);

            // Verify 
            _carRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);
            _rentalRepoMock.Verify(r => r.AddAsync(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        public async Task SetActualReturnDateAsync_ShouldUpdateRental_WhenExists()
        {
            // Arrange
            var rentalId = 1;
            var existingRental = new Rental
            {
                Id = rentalId,
                UserId = "user1",
                ActualReturnDate = null
            };

            _rentalRepoMock.Setup(r => r.GetByIdAsync(rentalId)).ReturnsAsync(existingRental);
            _rentalRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Rental>())).ReturnsAsync(true);

            // Act
            await _service.SetActualReturnDateAsync(rentalId);

            // Assert
            _rentalRepoMock.Verify(r =>
                r.UpdateAsync(It.Is<Rental>(x => x.Id == rentalId && x.ActualReturnDate != null)),
                Times.Once);
        }
    }
}
