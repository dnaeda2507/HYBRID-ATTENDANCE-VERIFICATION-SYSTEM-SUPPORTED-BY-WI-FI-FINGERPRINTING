using AutoMapper;
using CleanArchitecture.Application.Features.Users.Commands.UpdateUser;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CleanArchitecture.UnitTests.Users
{
    public class UpdateUserCommandHandler_Tests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateUserCommandHandler _handler;

        public UpdateUserCommandHandler_Tests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _mapperMock = new Mock<IMapper>();

            _handler = new UpdateUserCommandHandler(_userManagerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsApiException()
        {
            // Arrange
            var email = "test@example.com";
            var command = new UpdateUserCommand
            {
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                Roles = new List<Roles> { Roles.Student }
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(() =>
                _handler.Handle(command, CancellationToken.None));
            Assert.Equal($"User with email {email} not found.", exception.Message);
        }

        [Fact]
        public async Task Handle_UpdateUser_Success_ReturnsSuccessMessage()
        {
            // Arrange
            var email = "test@example.com";
            var command = new UpdateUserCommand
            {
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                Roles = new List<Roles> { Roles.Student }
            };

            var user = new ApplicationUser { Email = email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _mapperMock.Setup(m => m.Map(command, user))
                .Callback<UpdateUserCommand, ApplicationUser>((src, dest) =>
                {
                    dest.FirstName = src.FirstName;
                    dest.LastName = src.LastName;
                });

            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal($"User {email} updated successfully.", result);
            _mapperMock.Verify(m => m.Map(command, user), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateUser_Failure_ThrowsApiException()
        {
            // Arrange
            var email = "test@example.com";
            var command = new UpdateUserCommand
            {
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                Roles = new List<Roles> { Roles.Student }
            };

            var user = new ApplicationUser { Email = email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            _userManagerMock.Setup(x => x.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            var identityError = new IdentityError { Code = "UpdateFailed", Description = "Error updating user." };
            var failedResult = IdentityResult.Failed(identityError);

            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(failedResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(() =>
                _handler.Handle(command, CancellationToken.None));
            Assert.Contains(identityError.Description, exception.Message);
        }
    }
}