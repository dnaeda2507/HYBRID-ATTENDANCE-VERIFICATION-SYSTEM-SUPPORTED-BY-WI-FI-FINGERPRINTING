using AutoMapper;
using CleanArchitecture.Application.Features.Lectures.Commands.UpdateLecture;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Lectures;
using Moq;

namespace CleanArchitecture.UnitTests.Lectures
{
    public class UpdateLectureCommandHandler_Tests
    {
        private readonly Mock<ILectureRepositoryAsync> _lectureRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateLectureCommandHandler _handler;

        public UpdateLectureCommandHandler_Tests()
        {
            _lectureRepositoryMock = new Mock<ILectureRepositoryAsync>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateLectureCommandHandler(_lectureRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsLectureId()
        {
            // Arrange
            var command = new UpdateLectureCommand { Id = 1, Name = "Updated Lecture", Code = "CSE123", Description = "Updated Description" };
            var lecture = new Lecture { Id = 1, Name = "Old Lecture", Code = "CSE123", Description = "Old Description" };

            _lectureRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(lecture);
            _mapperMock.Setup(m => m.Map(command, lecture)).Returns(lecture);
            _lectureRepositoryMock.Setup(repo => repo.UpdateAsync(lecture)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(command.Id, result);
            _lectureRepositoryMock.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
            _mapperMock.Verify(m => m.Map(command, lecture), Times.Once);
            _lectureRepositoryMock.Verify(repo => repo.UpdateAsync(lecture), Times.Once);
        }

        [Fact]
        public async Task Handle_LectureNotFound_ThrowsArgumentException()
        {
            // Arrange
            var command = new UpdateLectureCommand { Id = 1, Name = "Updated Lecture", Code = "CSE123", Description = "Updated Description" };

            _lectureRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync((Lecture)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}