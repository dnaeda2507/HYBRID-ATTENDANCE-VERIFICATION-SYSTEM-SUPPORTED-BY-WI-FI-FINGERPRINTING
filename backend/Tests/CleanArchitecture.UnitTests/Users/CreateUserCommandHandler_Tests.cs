using CleanArchitecture.Application.Features.Users.Commands.CreateUser;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CleanArchitecture.UnitTests.Users
{
    public class CreateUserCommandHandler_Tests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandler_Tests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _handler = new CreateUserCommandHandler(_userManagerMock.Object);
        }

        [Fact]
        public async Task Handle_CreateUser_Success_ReturnsRegisteredMessage()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Password = "Password123!",
                Roles = new List<Roles> { Roles.Student }
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("User Registered.", result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(command.Email), Times.Once);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password), Times.Once);
            foreach (var role in command.Roles)
            {
                _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), role.ToString()), Times.Once);
            }
        }

        [Fact]
        public async Task Handle_EmailAlreadyRegistered_ThrowsApiException()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "existing@example.com",
                FirstName = "Jane",
                LastName = "Doe",
                Password = "Password123!",
                Roles = new List<Roles> { Roles.Student }
            };

            var existingUser = new ApplicationUser { Email = command.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
                .ReturnsAsync(existingUser);

            var exception = await Assert.ThrowsAsync<ApiException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal($"An error occurred while creating the user: Email {command.Email} is already registered.", exception.Message);
        }

        [Fact]
        public async Task Handle_CreateUserFailure_ThrowsApiException()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "fail@example.com",
                FirstName = "Alice",
                LastName = "Smith",
                Password = "Password123!",
                Roles = new List<Roles> { Roles.Student }
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
                .ReturnsAsync((ApplicationUser)null);

            var identityError = new IdentityError { Code = "CreateFailed", Description = "Failed to create user." };
            var failedResult = IdentityResult.Failed(identityError);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                .ReturnsAsync(failedResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(() =>
                _handler.Handle(command, CancellationToken.None));
            Assert.Equal($"An error occurred while creating the user: {identityError.ToString()}", exception.Message);
        }

        [Fact]
        public async Task Handle_ExceptionThrown_ThrowsException()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "error@example.com",
                FirstName = "Bob",
                LastName = "Brown",
                Password = "Password123!",
                Roles = new List<Roles> { Roles.Student }
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
                .ReturnsAsync((ApplicationUser)null);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
                .ThrowsAsync(new Exception("unexpected error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(command, CancellationToken.None));
            Assert.Equal("An error occurred while creating the user: unexpected error", exception.Message);
        }
    }
}