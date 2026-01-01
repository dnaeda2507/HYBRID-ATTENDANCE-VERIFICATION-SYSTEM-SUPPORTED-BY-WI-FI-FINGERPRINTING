using AutoMapper;
using CleanArchitecture.Application.Features.Users.Commands.DeleteUser;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CleanArchitecture.UnitTests.Users
{
    public class DeleteUserCommandHandler_Tests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly DeleteUserCommandHandler _handler;

        public DeleteUserCommandHandler_Tests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _mapperMock = new Mock<IMapper>();

            _handler = new DeleteUserCommandHandler(_userManagerMock.Object);
        }

        [Fact]
        public async Task Handle_UserExists_DeletesUser()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var command = new DeleteUserCommand { Id = id };

            _userManagerMock.Setup(x => x.FindByIdAsync(command.Id))
                .ReturnsAsync(new ApplicationUser { Id = command.Id });

            _userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.Equal($"User with ID {id} deleted successfully.", result);
        }

        [Fact]
        public async Task Handle_UserDoesNotExist_ThrowsApiException()
        {
            // Arrange
            var command = new DeleteUserCommand { Id = Guid.NewGuid().ToString() };

            _userManagerMock.Setup(x => x.FindByIdAsync(command.Id))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => _handler.Handle(command, default));
        }
    }
}