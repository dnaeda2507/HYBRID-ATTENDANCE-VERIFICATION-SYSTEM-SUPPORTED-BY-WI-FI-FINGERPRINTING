using CleanArchitecture.Application.Features.Lectures.Commands.DeleteLecture;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Lectures;
using Moq;

namespace CleanArchitecture.UnitTests.Lectures
{
    public class DeleteLectureCommandHandler_Tests
    {
        private readonly Mock<ILectureRepositoryAsync> _lectureRepositoryMock;
        private readonly DeleteLectureCommandHandler _handler;

        public DeleteLectureCommandHandler_Tests()
        {
            _lectureRepositoryMock = new Mock<ILectureRepositoryAsync>();
            _handler = new DeleteLectureCommandHandler(_lectureRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidId_DeletesLecture()
        {
            // Arrange
            var command = new DeleteLectureCommand { Id = 1 };
            var lecture = new Lecture { Id = 1 };

            _lectureRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id))
                .ReturnsAsync(lecture);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(lecture.Id, result);
            _lectureRepositoryMock.Verify(repo => repo.DeleteAsync(lecture), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var command = new DeleteLectureCommand { Id = 1 };

            _lectureRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id))
                .ReturnsAsync((Lecture)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
            _lectureRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Lecture>()), Times.Never);
        }

    }
}