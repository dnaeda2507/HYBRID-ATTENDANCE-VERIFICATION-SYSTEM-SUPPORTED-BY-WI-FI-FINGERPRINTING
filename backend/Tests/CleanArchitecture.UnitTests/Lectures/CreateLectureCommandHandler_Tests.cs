using AutoMapper;
using CleanArchitecture.Application.Features.Lectures.Commands.CreateLecture;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Lectures;
using Moq;

namespace CleanArchitecture.UnitTests.Lectures
{
    public class CreateLectureCommandHandler_Tests
    {
        private readonly Mock<ILectureRepositoryAsync> _lectureRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateLectureCommandHandler _handler;
        public CreateLectureCommandHandler_Tests()
        {
            _lectureRepositoryMock = new Mock<ILectureRepositoryAsync>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateLectureCommandHandler(_lectureRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldAddLecture_WhenCommandIsValid()
        {
            // Arrange
            var command = new CreateLectureCommand
            {
                Name = "Test Lecture",
                Code = "CSE123",
                Description = "Test Description",
            };

            var lectureId = 1;
            _mapperMock.Setup(m => m.Map<Lecture>(It.IsAny<CreateLectureCommand>()))
                .Returns(new Lecture { Id = lectureId });
            _lectureRepositoryMock.Setup(m => m.AddAsync(It.IsAny<Lecture>()))
                .ReturnsAsync(new Lecture { Id = lectureId });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(lectureId, result);
            _lectureRepositoryMock.Verify(m => m.AddAsync(It.IsAny<Lecture>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenCommandIsInvalid()
        {
            // Arrange
            var command = new CreateLectureCommand
            {
                Name = "",
                Code = "CSE123",
                Description = "Test Description",
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenDescriptionExceedsMaxLength()
        {
            // Arrange
            var command = new CreateLectureCommand
            {
                Name = "Test Lecture",
                Code = "CSE123",
                Description = new string('a', 501), // Exceeds max length
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}