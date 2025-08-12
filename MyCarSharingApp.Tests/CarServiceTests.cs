using Microsoft.Extensions.Logging;
using Moq;
using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Application.Interfaces;
using MyCarSharingApp.Application.Mappers;
using MyCarSharingApp.Application.Services;
using MyCarSharingApp.Domain.Entities;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MyCarSharingApp.Tests
{
    public class CarServiceTests
    {
        private readonly Mock<ICarRepository> _carRepositoryMock;
        private readonly Mock<ICarMapper> _mapperMock;
        private readonly Mock<ILogger<CarService>> _loggerMock;
        private readonly CarService _carService;

        public CarServiceTests()
        {
            _carRepositoryMock = new Mock<ICarRepository>();
            _mapperMock = new Mock<ICarMapper>();
            _loggerMock = new Mock<ILogger<CarService>>();

            // CarService(ICarRepository repository, ICarMapper mapper, ILogger<CarService> logger)
            _carService = new CarService(_carRepositoryMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task AddNewCarAsync_ShouldAddCar_WhenDataIsValid()
        {
            // Arrange
            var carDto = new CarRequestDto { Brand = "Toyota", Model = "Corolla", Type = "Sedan" };
            var carEntity = new Car
            {
                Id = 1,
                Brand = "Toyota",
                Model = "Corolla",
                Type = CarType.Sedan,
                Inventory = 0
            };

            _mapperMock.Setup(m => m.ToEntity(carDto)).Returns(carEntity);
            _mapperMock.Setup(m => m.ToDto(It.IsAny<Car>())).Returns(new CarResponseDto
            {
                Id = carEntity.Id,
                Brand = carEntity.Brand,
                Model = carEntity.Model,
                Type = "Sedan"
            });

            _carRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Car>())).ReturnsAsync(carEntity);

            // Act
            var result = await _carService.AddNewCarAsync(carDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Toyota", result.Brand);
            _carRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Once);
            _mapperMock.Verify(m => m.ToEntity(carDto), Times.Once);
            _mapperMock.Verify(m => m.ToDto(It.IsAny<Car>()), Times.Once);
        }

        [Fact]
        public async Task GetCarByIdAsync_ShouldReturnNull_WhenCarDoesNotExist()
        {
            // Arrange
            _carRepositoryMock.Setup(r => r.FindByIdAsync(It.IsAny<int>()))
                              .ReturnsAsync((Car?)null);

            // Act
            var ex = await Assert.ThrowsAsync<MyCarSharingApp.Application.Exceptions.EntityNotFoundException>(
            () => _carService.GetCarByIdAsync(999999));

            // Assert
            Assert.Contains("Can't find a car", ex.Message);
            _carRepositoryMock.Verify(r => r.FindByIdAsync(999999), Times.Once);
        }

        [Fact]
        public async Task DeleteCarAsync_ShouldThrowException_WhenCarNotFound()
        {
            // Arrange
            _carRepositoryMock.Setup(r => r.FindByIdAsync(It.IsAny<int>()))
                              .ReturnsAsync((Car?)null);

            // Act & Assert
            await Assert.ThrowsAsync<MyCarSharingApp.Application.Exceptions.EntityNotFoundException>(async () =>
                await _carService.DeleteCarByIdAsync(999999));

            _carRepositoryMock.Verify(r => r.FindByIdAsync(999999), Times.Once);
        }
    }
}